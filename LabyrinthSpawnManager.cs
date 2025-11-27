// LabyrinthSpawnManager.cs
// Mengelola spawn random untuk Player, Item, dan Enemy di dalam labirin
// - Validasi posisi spawn tidak menembus dinding
// - Spawn hanya di dalam bounds labirin
// - Spawn di area walkable (menggunakan GridBuilder)

using System.Collections.Generic;
using UnityEngine;

public class LabyrinthSpawnManager : MonoBehaviour
{
    [Header("Labyrinth Bounds")]
    public Transform labyrinthParent; // Assign parent GameObject labirin
    public Vector3 labyrinthCenter = Vector3.zero;
    public Vector3 labyrinthSize = new Vector3(50, 5, 50); // Ukuran area labirin (X, Y, Z)
    public float groundLevel = 0f; // Y position untuk spawn (ground level)
    
    [Header("Spawn Settings")]
    public LayerMask wallLayer; // Layer untuk wall/obstacle
    public float wallCheckRadius = 0.5f; // Radius check untuk obstacle
    public int maxSpawnAttempts = 100; // Max percobaan spawn per object
    public float minDistanceBetweenSpawns = 2f; // Jarak minimum antar spawn
    
    [Header("Player Spawn")]
    public GameObject playerPrefab;
    public int playerSpawnAttempts = 50;
    public List<Vector3> safePlayerSpawnZones = new List<Vector3>(); // Area aman untuk spawn player
    
    [Header("Item Spawn")]
    public GameObject itemPrefab;
    public int itemCount = 30;
    public float itemSpawnHeight = 1f; // Height item dari ground
    
    [Header("Enemy Spawn")]
    public GameObject enemyPrefab;
    public int enemyCount = 3;
    public float enemySpawnHeight = 1f;
    public float minDistanceFromPlayer = 10f; // Jarak minimum enemy dari player
    
    [Header("References")]
    public GridBuilder gridBuilder; // Reference ke GridBuilder untuk validasi
    
    [Header("Visualization")]
    public bool showGizmos = true;
    public Color spawnAreaColor = new Color(0, 1, 0, 0.2f);
    
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private Vector3 playerSpawnPosition;
    
    void Start()
    {
        // Auto-find GridBuilder jika tidak di-assign
        if (gridBuilder == null)
            gridBuilder = FindObjectOfType<GridBuilder>();
            
        // Auto-detect labyrinth bounds jika parent di-assign
        if (labyrinthParent != null)
        {
            DetectLabyrinthBounds();
        }
    }
    
