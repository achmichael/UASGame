using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Lebar grid (jumlah node di sumbu X)")]
    public int gridWidth = 10;
    [Tooltip("Panjang grid (jumlah node di sumbu Z)")]
    public int gridHeight = 10;
    [Tooltip("Jarak antar node dalam units")]
    public float nodeSpacing = 2f;
    [Tooltip("Titik awal grid (pojok kiri bawah labirin)")]
    public Vector3 gridOrigin = Vector3.zero;
    
    [Header("Walkability Detection")]
    [Tooltip("Layer untuk wall/obstacle - HARUS SAMA dengan wallLayer di SpawnManager")]
    public LayerMask unwalkableMask;
    [Tooltip("Radius check untuk obstacle detection")]
    public float nodeRadius = 0.5f;
    
    [Header("Auto Detection")]
    [Tooltip("Auto-detect grid bounds dari labyrinth parent")]
    public bool autoDetectBounds = false;
    [Tooltip("Parent object labirin untuk auto-detect")]
    public Transform labyrinthParent;
    [Tooltip("Offset ketinggian dari dasar labirin (bounds.min.y). Atur ini agar grid pas di atas lantai.")]
    public float autoDetectHeightOffset = 0.1f;
    
    [Header("Visualization")]
    public bool showGizmos = true;
    public Color walkableColor = Color.green;
    public Color unwalkableColor = Color.red;
    
    // Grid runtime untuk AI
    [HideInInspector]
    public Node[,] grid;
    
    void Awake()
    {
        if (autoDetectBounds && labyrinthParent != null)
        {
            AutoDetectGridSettings();
        }
        
        CreateGrid();
    }
    
    void AutoDetectGridSettings()
    {
        Collider[] colliders = labyrinthParent.GetComponentsInChildren<Collider>();
        
        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;
            foreach (Collider col in colliders)
            {
                bounds.Encapsulate(col.bounds);
            }
            
            // Set grid origin ke pojok kiri bawah (menggunakan bounds.min.y + offset)
            gridOrigin = new Vector3(bounds.min.x, bounds.min.y + autoDetectHeightOffset, bounds.min.z);
            
            // Hitung grid size
            gridWidth = Mathf.Max(5, Mathf.FloorToInt(bounds.size.x / nodeSpacing));
            gridHeight = Mathf.Max(5, Mathf.FloorToInt(bounds.size.z / nodeSpacing));
            
            Debug.Log($"[GridBuilder] Auto-detected: Origin={gridOrigin}, Grid={gridWidth}x{gridHeight}");
        }
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
