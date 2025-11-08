// GhostAI.cs
// AI hantu yang mengejar pemain dengan algoritma Dijkstra untuk pathfinding di labirin
// - Node-based grid yang di-setup di inspector (Node[,] grid)
// - Saat player dalam chaseRange, hitung jalur ke node player dan kejar
// - Saat player keluar range, kembali ke startPosition (patrol/return)
// - Implementasi Dijkstra sederhana (O(n^2)) untuk pembelajaran

using UnityEngine;
using System.Collections.Generic;

public class GhostAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Node[,] grid; // assign grid in inspector or by a GridBuilder script

    [Header("Behavior")]
    public float chaseRange = 10f;
    public float moveSpeed = 3f;
    public float nodeReachThreshold = 0.1f;

    private Vector3 startPosition;
    private Node currentNode;
    private List<Node> path = new List<Node>();
    private int pathIndex = 0;

    void Start()
    {
        startPosition = transform.position;
        if (grid != null && grid.Length > 0)
            currentNode = GetClosestNode(transform.position);
    }

    void Update()
    {
        if (player == null || grid == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        Node targetNode;
        if (distanceToPlayer <= chaseRange)
        {
            // Chase player
            targetNode = GetClosestNode(player.position);
        }
        else
        {
            // Return to start
            targetNode = GetClosestNode(startPosition);
        }

        // Recompute path if necessary or if target changed
        Node myNode = GetClosestNode(transform.position);
        if (currentNode == null || myNode != currentNode || path.Count == 0)
        {
            currentNode = myNode;
            path = Dijkstra(currentNode, targetNode);
            pathIndex = 0;
        }

        FollowPath();
    }

    // Fungsi mencari node terdekat dari posisi tertentu
    Node GetClosestNode(Vector3 pos)
    {
        Node closest = null;
        float minDist = Mathf.Infinity;
        foreach (Node n in grid)
        {
            if (n == null) continue;
            float dist = Vector3.Distance(pos, n.position);
            if (dist < minDist)
            {
                closest = n;
                minDist = dist;
            }
        }
        return closest;
    }

    // Implementasi algoritma Dijkstra (sederhana)
    List<Node> Dijkstra(Node start, Node target)
    {
        List<Node> result = new List<Node>();
        if (start == null || target == null) return result;

        // Prepare sets
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        List<Node> unvisited = new List<Node>();

        foreach (Node node in grid)
        {
            if (node == null) continue;
            dist[node] = Mathf.Infinity;
            prev[node] = null;
            unvisited.Add(node);
        }

        dist[start] = 0f;

        while (unvisited.Count > 0)
        {
            // Pick node with smallest distance
            unvisited.Sort((a, b) => dist[a].CompareTo(dist[b]));
            Node u = unvisited[0];
            unvisited.RemoveAt(0);

            if (u == target)
                break;

            // If unreachable, break
            if (dist[u] == Mathf.Infinity) break;

            foreach (Node neighbor in u.neighbors)
            {
                if (neighbor == null) continue;
                float alt = dist[u] + Vector3.Distance(u.position, neighbor.position);
                if (alt < dist[neighbor])
                {
                    dist[neighbor] = alt;
                    prev[neighbor] = u;
                }
            }
        }

        // Reconstruct path
        Node curr = target;
        while (curr != null)
        {
            result.Insert(0, curr);
            prev.TryGetValue(curr, out curr);
        }

        return result;
    }

    // Gerakkan hantu mengikuti path
    void FollowPath()
    {
        if (path == null || path.Count == 0) return;

        // Ensure pathIndex valid
        if (pathIndex >= path.Count) pathIndex = path.Count - 1;

        // Jika path lebih dari 1 node, tuju node berikutnya
        if (path.Count > 1)
        {
            int targetIndex = Mathf.Min(pathIndex + 1, path.Count - 1);
            Vector3 targetPos = path[targetIndex].position;

            // Move
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            // Rotate to face
            Vector3 lookDir = (targetPos - transform.position);
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.deltaTime);
            }

            // If reached node, advance
            if (Vector3.Distance(transform.position, targetPos) <= nodeReachThreshold)
            {
                currentNode = path[targetIndex];
                pathIndex = targetIndex;
            }
        }
    }

    // Optional: visualize path for debugging
    void OnDrawGizmosSelected()
    {
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < path.Count - 1; i++)
            {
                if (path[i] != null && path[i + 1] != null)
                    Gizmos.DrawLine(path[i].position, path[i + 1].position);
            }
        }

        // draw chase range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}

// Kelas Node untuk grid labirin
[System.Serializable]
public class Node
{
    public Vector3 position;
    public List<Node> neighbors = new List<Node>();
}
