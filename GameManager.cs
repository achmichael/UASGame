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
    public int totalCollectibles = 5;

    [Header("Player")]
    public int playerLives = 3;
    private Vector3 lastCheckpointPos;

    [Header("References")]
    public HUDController hudController;

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
        CutsceneController cs = FindObjectOfType<CutsceneController>();
        if (cs != null)
            cs.PlayNormalEnding();
        else
            SceneManager.LoadScene("NormalEndingScene");
    }

    public void TriggerSecretEnding()
    {
        CutsceneController cs = FindObjectOfType<CutsceneController>();
        if (cs != null)
            cs.PlaySecretEnding();
        else
            SceneManager.LoadScene("SecretEndingScene");
    }

    void UpdateHUD()
    {
        if (hudController != null)
            hudController.UpdateHUD(collectedCount, playerLives);
    }
}
