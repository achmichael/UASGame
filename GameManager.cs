// GameManager.cs
// Mengatur logika global game: checkpoint, collectible, respawn, dan kondisi kemenangan
// - Singleton pattern
// - Simpan checkpoint di PlayerPrefs untuk autosave
// - Integrasi HUDController untuk update UI

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Collectibles")]
    public int collectedCount = 0;
    public int totalCollectibles = 30; // Default 30 lembaran
    public bool autoDetectCollectibles = true; // Auto-count items di scene
    public GameObject collectiblePrefab; // Optional: prefab untuk auto-spawn

    [Header("Player")]
    public int playerLives = 3;
    private Vector3 lastCheckpointPos;

    [Header("References")]
    public HUDController hudController;
    public GameObject ghostPrefab; // Assign Ghost prefab di Inspector

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

        int difficulty = PlayerPrefs.GetInt("Difficulty", 0); // default Easy
        Debug.Log("Game Difficulty Level: " + difficulty);
        switch (difficulty)
        {
            case 0: // Easy
                playerLives = 5;
                totalCollectibles = 30;
                // Activate 1 ghosts only
                ActivateGhosts(1);
                break;
            case 1: // Normal
                playerLives = 4;
                totalCollectibles = 30;
                // Activate 2 ghosts
                ActivateGhosts(2);
                break;
            case 2: // Hard
                playerLives = 3;
                totalCollectibles = 30;
                // Activate 3 ghosts
                ActivateGhosts(3);
                break;
        }

        // Auto-detect dan sync total collectibles dengan item di scene
        if (autoDetectCollectibles)
        {
            SyncCollectiblesCount();
        }

        // Try find HUD in scene if not manually assigned
        if (hudController == null)
            hudController = FindObjectOfType<HUDController>();

        // load last checkpoint if present
        if (PlayerPrefs.HasKey("CheckpointX"))
        {
            lastCheckpointPos = new Vector3(
                PlayerPrefs.GetFloat("CheckpointX"),
                PlayerPrefs.GetFloat("CheckpointY"),
                PlayerPrefs.GetFloat("CheckpointZ")
            );
        }
        else
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                lastCheckpointPos = playerObj.transform.position;
        }

        UpdateHUD();
    }
    
    void SyncCollectiblesCount()
    {
        // Hitung total CollectibleItem yang ada di scene
        CollectibleItem[] items = FindObjectsOfType<CollectibleItem>();
        int itemsInScene = items.Length;
        
        Debug.Log($"[GameManager] Collectibles di scene: {itemsInScene}, Target: {totalCollectibles}");
        
        if (itemsInScene == 0)
        {
            Debug.LogWarning("[GameManager] PERINGATAN: Tidak ada CollectibleItem di scene!");
            Debug.LogWarning("[GameManager] Tambahkan CollectibleItem atau assign collectiblePrefab untuk auto-spawn.");
        }
        else if (itemsInScene < totalCollectibles)
        {
            Debug.LogWarning($"[GameManager] PERINGATAN: Collectibles di scene ({itemsInScene}) kurang dari target ({totalCollectibles})!");
            
            // Coba auto-spawn jika prefab tersedia
            if (collectiblePrefab != null)
            {
                int needed = totalCollectibles - itemsInScene;
                Debug.Log($"[GameManager] Auto-spawning {needed} collectibles...");
                SpawnCollectibles(needed);
            }
            else
            {
                // Sync ke jumlah aktual
                Debug.Log($"[GameManager] Menyesuaikan totalCollectibles ke {itemsInScene}");
                totalCollectibles = itemsInScene;
            }
        }
        else if (itemsInScene > totalCollectibles)
        {
            Debug.LogWarning($"[GameManager] INFO: Collectibles di scene ({itemsInScene}) lebih banyak dari target ({totalCollectibles}).");
            Debug.Log($"[GameManager] Menyesuaikan totalCollectibles ke {itemsInScene}");
            totalCollectibles = itemsInScene;
        }
        else
        {
            Debug.Log($"[GameManager] ✓ Collectibles count sesuai: {totalCollectibles}");
        }
    }
    
    void SpawnCollectibles(int count)
    {
        if (collectiblePrefab == null)
        {
            Debug.LogError("[GameManager] collectiblePrefab tidak di-assign! Tidak bisa spawn items.");
            return;
        }
        
        // Posisi spawn default - sesuaikan dengan map Anda
        // Atau gunakan spawn zones yang sudah ada
        Vector3[] spawnPositions = GenerateSpawnPositions(count);
        
        for (int i = 0; i < count && i < spawnPositions.Length; i++)
        {
            GameObject item = Instantiate(collectiblePrefab, spawnPositions[i], Quaternion.identity);
            item.name = $"Lembaran_AlQuran_{i + 1}";
            Debug.Log($"[GameManager] Spawned collectible at {spawnPositions[i]}");
        }
        
        Debug.Log($"[GameManager] ✓ Spawned {count} collectibles successfully!");
    }
    
    Vector3[] GenerateSpawnPositions(int count)
    {
        // Generate posisi spawn secara prosedural
        // Bisa disesuaikan dengan layout map Anda
        Vector3[] positions = new Vector3[count];
        
        // Contoh: Spawn dalam grid pattern
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(count));
        float spacing = 5f; // Jarak antar item
        
        for (int i = 0; i < count; i++)
        {
            int x = i % gridSize;
            int z = i / gridSize;
            
            // Posisi dengan sedikit random offset
            Vector3 pos = new Vector3(
                x * spacing + Random.Range(-1f, 1f),
                1f, // Height di atas ground
                z * spacing + Random.Range(-1f, 1f)
            );
            
            positions[i] = pos;
        }
        
        return positions;
    }

    void ActivateGhosts(int count)
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        Debug.Log("Found " + ghosts.Length + " ghosts in scene, need " + count);
        
        // Jika ghost di scene kurang dari yang dibutuhkan, spawn ghost baru
        if (ghostPrefab != null && ghosts.Length < count)
        {
            int needed = count - ghosts.Length;
            Debug.Log("Spawning " + needed + " additional ghosts...");
            
            // Spawn posisi default - sesuaikan dengan map Anda
            Vector3[] spawnPositions = new Vector3[]
            {
                new Vector3(10, 1, 10),
                new Vector3(-10, 1, 10),
                new Vector3(10, 1, -10),
                new Vector3(-10, 1, -10)
            };
            
            for (int i = 0; i < needed && i < spawnPositions.Length; i++)
            {
                GameObject newGhost = Instantiate(ghostPrefab, spawnPositions[i], Quaternion.identity);
                newGhost.tag = "Ghost";
                newGhost.name = "Ghost_" + (ghosts.Length + i + 1);
            }
            
            // Refresh ghost list setelah spawn
            ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        }
        
        // Activate/deactivate ghosts sesuai difficulty
        Debug.Log("Activating " + count + " ghosts out of " + ghosts.Length);
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].SetActive(i < count);
        }
    }

    public void AddCollectedItem()
    {
        collectedCount++;
        UpdateHUD();

        if (collectedCount >= totalCollectibles)
        {
            // Semua lembaran terkumpul → trigger ending
            Invoke(nameof(TriggerNormalEnding), 1.5f);
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPos = position;
        PlayerPrefs.SetFloat("CheckpointX", position.x);
        PlayerPrefs.SetFloat("CheckpointY", position.y);
        PlayerPrefs.SetFloat("CheckpointZ", position.z);
        PlayerPrefs.Save();

        Debug.Log("Checkpoint tersimpan di: " + position);
    }

    public void RespawnPlayer(GameObject player)
    {
        // Jika player null, cari berdasarkan tag
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Tidak menemukan objek player untuk respawn.");
                return;
            }
        }

        // Teleport player ke last checkpoint
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            // disable controller temporarily to avoid collision/overlap issues
            cc.enabled = false;
            player.transform.position = lastCheckpointPos;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = lastCheckpointPos;
        }

        playerLives--;

        if (playerLives <= 0)
        {
            SceneManager.LoadScene("GameOverScene");
        }
        else
        {
            UpdateHUD();
        }
    }

    void TriggerNormalEnding()
    {
        // Find CutsceneController and play normal ending if present
        // CutsceneController cs = FindObjectOfType<CutsceneController>();
        // if (cs != null)
        //     cs.PlayNormalEnding();
        // else
        //     SceneManager.LoadScene("NormalEndingScene");
    }

    public void TriggerSecretEnding()
    {
        // CutsceneController cs = FindObjectOfType<CutsceneController>();
        // if (cs != null)
        //     cs.PlaySecretEnding();
        // else
        //     SceneManager.LoadScene("SecretEndingScene");
    }

    void UpdateHUD()
    {
        if (hudController != null)
            hudController.UpdateHUD(collectedCount, playerLives);
    }
}