    void DetectLabyrinthBounds()
    {
        // Hitung bounds dari semua collider di labyrinth
        Renderer[] renderers = labyrinthParent.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            labyrinthCenter = bounds.center;
            labyrinthSize = bounds.size;
            
            Debug.Log($"[SpawnManager] Auto-detected labyrinth bounds: Center={labyrinthCenter}, Size={labyrinthSize}");
        }
    }
    
    /// <summary>
    /// Spawn semua objects (Player, Items, Enemies) dengan validasi
    /// </summary>
    public void SpawnAllObjects()
    {
        spawnedPositions.Clear();
        
        // 1. Spawn Player terlebih dahulu
        if (playerPrefab != null)
        {
            SpawnPlayer();
        }
        
        // 2. Spawn Items
        if (itemPrefab != null)
        {
            SpawnItems(itemCount);
        }
        
        // 3. Spawn Enemies (jauh dari player)
        if (enemyPrefab != null)
        {
            SpawnEnemies(enemyCount);
        }
    }
    
    /// <summary>
    /// Spawn player di posisi aman
    /// </summary>
    void SpawnPlayer()
    {
        Vector3 spawnPos;
        
        // Coba spawn di safe zones jika tersedia
        if (safePlayerSpawnZones.Count > 0)
        {
            // Pilih random safe zone
            Vector3 safeZone = safePlayerSpawnZones[Random.Range(0, safePlayerSpawnZones.Count)];
            spawnPos = FindValidSpawnPosition(safeZone, playerSpawnAttempts);
        }
        else
        {
            // Random spawn di dalam labirin
            spawnPos = FindValidSpawnPosition(labyrinthCenter, playerSpawnAttempts);
        }
        
        if (spawnPos != Vector3.zero)
        {
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.tag = "Player";
            playerSpawnPosition = spawnPos;
            spawnedPositions.Add(spawnPos);
            
            Debug.Log($"[SpawnManager] Player spawned at {spawnPos}");
        }
        else
        {
            Debug.LogError("[SpawnManager] Failed to find valid spawn position for Player!");
        }
    }
    
    /// <summary>
    /// Spawn items secara random di labirin
    /// </summary>
    public void SpawnItems(int count)
    {
        int spawned = 0;
        int attempts = 0;
        int maxTotalAttempts = count * maxSpawnAttempts;
        
        while (spawned < count && attempts < maxTotalAttempts)
        {
            Vector3 spawnPos = FindValidSpawnPosition(labyrinthCenter, 1, itemSpawnHeight);
            
            if (spawnPos != Vector3.zero && !IsTooCloseToOtherSpawns(spawnPos, minDistanceBetweenSpawns))
            {
                GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
                item.name = $"Item_{spawned + 1}";
                spawnedPositions.Add(spawnPos);
                spawned++;
            }
            
            attempts++;
        }
        
        Debug.Log($"[SpawnManager] Spawned {spawned}/{count} items in {attempts} attempts");
    }
    
    /// <summary>
    /// Spawn enemies jauh dari player
    /// </summary>
    public void SpawnEnemies(int count)
    {
        int spawned = 0;
        int attempts = 0;
        int maxTotalAttempts = count * maxSpawnAttempts;
        
        while (spawned < count && attempts < maxTotalAttempts)
        {
            Vector3 spawnPos = FindValidSpawnPosition(labyrinthCenter, 1, enemySpawnHeight);
            
            // Validasi: Jauh dari player dan tidak terlalu dekat dengan spawn lain
            bool farFromPlayer = Vector3.Distance(spawnPos, playerSpawnPosition) > minDistanceFromPlayer;
            bool notTooClose = !IsTooCloseToOtherSpawns(spawnPos, minDistanceBetweenSpawns * 2f);
            
            if (spawnPos != Vector3.zero && farFromPlayer && notTooClose)
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.tag = "Ghost";
                enemy.name = $"Ghost_{spawned + 1}";
                spawnedPositions.Add(spawnPos);
                spawned++;
            }
            
            attempts++;
        }
        
        Debug.Log($"[SpawnManager] Spawned {spawned}/{count} enemies in {attempts} attempts");
    }
    
    /// <summary>
    /// Mencari posisi spawn yang valid (tidak menembus dinding)
    /// </summary>
    Vector3 FindValidSpawnPosition(Vector3 centerPoint, int attempts, float height = 1f)
    {
        for (int i = 0; i < attempts; i++)
        {
            // Generate random position dalam bounds labirin
            Vector3 randomPos = new Vector3(
                centerPoint.x + Random.Range(-labyrinthSize.x / 2, labyrinthSize.x / 2),
                groundLevel + height,
                centerPoint.z + Random.Range(-labyrinthSize.z / 2, labyrinthSize.z / 2)
            );
            
            // Validasi posisi
            if (IsValidSpawnPosition(randomPos))
            {
                return randomPos;
            }
        }
        
        return Vector3.zero; // Gagal menemukan posisi valid
    }
    
    /// <summary>
    /// Validasi apakah posisi aman untuk spawn
    /// </summary>
    bool IsValidSpawnPosition(Vector3 position)
    {
        // 1. Check apakah dalam bounds labirin
        if (!IsInsideLabyrinthBounds(position))
        {
            return false;
        }
        
        // 2. Check apakah ada wall/obstacle di posisi tersebut
        if (IsPositionBlocked(position))
        {
            return false;
        }
        
        // 3. Check apakah ada ground di bawah posisi
        if (!HasGroundBelow(position))
        {
            return false;
        }
        
        // 4. Optional: Check dengan GridBuilder jika tersedia
        if (gridBuilder != null && gridBuilder.grid != null)
        {
            Node node = gridBuilder.GetNodeFromWorldPosition(position);
            if (node != null && !node.isWalkable)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Check apakah posisi dalam bounds labirin
    /// </summary>
    bool IsInsideLabyrinthBounds(Vector3 position)
    {
        Vector3 localPos = position - labyrinthCenter;
        
        return Mathf.Abs(localPos.x) <= labyrinthSize.x / 2 &&
               Mathf.Abs(localPos.y) <= labyrinthSize.y / 2 &&
               Mathf.Abs(localPos.z) <= labyrinthSize.z / 2;
    }
    
    /// <summary>
    /// Check apakah ada obstacle di posisi
    /// </summary>
    bool IsPositionBlocked(Vector3 position)
    {
        // Check sphere overlap dengan wall layer
        Collider[] colliders = Physics.OverlapSphere(position, wallCheckRadius, wallLayer);
        return colliders.Length > 0;
    }
    
    /// <summary>
    /// Check apakah ada ground di bawah posisi
    /// </summary>
    bool HasGroundBelow(Vector3 position)
    {
        // Raycast ke bawah untuk detect ground
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, 5f))
        {
            // Ada ground dalam jarak 5 unit ke bawah
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Check apakah posisi terlalu dekat dengan spawn lain
    /// </summary>
    bool IsTooCloseToOtherSpawns(Vector3 position, float minDistance)
    {
        foreach (Vector3 spawnedPos in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minDistance)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Respawn player ke posisi random yang aman
    /// </summary>
    public Vector3 GetPlayerRespawnPosition()
    {
        Vector3 respawnPos;
        
        if (safePlayerSpawnZones.Count > 0)
        {
            Vector3 safeZone = safePlayerSpawnZones[Random.Range(0, safePlayerSpawnZones.Count)];
            respawnPos = FindValidSpawnPosition(safeZone, playerSpawnAttempts);
        }
        else
        {
            respawnPos = FindValidSpawnPosition(labyrinthCenter, playerSpawnAttempts);
        }
        
        return respawnPos != Vector3.zero ? respawnPos : playerSpawnPosition; // Fallback ke posisi awal
    }
    
    /// <summary>
    /// Spawn single item di posisi random
    /// </summary>
    public GameObject SpawnSingleItem(Vector3 nearPosition)
    {
        Vector3 spawnPos = FindValidSpawnPosition(nearPosition, maxSpawnAttempts, itemSpawnHeight);
        
        if (spawnPos != Vector3.zero)
        {
            GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
            spawnedPositions.Add(spawnPos);
            return item;
        }
        
        return null;
    }
    
    /// <summary>
    /// Spawn single enemy di posisi random
    /// </summary>
    public GameObject SpawnSingleEnemy(Vector3 nearPosition)
    {
        Vector3 spawnPos = FindValidSpawnPosition(nearPosition, maxSpawnAttempts, enemySpawnHeight);
        
        if (spawnPos != Vector3.zero)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.tag = "Ghost";
            spawnedPositions.Add(spawnPos);
            return enemy;
        }
        
        return null;
    }
    
    /// <summary>
    /// Clear semua spawned positions (untuk reset)
    /// </summary>
    public void ClearSpawnedPositions()
    {
        spawnedPositions.Clear();
    }
    
    // Visualisasi bounds di Scene view
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw labyrinth bounds
        Gizmos.color = spawnAreaColor;
        Gizmos.DrawWireCube(labyrinthCenter, labyrinthSize);
        
        // Draw safe player spawn zones
        Gizmos.color = Color.green;
        foreach (Vector3 safeZone in safePlayerSpawnZones)
        {
            Gizmos.DrawWireSphere(safeZone, 2f);
        }
        
        // Draw spawned positions (runtime only)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            foreach (Vector3 pos in spawnedPositions)
            {
                Gizmos.DrawSphere(pos, 0.3f);
            }
            
            // Draw player spawn
            if (playerSpawnPosition != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(playerSpawnPosition, 0.5f);
            }
        }
    }
}
