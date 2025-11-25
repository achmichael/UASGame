using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 10;      // Lebar grid (X)
    public int gridHeight = 10;     // Tinggi grid (Z)
    public float nodeSpacing = 2f;  // Jarak antar node
    public Vector3 gridOrigin = Vector3.zero;  // Titik awal grid
    
    [Header("Walkability Detection")]
    public LayerMask unwalkableMask;  // Layer untuk wall/obstacle
    public float nodeRadius = 0.5f;   // Radius check untuk obstacle
    
    [Header("Visualization")]
    public bool showGizmos = true;
    public Color walkableColor = Color.green;
    public Color unwalkableColor = Color.red;
    
    // Grid runtime untuk AI
    [HideInInspector]
    public Node[,] grid;
    
    void Awake()
    {
        CreateGrid();
    }
    
    void CreateGrid()
    {
        grid = new Node[gridWidth, gridHeight];
        
        // 1. Buat semua nodes
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPos = gridOrigin + new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
                
                // Check apakah node walkable (tidak ada obstacle)
                bool walkable = !Physics.CheckSphere(worldPos, nodeRadius, unwalkableMask);
                
                grid[x, z] = new Node(worldPos);
                grid[x, z].isWalkable = walkable;
            }
        }
        
        // 2. Hubungkan neighbors (4-directional: up, down, left, right)
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                List<Node> neighborsList = new List<Node>();
                
                // Kanan
                if (x + 1 < gridWidth && grid[x + 1, z].isWalkable)
                    neighborsList.Add(grid[x + 1, z]);
                
                // Kiri
                if (x - 1 >= 0 && grid[x - 1, z].isWalkable)
                    neighborsList.Add(grid[x - 1, z]);
                
                // Atas
                if (z + 1 < gridHeight && grid[x, z + 1].isWalkable)
                    neighborsList.Add(grid[x, z + 1]);
                
                // Bawah
                if (z - 1 >= 0 && grid[x, z - 1].isWalkable)
                    neighborsList.Add(grid[x, z - 1]);
                
                grid[x, z].neighbors = neighborsList.ToArray();
            }
        }
        
        Debug.Log($"Grid created: {gridWidth}x{gridHeight} = {gridWidth * gridHeight} nodes");
    }
    
    // Dapatkan node terdekat dari posisi world
    public Node GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        // Convert world position ke grid coordinate
        float percentX = (worldPosition.x - gridOrigin.x) / (gridWidth * nodeSpacing);
        float percentZ = (worldPosition.z - gridOrigin.z) / (gridHeight * nodeSpacing);
        
        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);
        
        int x = Mathf.RoundToInt((gridWidth - 1) * percentX);
        int z = Mathf.RoundToInt((gridHeight - 1) * percentZ);
        
        return grid[x, z];
    }
    
    // Visualisasi grid di Scene view
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        if (grid != null)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = node.isWalkable ? walkableColor : unwalkableColor;
                Gizmos.DrawWireCube(node.position, Vector3.one * (nodeSpacing * 0.8f));
                
                // Gambar garis ke neighbors
                if (node.neighbors != null)
                {
                    Gizmos.color = Color.cyan;
                    foreach (Node neighbor in node.neighbors)
                    {
                        Gizmos.DrawLine(node.position, neighbor.position);
                    }
                }
            }
        }
    }
}
