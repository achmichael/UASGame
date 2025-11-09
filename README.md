# PANDUAN INTEGRASI SCRIPT DENGAN UNITY GAMEOBJECT
## Game: Labirin Al-Qur'an - Perjalanan Saipul

---

## 📋 DAFTAR ISI
1. [Setup Awal Project](#1-setup-awal-project)
2. [Konfigurasi Player (Saipul)](#2-konfigurasi-player-saipul)
3. [Konfigurasi Camera (First Person)](#3-konfigurasi-camera-first-person)
4. [Setup GameManager](#4-setup-gamemanager)
5. [Setup HUD/UI](#5-setup-hudui)
6. [Setup Collectible Items (Lembaran Al-Qur'an)](#6-setup-collectible-items-lembaran-al-quran)
7. [Setup Checkpoint Zone](#7-setup-checkpoint-zone)
8. [Setup Ghost AI (Hantu)](#8-setup-ghost-ai-hantu)
9. [Setup Exit Trigger (Pintu Keluar)](#9-setup-exit-trigger-pintu-keluar)
10. [Setup Cutscene & Ending](#10-setup-cutscene--ending)
11. [Setup Efek Visual & Audio](#11-setup-efek-visual--audio)
12. [Konfigurasi Level Difficulty](#12-konfigurasi-level-difficulty)
13. [Testing & Troubleshooting](#13-testing--troubleshooting)

---

## 1. SETUP AWAL PROJECT

### 1.1 Import Package yang Diperlukan
1. Buka Unity Hub dan buat/buka project UAS Game
2. Install package berikut via **Window > Package Manager**:
   - **TextMeshPro** (untuk UI modern)
   - **Timeline** (untuk cutscene)
   - **Cinemachine** (optional untuk camera advanced)
   - **ProBuilder** (optional untuk membuat labirin)

### 1.2 Setup Tags & Layers
1. Buka **Edit > Project Settings > Tags and Layers**
2. Tambahkan Tags berikut:
   - `Player`
   - `Ghost`
   - `Collectible`
   - `Checkpoint`
   - `SafeZone`
3. Tambahkan Layers:
   - `Ground`
   - `Wall`
   - `Ghost`

### 1.3 Setup Input Manager (Optional)
Pastikan Input Manager sudah memiliki:
- **Horizontal**: A/D atau Arrow Left/Right
- **Vertical**: W/S atau Arrow Up/Down  
- **Jump**: Space
- **Mouse X**: Mouse X-axis
- **Mouse Y**: Mouse Y-axis

---

## 2. KONFIGURASI PLAYER (SAIPUL)

### 2.1 Membuat GameObject Player
1. Buat GameObject kosong: **GameObject > Create Empty**
2. Rename menjadi `Player`
3. Tambahkan **Tag: Player**
4. **Position:** (0, 1, 0) atau sesuai spawn point

### 2.2 Menambahkan Components ke Player

#### A. CharacterController
1. Pilih `Player` GameObject
2. **Add Component > CharacterController**
3. Setting yang disarankan:
   - **Center:** (0, 1, 0)
   - **Radius:** 0.5
   - **Height:** 2
   - **Skin Width:** 0.08

#### B. PlayerController Script
1. **Add Component > Player Controller** (script)
2. Atur parameter:
   - **Speed:** 5 (Easy), 4.5 (Normal), 4 (Hard)
   - **Gravity:** -9.81
   - **Jump Force:** 2
   - **Camera Transform:** Assign Main Camera (child dari Player, lihat bagian 3)

#### C. PlayerHealth Script
1. **Add Component > Player Health**
2. Atur parameter:
   - **Max Health:** 100
   - **Damage Cooldown:** 1.5
   - **Damage Effect:** Assign dari Canvas (lihat bagian 11)
   - **Fade Transition:** Assign dari Canvas
   - **Hurt Sound:** Import file audio (.wav/.mp3) dan drag ke slot
   - **Death Sound:** Import file audio dan drag ke slot

### 2.3 Membuat Ground Check (Optional tapi Recommended)
1. Dalam `Player`, buat Empty GameObject baru: **Create Empty**
2. Rename menjadi `GroundCheck`
3. Position: (0, 0, 0) - tepat di bawah player
4. Kembali ke `Player` > **PlayerController**:
   - **Ground Check:** Drag `GroundCheck` GameObject
   - **Ground Distance:** 0.2
   - **Ground Mask:** Pilih layer `Ground`

### 2.4 Menambahkan Capsule Visual (Optional)
1. Dalam `Player`: **Right Click > 3D Object > Capsule**
2. Rename `PlayerModel`
3. **Remove Capsule Collider** (sudah ada CharacterController)
4. Adjust Transform:
   - **Position:** (0, 1, 0)
   - **Scale:** (1, 1, 1)
5. Tambahkan Material untuk visualisasi

---

## 3. KONFIGURASI CAMERA (FIRST PERSON)

### 3.1 Membuat Main Camera sebagai Child Player
1. Jika sudah ada Main Camera di scene, delete atau disable
2. Dalam `Player`: **Right Click > Camera**
3. Rename menjadi `Main Camera`
4. Pastikan tag **MainCamera** aktif
5. Transform:
   - **Position:** (0, 1.6, 0) - setara tinggi mata
   - **Rotation:** (0, 0, 0)

### 3.2 Menambahkan CameraController Script
1. Pilih `Main Camera` (child dari Player)
2. **Add Component > Camera Controller**
3. Setting:
   - **Mouse Sensitivity:** 200 (bisa disesuaikan preferensi)
   - **Player Body:** Drag `Player` GameObject (parent)
   - **Lock Cursor On Start:** ✅ Centang

### 3.3 Testing Camera
1. Play mode
2. Gerakkan mouse → camera harus rotate
3. Mouse horizontal → player body rotate (Y-axis)
4. Mouse vertical → camera rotate (X-axis, terbatas -80° sampai +80°)

---

## 4. SETUP GAMEMANAGER

### 4.1 Membuat GameObject GameManager
1. **GameObject > Create Empty**
2. Rename: `GameManager`
3. **Position:** (0, 0, 0)
4. **Add Component > Game Manager** (script)

### 4.2 Konfigurasi GameManager
1. Pilih `GameManager`
2. Setting inspector:
   - **Collected Count:** 0 (auto-update saat gameplay)
   - **Total Collectibles:** 30 (sesuai design: 30 lembaran)
   - **Player Lives:** 
     - Easy: 5
     - Normal: 4
     - Hard: 3
   - **HUD Controller:** Drag `Canvas > HUDController` (akan dibuat di bagian 5)

### 4.3 DontDestroyOnLoad
GameManager otomatis menjadi **Singleton** dan persistent antar scene. Pastikan hanya ada 1 GameManager di scene!

---

## 5. SETUP HUD/UI

### 5.1 Membuat Canvas
1. **GameObject > UI > Canvas**
2. Canvas akan otomatis dibuat dengan EventSystem
3. Canvas Settings:
   - **Render Mode:** Screen Space - Overlay
   - **UI Scale Mode:** Scale With Screen Size
   - **Reference Resolution:** 1920x1080

### 5.2 Membuat HUD Text - Collectible Counter

#### A. Membuat Text GameObject
1. Dalam Canvas: **Right Click > UI > Text - TextMeshPro**
   - Jika muncul popup import TMP Essentials, klik **Import**
2. Rename: `CollectibleText`
3. Transform:
   - **Anchor Preset:** Top Left
   - **Pos X:** 100
   - **Pos Y:** -50
   - **Width:** 300
   - **Height:** 50

#### B. Setting TextMeshProUGUI
- **Text:** `Lembaran: 0 / 30`
- **Font Size:** 24
- **Color:** Putih atau kuning
- **Alignment:** Left & Middle

### 5.3 Membuat HUD Text - Lives Counter

#### A. Membuat Text GameObject
1. Dalam Canvas: **Right Click > UI > Text - TextMeshPro**
2. Rename: `LivesText`
3. Transform:
   - **Anchor Preset:** Top Left
   - **Pos X:** 100
   - **Pos Y:** -100
   - **Width:** 200
   - **Height:** 50

#### B. Setting TextMeshProUGUI
- **Text:** `Nyawa: 5`
- **Font Size:** 24
- **Color:** Merah atau putih
- **Alignment:** Left & Middle

### 5.4 Menambahkan HUDController Script
1. Pilih `Canvas` GameObject
2. **Add Component > HUD Controller**
3. Setting:
   - **Collectible Text:** Drag `CollectibleText`
   - **Lives Text:** Drag `LivesText`

### 5.5 Hubungkan ke GameManager
1. Pilih `GameManager`
2. **HUD Controller:** Drag `Canvas` GameObject

---

## 6. SETUP COLLECTIBLE ITEMS (LEMBARAN AL-QUR'AN)

### 6.1 Membuat Prefab Lembaran Al-Qur'an

#### A. Buat GameObject Collectible
1. **GameObject > 3D Object > Cube** (atau model 3D lembaran)
2. Rename: `Lembaran_AlQuran`
3. **Tag:** Collectible
4. Transform:
   - **Scale:** (0.3, 0.4, 0.05) - bentuk seperti kertas

#### B. Menambahkan Collider
1. Pastikan ada **Box Collider**
2. **✅ Is Trigger** - HARUS dicentang!

#### C. Menambahkan Material
1. Buat material baru: **Assets > Create > Material**
2. Rename: `Mat_Lembaran`
3. Atur warna/texture sesuai design (kuning keemasan)
4. Drag material ke `Lembaran_AlQuran`

#### D. Menambahkan CollectibleItem Script
1. Pilih `Lembaran_AlQuran`
2. **Add Component > Collectible Item**
3. Setting:
   - **Item Name:** "Lembaran Al-Qur'an"
   - **Rotation Speed:** 50 (efek berputar)
   - **Collect Sound:** Import audio dan drag (.wav recommended)
   - **Glow Light:** (Optional) Tambah **Point Light** sebagai child
     - **Color:** Kuning
     - **Range:** 5
     - **Intensity:** 1.5

### 6.2 Membuat Prefab
1. Drag `Lembaran_AlQuran` dari Hierarchy ke folder **Assets/Prefabs**
2. Delete dari scene (akan spawn manual)

### 6.3 Menempatkan 30 Lembaran di Labirin
1. Drag prefab `Lembaran_AlQuran` ke scene
2. Atur position di berbagai lokasi labirin
3. **IMPORTANT:** Pastikan total ada **30 lembaran** sesuai `GameManager > Total Collectibles`
4. Tips distribusi:
   - Beberapa di jalur utama (mudah ditemukan)
   - Beberapa di dead-end (menantang)
   - Beberapa dekat checkpoint
   - Beberapa di area berbahaya (dekat spawn ghost)

---

## 7. SETUP CHECKPOINT ZONE

### 7.1 Membuat Checkpoint GameObject

#### A. Buat Trigger Zone
1. **GameObject > 3D Object > Cube**
2. Rename: `Checkpoint_01`
3. **Tag:** Checkpoint
4. Transform:
   - **Scale:** (4, 3, 4) - area cukup luas untuk player masuk

#### B. Setup Collider
1. Pastikan **Box Collider** ada
2. **✅ Is Trigger** - HARUS dicentang!
3. Hilangkan visual: Disable **Mesh Renderer** atau beri material transparan

#### C. Menambahkan CheckpointZone Script
1. **Add Component > Checkpoint Zone**
2. Setting:
   - **Checkpoint ID:** 1 (increment untuk checkpoint berikutnya: 2, 3, dst)
   - **Checkpoint Light:** Buat child **Point Light**
     - **Color:** Putih (akan berubah hijau saat aktif)
     - **Range:** 8
     - **Intensity:** 2
   - **Activate Sound:** Import audio checkpoint (.wav)

### 7.2 Membuat Multiple Checkpoints
Berdasarkan difficulty design:
- **Easy Mode:** 4 checkpoint (1 di spawn + 3 di jalur)
- **Normal Mode:** 3 checkpoint  
- **Hard Mode:** 2 checkpoint (minimal)

**Cara duplikat:**
1. Duplicate `Checkpoint_01`: **Ctrl+D**
2. Rename: `Checkpoint_02`, `Checkpoint_03`, dst
3. **PENTING:** Update **Checkpoint ID** di inspector untuk setiap checkpoint!
4. Posisikan di lokasi strategis labirin

### 7.3 Checkpoint sebagai SafeZone (Optional)
Jika ingin ghost tidak bisa masuk checkpoint:
1. Tambahkan script **SafeZone** atau gunakan tag
2. Di `GhostAI`, tambahkan logika untuk deteksi SafeZone

---

## 8. SETUP GHOST AI (HANTU)

### 8.1 Membuat GameObject Ghost

#### A. Buat Ghost Model
1. **GameObject > 3D Object > Capsule** (atau import 3D model hantu)
2. Rename: `Ghost_01`
3. **Tag:** Ghost
4. **Layer:** Ghost
5. Transform:
   - **Scale:** (1, 1.5, 1) - lebih tinggi dari player

#### B. Tambahkan Collider
1. Jika pakai capsule, sudah ada **Capsule Collider**
2. **JANGAN centang Is Trigger** (untuk collision damage)
3. Atau bisa pakai trigger terpisah untuk damage detection

#### C. Material & Visual
1. Buat material hitam/putih transparan dengan Emission
2. Tambahkan **Particle System** untuk efek smoke/aura (optional)

### 8.2 Membuat Node Grid untuk Pathfinding

⚠️ **PENTING:** GhostAI menggunakan algoritma Dijkstra dengan Node Grid!

#### A. Membuat Node Script (jika belum ada)
Buat script `Node.cs`:
```csharp
using UnityEngine;

[System.Serializable]
public class Node
{
    public Vector3 position;
    public bool isWalkable = true;
    public Node[] neighbors;

    public Node(Vector3 pos)
    {
        position = pos;
    }
}
```

#### B. Membuat Grid Builder (Manual Setup)
1. Buat Empty GameObject: `GridManager`
2. Buat script `GridBuilder.cs` untuk generate grid otomatis
3. Atau **MANUAL** (untuk pembelajaran):
   - Buat Empty GameObjects sebagai nodes
   - Position di intersection/junction labirin
   - Hubungkan neighbors manual via script

#### C. Setup Grid di Inspector
1. Pilih `Ghost_01`
2. **Add Component > Ghost AI**
3. Setting:
   - **Player:** Drag `Player` GameObject
   - **Grid:** Assign Node[,] array (complex - bisa lewat GridBuilder)
   - **Chase Range:** 
     - Easy: 8
     - Normal: 12
     - Hard: 15
   - **Move Speed:**
     - Easy: 3
     - Normal: 4
     - Hard: 5
   - **Node Reach Threshold:** 0.1

### 8.3 Duplikasi Ghost untuk Difficulty
Sesuai design:
- **Easy:** 2 ghost
- **Normal:** 3 ghost
- **Hard:** 4 ghost

1. Duplicate `Ghost_01`: **Ctrl+D**
2. Rename: `Ghost_02`, `Ghost_03`, dst
3. Posisikan di spawn point berbeda

### 8.4 Damage System Ghost
1. Tambahkan script `GhostDamage.cs`:
```csharp
using UnityEngine;

public class GhostDamage : MonoBehaviour
{
    public int damageAmount = 20;
    public float damageCooldown = 2f;
    private float lastDamageTime;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time - lastDamageTime > damageCooldown)
            {
                PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damageAmount);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}
```
2. **Add Component > Ghost Damage** ke setiap Ghost

---

## 9. SETUP EXIT TRIGGER (PINTU KELUAR)

### 9.1 Membuat Pintu Keluar

#### A. Buat GameObject Pintu
1. **GameObject > 3D Object > Cube** (atau model pintu 3D)
2. Rename: `ExitDoor`
3. Transform:
   - **Scale:** (3, 4, 0.3) - bentuk pintu
4. Tambahkan material/texture pintu

#### B. Buat Trigger Zone
1. Dalam `ExitDoor`: **Right Click > Create Empty**
2. Rename: `ExitTrigger`
3. **Add Component > Box Collider**
4. Setting:
   - **✅ Is Trigger**
   - **Size:** (3, 4, 2) - area di depan pintu

### 9.2 Menambahkan ExitTrigger Script
1. Pilih `ExitTrigger`
2. **Add Component > Exit Trigger**
3. Setting:
   - **Cutscene Controller:** Assign dari GameObject `CutsceneManager` (akan dibuat di bagian 10)
   - **Player:** Drag `Player` GameObject (atau biarkan null, auto-detect)

### 9.3 Visual Effect Pintu
1. Tambahkan **Point Light** di atas pintu (cahaya dramatis)
2. Particle System (optional): cahaya sparkle
3. Sound ambient: Audio Source dengan loop

---

## 10. SETUP CUTSCENE & ENDING

### 10.1 Membuat CutsceneManager GameObject
1. **GameObject > Create Empty**
2. Rename: `CutsceneManager`

### 10.2 Setup Timeline untuk Normal Ending

#### A. Buat Timeline Asset
1. **Assets > Create > Timeline**
2. Rename: `Timeline_NormalEnding`

#### B. Setup Timeline
1. Pilih `CutsceneManager`
2. **Window > Sequencing > Timeline**
3. Klik **Create** dan pilih `Timeline_NormalEnding`
4. Tambahkan tracks:
   - **Animation Track:** Animasi Saipul terbangun
   - **Audio Track:** BGM ending
   - **Cinemachine Track:** Camera movement (optional)

#### C. Add Playable Director
1. `CutsceneManager` otomatis dapat **Playable Director** component
2. Timeline sudah ter-assign

### 10.3 Setup Timeline untuk Secret Ending
1. Ulangi langkah A-C untuk `Timeline_SecretEnding`
2. Buat asset: `Timeline_SecretEnding`
3. Isi dengan cutscene Pak Ustadz

### 10.4 Menambahkan CutsceneController Script
1. Pilih `CutsceneManager`
2. **Add Component > Cutscene Controller**
3. Setting:
   - **Normal Ending Director:** Drag `CutsceneManager` (akan auto-detect Timeline_NormalEnding)
   - **Secret Ending Director:** Bisa duplicate GameObject atau buat baru dengan Timeline_SecretEnding
   - **Subtitle Controller:** Assign dari Canvas (next step)
   - **Fade Transition:** Assign dari Canvas
   - **BGM Normal:** Import audio (.mp3/.wav)
   - **BGM Secret:** Import audio

### 10.5 Setup Subtitle Controller
1. Dalam `Canvas`: **Right Click > UI > Text - TextMeshPro**
2. Rename: `SubtitleText`
3. Transform:
   - **Anchor:** Bottom center
   - **Pos Y:** 150
   - **Width:** 1600
   - **Height:** 200
4. TextMeshPro Settings:
   - **Font Size:** 28
   - **Color:** Putih
   - **Alignment:** Center
   - **Text:** (kosong)
5. **Add Component > Subtitle Controller** ke `Canvas`
6. Setting:
   - **Subtitle Text:** Drag `SubtitleText`
   - **Fade Speed:** 2

---

## 11. SETUP EFEK VISUAL & AUDIO

### 11.1 Damage Effect (Red Screen)

#### A. Buat Red Overlay UI
1. Dalam `Canvas`: **Right Click > UI > Image**
2. Rename: `RedOverlay`
3. Transform:
   - **Anchor:** Stretch All (full screen)
   - **Left, Right, Top, Bottom:** 0
4. Image Settings:
   - **Color:** Merah (R:255, G:0, B:0, A:0) - **Alpha 0 = transparan**
   - **Raycast Target:** ❌ Uncheck

#### B. Add DamageEffect Script
1. Pilih `Canvas`
2. **Add Component > Damage Effect**
3. Setting:
   - **Red Overlay:** Drag `RedOverlay`
   - **Fade Speed:** 1.5
   - **Max Alpha:** 0.6

#### C. Hubungkan ke PlayerHealth
1. Pilih `Player`
2. **PlayerHealth > Damage Effect:** Drag `Canvas`

### 11.2 Fade Transition (Death/Respawn)

#### A. Buat Fade Panel
1. Dalam `Canvas`: **Right Click > UI > Image**
2. Rename: `FadePanel`
3. Transform:
   - **Anchor:** Stretch All
   - **Left, Right, Top, Bottom:** 0
4. Image Settings:
   - **Color:** Hitam (A: 0)
   - **Raycast Target:** ❌ Uncheck

#### B. Add FadeTransition Script
1. Pilih `Canvas`
2. **Add Component > Fade Transition**
3. Setting:
   - **Fade Panel:** Drag `FadePanel`
   - **Fade Duration:** 1.2

#### C. Hubungkan ke PlayerHealth & CutsceneController
1. `Player > PlayerHealth > Fade Transition:` Drag `Canvas`
2. `CutsceneManager > CutsceneController > Fade Transition:` Drag `Canvas`

### 11.3 Audio Setup

#### A. Background Music
1. Dalam scene: **GameObject > Audio > Audio Source**
2. Rename: `BGM_Gameplay`
3. Settings:
   - **Audio Clip:** Import musik labirin (.mp3)
   - **✅ Play On Awake**
   - **✅ Loop**
   - **Volume:** 0.3-0.5

#### B. Ambient Sound
1. Duplicate Audio Source → `Ambient_Sound`
2. Audio Clip: Suara angin/horror
3. **✅ Loop**
4. **Volume:** 0.2

---

## 12. KONFIGURASI LEVEL DIFFICULTY

### 12.1 Setup Difficulty Selection (Pre-Game)
Buat scene terpisah untuk menu difficulty selection:

#### A. Buat Scene Menu
1. **File > New Scene**
2. Save as: `DifficultySelect`

#### B. Buat UI Buttons
1. Canvas > **3 Buttons:** Easy, Normal, Hard
2. Buat script `DifficultyManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyManager : MonoBehaviour
{
    public void SetDifficulty(int level)
    {
        PlayerPrefs.SetInt("Difficulty", level); // 0=Easy, 1=Normal, 2=Hard
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }
}
```

3. Attach ke Canvas, hubungkan buttons ke fungsi `SetDifficulty()`

### 12.2 Load Difficulty di GameScene

#### A. Update GameManager Start()
Tambahkan di `GameManager.cs > Start()`:

```csharp
void Start()
{
    int difficulty = PlayerPrefs.GetInt("Difficulty", 0); // default Easy
    
    switch(difficulty)
    {
        case 0: // Easy
            playerLives = 5;
            totalCollectibles = 30;
            // Activate 2 ghosts only
            ActivateGhosts(2);
            break;
        case 1: // Normal
            playerLives = 4;
            // Activate 3 ghosts
            ActivateGhosts(3);
            break;
        case 2: // Hard
            playerLives = 3;
            // Activate 4 ghosts
            ActivateGhosts(4);
            break;
    }
    
    UpdateHUD();
}

void ActivateGhosts(int count)
{
    GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
    for(int i = 0; i < ghosts.Length; i++)
    {
        ghosts[i].SetActive(i < count);
    }
}
```

### 12.3 Adjust Ghost Speed per Difficulty
Di `GhostAI.cs`, tambahkan di `Start()`:

```csharp
void Start()
{
    int difficulty = PlayerPrefs.GetInt("Difficulty", 0);
    
    switch(difficulty)
    {
        case 0: moveSpeed = 3f; chaseRange = 8f; break;  // Easy
        case 1: moveSpeed = 4f; chaseRange = 12f; break; // Normal
        case 2: moveSpeed = 5f; chaseRange = 15f; break; // Hard
    }
    
    // ... existing code
}
```

---

## 13. TESTING & TROUBLESHOOTING

### 13.1 Testing Checklist

#### ✅ Player Movement
- [ ] WASD untuk bergerak
- [ ] Space untuk lompat
- [ ] Tidak tembus dinding
- [ ] Tidak jatuh dari map

#### ✅ Camera Control
- [ ] Mouse gerakkan camera smooth
- [ ] Rotasi vertikal terbatas (-80° to +80°)
- [ ] Cursor terkunci saat game start

#### ✅ Collectibles
- [ ] Total 30 lembaran di map
- [ ] Counter bertambah saat collect
- [ ] Suara collect terdengar
- [ ] Lembaran hilang setelah diambil

#### ✅ Checkpoint
- [ ] Checkpoint aktif saat player masuk (light hijau)
- [ ] Player respawn di checkpoint terakhir setelah mati
- [ ] Suara aktivasi checkpoint

#### ✅ Ghost AI
- [ ] Ghost mengejar player saat dalam range
- [ ] Ghost kembali ke start position saat player jauh
- [ ] Jumlah ghost sesuai difficulty
- [ ] Damage player saat collision
- [ ] (Advanced) Ghost tidak masuk SafeZone

#### ✅ Health System
- [ ] Red overlay muncul saat damage
- [ ] Player mati setelah HP habis
- [ ] Fade transition saat respawn
- [ ] Lives counter berkurang

#### ✅ HUD
- [ ] Counter lembaran update real-time
- [ ] Lives display benar
- [ ] Text readable

#### ✅ Exit & Ending
- [ ] Pintu keluar bisa diakses
- [ ] **Normal Ending:** Trigger jika 30/30 lembaran terkumpul
- [ ] **Secret Ending:** Trigger jika < 30 lembaran
- [ ] Subtitle muncul saat cutscene
- [ ] BGM berganti
- [ ] Load scene credit setelah cutscene

### 13.2 Common Issues & Solutions

#### ❌ **Player jatuh terus**
- ✅ Pastikan Ground memiliki Collider
- ✅ Layer Ground sudah di-assign
- ✅ CharacterController > Skin Width tidak 0

#### ❌ **Camera tidak rotate**
- ✅ Cek CameraController > Player Body sudah di-assign
- ✅ Pastikan camera adalah child dari Player
- ✅ Input Manager > Mouse X/Y sensitivity tidak 0

#### ❌ **Collectible tidak hilang**
- ✅ Collider > Is Trigger harus ✅
- ✅ Player GameObject harus punya Tag "Player"
- ✅ GameManager ada di scene

#### ❌ **Ghost tidak bergerak**
- ✅ Grid/Node harus di-setup (complex - cek GhostAI.cs)
- ✅ Player reference di-assign
- ✅ Cek console untuk error pathfinding

#### ❌ **Checkpoint tidak save**
- ✅ GameManager.Instance != null
- ✅ CheckpointZone > Collider is Trigger
- ✅ Player Tag benar

#### ❌ **HUD tidak update**
- ✅ GameManager > HUD Controller ter-assign
- ✅ HUDController > TextMeshProUGUI ter-assign
- ✅ Cek GameManager.UpdateHUD() dipanggil

#### ❌ **Ending tidak trigger**
- ✅ ExitTrigger > CutsceneController ter-assign
- ✅ Total collectibles di GameManager = 30
- ✅ Timeline assets ada dan configured

---

## 📚 LAMPIRAN: STRUKTUR SCENE FINAL

```
Hierarchy View:
├── GameManager
├── Player
│   ├── Main Camera (CameraController)
│   ├── GroundCheck
│   └── PlayerModel (Capsule)
├── Canvas
│   ├── CollectibleText
│   ├── LivesText
│   ├── SubtitleText
│   ├── RedOverlay
│   └── FadePanel
├── EventSystem
├── CutsceneManager (Timeline Directors)
├── Labirin
│   ├── Walls
│   ├── Floor
│   └── Ceiling
├── Collectibles
│   ├── Lembaran_01
│   ├── Lembaran_02
│   ├── ... (total 30)
│   └── Lembaran_30
├── Checkpoints
│   ├── Checkpoint_01
│   ├── Checkpoint_02
│   ├── Checkpoint_03
│   └── Checkpoint_04 (Easy mode only)
├── Ghosts
│   ├── Ghost_01
│   ├── Ghost_02
│   ├── Ghost_03 (Normal+)
│   └── Ghost_04 (Hard only)
├── ExitDoor
│   └── ExitTrigger
├── Lighting
│   ├── Directional Light
│   └── Point Lights
└── Audio
    ├── BGM_Gameplay
    └── Ambient_Sound
```

---

## 🎮 RINGKASAN INTEGRASI

| **Component** | **Script** | **GameObject** | **Key Settings** |
|---------------|------------|----------------|------------------|
| Player Movement | PlayerController.cs | Player | Speed, CameraTransform |
| Camera FPS | CameraController.cs | Main Camera (child) | MouseSensitivity, PlayerBody |
| Health System | PlayerHealth.cs | Player | MaxHealth, DamageEffect |
| Game Logic | GameManager.cs | GameManager | TotalCollectibles=30, Lives |
| UI Display | HUDController.cs | Canvas | CollectibleText, LivesText |
| Collectible | CollectibleItem.cs | Lembaran_AlQuran (x30) | Is Trigger = ✅ |
| Checkpoint | CheckpointZone.cs | Checkpoint_01-04 | Is Trigger = ✅ |
| Enemy AI | GhostAI.cs | Ghost_01-04 | ChaseRange, MoveSpeed |
| Exit Gate | ExitTrigger.cs | ExitTrigger | CutsceneController |
| Cutscene | CutsceneController.cs | CutsceneManager | Timeline Directors |
| Subtitle | SubtitleController.cs | Canvas | SubtitleText |
| Visual FX | DamageEffect.cs | Canvas | RedOverlay |
| Transition | FadeTransition.cs | Canvas | FadePanel |

---

## ✅ CHECKLIST FINAL SEBELUM BUILD

- [ ] Semua script sudah di-attach ke GameObject yang benar
- [ ] Semua reference di Inspector sudah di-assign (tidak ada "None")
- [ ] Tags sudah benar (Player, Ghost, Collectible, Checkpoint)
- [ ] Layers sudah setup (Ground, Wall)
- [ ] Total 30 lembaran ada di scene
- [ ] Jumlah checkpoint sesuai difficulty (Easy=4, Normal=3, Hard=2)
- [ ] Jumlah ghost sesuai difficulty (Easy=2, Normal=3, Hard=4)
- [ ] GameManager Singleton (hanya 1 instance)
- [ ] Timeline untuk Normal & Secret Ending sudah configured
- [ ] Audio clips (BGM, SFX) sudah import dan di-assign
- [ ] HUD Text readable dan positioned correctly
- [ ] Testing semua gameplay mechanics
- [ ] Build Settings > Scene "GameScene" dan "CreditScene" added

---

## 📞 SUPPORT & NEXT STEPS

Jika ada error atau bug:
1. Cek **Console** untuk error messages
2. Pastikan semua **References** di Inspector tidak null
3. Test di **Play Mode** step by step
4. Review script comments untuk penjelasan logic

**Good luck dengan project game UAS Anda! 🎮✨**

---

**Dokumentasi dibuat untuk:**  
Project: Labirin Al-Qur'an - Perjalanan Saipul  
Mata Kuliah: Multimedia  
Semester: 5  
Tanggal: 8 November 2025
