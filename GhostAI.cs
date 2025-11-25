using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GridBuilder gridBuilder;

    [Header("AI Settings")]
    public float chaseRange = 12f;
    public float moveSpeed = 4f;
    public float nodeReachThreshold = 0.1f;
    public float pathUpdateInterval = 0.5f;  // Update path setiap 0.5 detik
    
    [Header("Path Separation Settings")]
    public float occupiedNodePenalty = 10f;  // Penalty untuk node yang sudah dipakai ghost lain
    public float personalSpaceRadius = 1.5f; // Jarak minimum antar ghost

    [Header("States")]
    public bool isChasing = false;

    // Pathfinding
    private List<Node> currentPath;
    private int currentPathIndex = 0;
    private float lastPathUpdateTime;
    private Node currentNode = null;  // Node yang sedang ditempati

    void Start()
    {

        int difficulty = PlayerPrefs.GetInt("Difficulty", 0);

        switch (difficulty)
        {
            case 0: moveSpeed = 3f; chaseRange = 8f; break;  // Easy
            case 1: moveSpeed = 4f; chaseRange = 12f; break; // Normal
            case 2: moveSpeed = 5f; chaseRange = 15f; break; // Hard
        }

        // Auto-find references jika belum diassign
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (gridBuilder == null)
            gridBuilder = FindObjectOfType<GridBuilder>();

        if (gridBuilder == null)
            Debug.LogError("GridBuilder tidak ditemukan! Pastikan ada GameObject dengan GridBuilder script di scene.");
    }

    void Update()
    {
        if (player == null || gridBuilder == null || gridBuilder.grid == null) return;

        // Update node reservation
        UpdateNodeReservation();

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check apakah player dalam chase range
        if (distanceToPlayer <= chaseRange)
        {
            isChasing = true;

            // Update path secara periodik
            if (Time.time - lastPathUpdateTime > pathUpdateInterval)
            {
                FindPathToPlayer();
                lastPathUpdateTime = Time.time;
            }

            // Follow path
            FollowPath();
        }
        else
        {
            isChasing = false;
            ReleaseAllReservations();
            currentPath = null;
        }
    }
    
    void UpdateNodeReservation()
    {
        Node newNode = gridBuilder.GetNodeFromWorldPosition(transform.position);
        
        if (newNode != currentNode)
        {
            // Release old node
            if (currentNode != null)
            {
                currentNode.Release(this);
            }
            
            // Reserve new node
            if (newNode != null)
            {
                newNode.Reserve(this);
                currentNode = newNode;
            }
        }
        else if (currentNode != null)
        {
            // Update timestamp
            currentNode.Reserve(this);
        }
    }
    
    void ReleaseAllReservations()
    {
        if (currentNode != null)
        {
            currentNode.Release(this);
            currentNode = null;
        }
        
        if (currentPath != null)
        {
            foreach (Node node in currentPath)
            {
                node.Release(this);
            }
        }
    }

    void FindPathToPlayer()
    {
        Node startNode = gridBuilder.GetNodeFromWorldPosition(transform.position);
        Node targetNode = gridBuilder.GetNodeFromWorldPosition(player.position);

        if (startNode == null || targetNode == null || !targetNode.isWalkable)
        {
            currentPath = null;
            return;
        }

        // Dijkstra Algorithm
        currentPath = Dijkstra(startNode, targetNode);
        currentPathIndex = 0;
    }

    List<Node> Dijkstra(Node start, Node target)
    {
        Dictionary<Node, float> distances = new Dictionary<Node, float>();
        Dictionary<Node, Node> previous = new Dictionary<Node, Node>();
        List<Node> unvisited = new List<Node>();

        // Initialize
        foreach (Node node in gridBuilder.grid)
        {
            distances[node] = float.MaxValue;
            unvisited.Add(node);
        }
        distances[start] = 0;

        while (unvisited.Count > 0)
        {
            // Find node with smallest distance
            Node current = null;
            float smallestDistance = float.MaxValue;
            foreach (Node node in unvisited)
            {
                if (distances[node] < smallestDistance)
                {
                    smallestDistance = distances[node];
                    current = node;
                }
            }

            if (current == null || current == target)
                break;

            unvisited.Remove(current);

            // Check neighbors
            if (current.neighbors != null)
            {
                foreach (Node neighbor in current.neighbors)
                {
                    if (!unvisited.Contains(neighbor)) continue;

                    float distance = Vector3.Distance(current.position, neighbor.position);
                    
                    // TAMBAHAN: Penalty untuk node yang sudah dipakai ghost lain
                    float penalty = 0f;
                    if (neighbor.IsOccupied(this))
                    {
                        penalty = occupiedNodePenalty;
                    }
                    
                    // TAMBAHAN: Penalty tambahan jika ada ghost lain terlalu dekat
                    penalty += GetProximityPenalty(neighbor.position);
                    
                    float alt = distances[current] + distance + penalty;

                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = current;
                    }
                }
            }
        }

        // Reconstruct path
        List<Node> path = new List<Node>();
        Node temp = target;

        while (previous.ContainsKey(temp))
        {
            path.Add(temp);
            temp = previous[temp];
        }

        path.Reverse();
        return path.Count > 0 ? path : null;
    }
    
    // Hitung penalty berdasarkan jarak ke ghost lain
    float GetProximityPenalty(Vector3 position)
    {
        float penalty = 0f;
        GhostAI[] otherGhosts = FindObjectsOfType<GhostAI>();
        
        foreach (GhostAI ghost in otherGhosts)
        {
            if (ghost == this) continue;
            
            float distance = Vector3.Distance(position, ghost.transform.position);
            
            // Jika ghost lain terlalu dekat, tambah penalty
            if (distance < personalSpaceRadius)
            {
                penalty += (personalSpaceRadius - distance) * 5f;
            }
        }
        
        return penalty;
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        // Check jika sudah sampai di akhir path
        if (currentPathIndex >= currentPath.Count)
        {
            currentPath = null;
            return;
        }

        Node targetNode = currentPath[currentPathIndex];
        Vector3 targetPosition = new Vector3(targetNode.position.x, transform.position.y, targetNode.position.z);
        
        // TAMBAHAN: Collision avoidance dengan ghost lain
        Vector3 avoidanceVector = GetAvoidanceVector();
        if (avoidanceVector != Vector3.zero)
        {
            targetPosition += avoidanceVector;
        }

        // Move ke target node
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Rotate ke arah target
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // Check jika sudah sampai di node
        if (Vector3.Distance(transform.position, targetPosition) < nodeReachThreshold)
        {
            currentPathIndex++;
        }
    }
    
    // Real-time collision avoidance dengan ghost lain
    Vector3 GetAvoidanceVector()
    {
        Vector3 avoidance = Vector3.zero;
        GhostAI[] otherGhosts = FindObjectsOfType<GhostAI>();
        
        foreach (GhostAI ghost in otherGhosts)
        {
            if (ghost == this) continue;
            
            Vector3 toOther = ghost.transform.position - transform.position;
            float distance = toOther.magnitude;
            
            // Jika terlalu dekat, hindari
            if (distance < personalSpaceRadius && distance > 0.1f)
            {
                // Push away dari ghost lain
                Vector3 pushDirection = -toOther.normalized;
                float pushStrength = (personalSpaceRadius - distance) / personalSpaceRadius;
                avoidance += pushDirection * pushStrength * 0.5f;
            }
        }
        
        return avoidance;
    }
    
    void OnDestroy()
    {
        // Cleanup reservations saat ghost dihancurkan
        ReleaseAllReservations();
    }

    // Visualisasi path di Scene view
    void OnDrawGizmosSelected()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i].position, currentPath[i + 1].position);
            }
        }

        // Draw chase range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
