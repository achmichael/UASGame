using UnityEngine;

[System.Serializable]
public class Node
{
    public Vector3 position;
    public bool isWalkable = true;
    [System.NonSerialized]
    public Node[] neighbors;
    
    // Untuk path separation - track ghost yang sedang menggunakan node ini
    [System.NonSerialized]
    public GhostAI occupyingGhost = null;
    
    // Timestamp terakhir node digunakan (untuk decay reservation)
    [System.NonSerialized]
    public float lastOccupiedTime = 0f;

    public Node(Vector3 pos)
    {
        position = pos;
    }
    
    // Check apakah node sedang digunakan ghost lain
    public bool IsOccupied(GhostAI currentGhost)
    {
        if (occupyingGhost == null) return false;
        if (occupyingGhost == currentGhost) return false;
        
        // Jika sudah lebih dari 2 detik, anggap tidak occupied lagi
        if (Time.time - lastOccupiedTime > 2f)
        {
            occupyingGhost = null;
            return false;
        }
        
        return true;
    }
    
    // Reserve node untuk ghost tertentu
    public void Reserve(GhostAI ghost)
    {
        occupyingGhost = ghost;
        lastOccupiedTime = Time.time;
    }
    
    // Release reservation
    public void Release(GhostAI ghost)
    {
        if (occupyingGhost == ghost)
        {
            occupyingGhost = null;
        }
    }
}