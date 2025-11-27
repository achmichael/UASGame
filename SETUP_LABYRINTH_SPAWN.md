# PANDUAN SETUP LABYRINTH SPAWN SYSTEM
## Spawn Random untuk Player, Item, dan Enemy di Dalam Labirin

---

## 📋 OVERVIEW

Sistem ini memastikan:
- ✅ Spawn random untuk Player, Items, dan Enemies
- ✅ **TIDAK menembus dinding** labirin
- ✅ Spawn **HANYA di dalam labirin**, bukan di luar
- ✅ Validasi posisi spawn (walkable area)
- ✅ Jarak minimum antar spawns
- ✅ Player dan Enemy tidak bisa menembus wall saat bergerak

---

## 🎯 KOMPONEN YANG DIBUAT

### 1. **LabyrinthSpawnManager.cs**
- Mengelola spawn random untuk semua objects
- Validasi posisi tidak menembus dinding
- Integrasi dengan GridBuilder untuk walkable area

### 2. **WallCollisionPreventer.cs**
- Mencegah Player/Ghost menembus dinding saat bergerak
- Auto-correct posisi jika terdeteksi menembus wall
- Real-time wall detection

### 3. **GameManager.cs** (Updated)
- Integrasi dengan LabyrinthSpawnManager
- Respawn player menggunakan spawn manager

---

## 🔧 STEP-BY-STEP SETUP

### STEP 1: Setup Layer untuk Wall

1. **Edit > Project Settings > Tags and Layers**
2. Tambahkan layer baru: **Wall**
3. Assign layer **Wall** ke semua GameObject wall/dinding di labirin:
   - Pilih semua wall di Hierarchy
   - Inspector > Layer > **Wall**

---

### STEP 2: Setup Labyrinth Parent

1. Buat Empty GameObject: **GameObject > Create Empty**
2. Rename: **Labyrinth**
3. Jadikan semua wall/floor/ceiling labirin sebagai **child** dari Labyrinth:
   ```
   Labyrinth (Parent)
   ├── Walls
   ├── Floor
   ├── Ceiling
   └── Props
   ```

---

### STEP 3: Setup LabyrinthSpawnManager

#### A. Buat GameObject Spawn Manager
1. **GameObject > Create Empty**
2. Rename: **SpawnManager**
3. **Add Component > Labyrinth Spawn Manager**

#### B. Configure Inspector Settings

```yaml
Labyrinth Bounds:
  Labyrinth Parent: [Drag "Labyrinth" GameObject]
  Labyrinth Center: (0, 0, 0)  # Auto-detected jika parent assigned
  Labyrinth Size: (50, 5, 50)  # Auto-detected jika parent assigned
  Ground Level: 0

Spawn Settings:
  Wall Layer: Wall  # Pilih layer Wall
  Wall Check Radius: 0.5
  Max Spawn Attempts: 100
  Min Distance Between Spawns: 2

Player Spawn:
  Player Prefab: [Drag Player prefab]
  Player Spawn Attempts: 50
  Safe Player Spawn Zones: # Optional - area aman untuk spawn
    - Size: 0
    - Element 0: (5, 0, 5)   # Contoh safe zone
    - Element 1: (-5, 0, -5) # Contoh safe zone

Item Spawn:
  Item Prefab: [Drag Lembaran_AlQuran prefab]
  Item Count: 30
  Item Spawn Height: 1

Enemy Spawn:
  Enemy Prefab: [Drag Ghost prefab]
  Enemy Count: 3
  Min Distance From Player: 10

References:
  Grid Builder: [Auto-detect atau drag GridManager]

Visualization:
  Show Gizmos: ✓ (untuk debug)
  Spawn Area Color: Green semi-transparent
```

---

### STEP 4: Setup Player Prefab

1. Pilih **Player** prefab atau GameObject
2. **Add Component > Wall Collision Preventer**
3. Settings:
   ```
   Wall Detection:
     Wall Layer: Wall
     Detection Radius: 0.5
     Push Back Force: 2
     Auto Correct Position: ✓
   
   Debug:
     Show Debug Rays: ✓ (untuk testing)
   ```

---

### STEP 5: Setup Ghost/Enemy Prefab

1. Pilih **Ghost** prefab atau GameObject
2. **Add Component > Wall Collision Preventer**
3. Settings sama seperti Player
4. Pastikan Ghost punya:
   - ✅ Rigidbody (Is Kinematic = ✓, Freeze Rotation = ✓)
   - ✅ Collider (Capsule/Box)
   - ✅ Tag = "Ghost"

---

### STEP 6: Setup GameManager Integration

1. Pilih **GameManager** GameObject
2. Inspector > Game Manager Component:
   ```
   References:
     Spawn Manager: [Drag SpawnManager GameObject]
   ```

---

### STEP 7: Setup Wall Colliders

**PENTING:** Semua wall HARUS punya Collider!

1. Pilih semua wall GameObject
2. Pastikan ada **Box Collider** atau **Mesh Collider**
3. Settings:
   ```
   Box Collider:
     Is Trigger: ✗ (UNCHECK - harus collision biasa)
     Center: (0, 0, 0)
     Size: Sesuaikan dengan wall
   ```

---

## 🎮 CARA PAKAI

### Opsi 1: Auto-Spawn saat Scene Load (Recommended)

1. Buat script `GameInitializer.cs`:

```csharp
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public LabyrinthSpawnManager spawnManager;
    public bool autoSpawnOnStart = true;
    
    void Start()
    {
        if (autoSpawnOnStart && spawnManager != null)
        {
            // Spawn semua objects
            spawnManager.SpawnAllObjects();
            Debug.Log("[GameInitializer] All objects spawned!");
        }
    }
}
```

2. Attach ke GameObject (misal SpawnManager)
3. Assign `spawnManager` reference
4. Play → Otomatis spawn semua

---

### Opsi 2: Manual Spawn via Script

```csharp
// Di script lain (misal GameManager)
void InitializeGame()
{
    LabyrinthSpawnManager spawnManager = FindObjectOfType<LabyrinthSpawnManager>();
    
    if (spawnManager != null)
    {
        spawnManager.SpawnAllObjects();
    }
}

// Spawn item tambahan
void SpawnExtraItem(Vector3 nearPosition)
{
    spawnManager.SpawnSingleItem(nearPosition);
}

// Spawn enemy tambahan
void SpawnExtraEnemy(Vector3 nearPosition)
{
    spawnManager.SpawnSingleEnemy(nearPosition);
}
```

---

### Opsi 3: Manual Placement + Auto-Validation

Jika sudah ada items/enemies di scene manual:
1. Uncheck `auto Spawn On Start`
2. Items/enemies tetap di posisi manual
3. WallCollisionPreventer akan auto-correct jika ada yang menembus

---

## 🧪 TESTING & VALIDATION

### Test 1: Spawn Position Validation
1. Play Mode
2. Check Console:
   ```
   [SpawnManager] Auto-detected labyrinth bounds: Center=(0,0,0), Size=(50,5,50)
   [SpawnManager] Player spawned at (12.3, 1, -8.5)
   [SpawnManager] Spawned 30/30 items in 45 attempts
   [SpawnManager] Spawned 3/3 enemies in 12 attempts
   ```

3. Check Scene View:
   - **Green wireframe cube** = Labyrinth bounds
   - **Green spheres** = Safe player spawn zones
   - **Cyan spheres** = Spawned positions
   - **Blue sphere** = Player spawn position

### Test 2: Wall Collision Prevention
1. Play Mode
2. Jalan player ke wall dengan **W**
3. **Expected:**
   - ✅ Player **TIDAK menembus** wall
   - ✅ Player **terhenti** saat menabrak wall
   - ✅ Jika somehow menembus → Auto-correct kembali

4. Check Console jika ada warning:
   ```
   [WallPreventer] Player corrected to last valid position: (10, 1, 5)
   ```

### Test 3: Enemy Movement
1. Play Mode
2. Tunggu enemy chase player
3. **Expected:**
   - ✅ Enemy mengikuti path
   - ✅ Enemy **TIDAK menembus** wall
   - ✅ Path hanya melalui walkable area

### Test 4: Item Spawn Distribution
1. Play Mode
2. Jalan explore labirin
3. **Expected:**
   - ✅ Items tersebar merata di labirin
   - ✅ **TIDAK ada items di luar labirin**
   - ✅ **TIDAK ada items di dalam wall**
   - ✅ Jarak antar items minimal 2 unit

### Test 5: Player Respawn
1. Play Mode
2. Trigger death (misal tekan F atau kena ghost)
3. **Expected:**
   - ✅ Player respawn di posisi random **AMAN**
   - ✅ **TIDAK spawn di dalam wall**
   - ✅ **TIDAK spawn di luar labirin**

---

## ⚙️ PARAMETER TUNING

### Jika Spawn Gagal (Not enough valid positions)

**Problem:** `Spawned 15/30 items in 3000 attempts`

**Solution:**
```csharp
// Di LabyrinthSpawnManager Inspector
Max Spawn Attempts: 200 // Naikkan dari 100
Wall Check Radius: 0.3  // Kurangi dari 0.5 (lebih toleran)
Min Distance Between Spawns: 1.5 // Kurangi dari 2
```

---

### Jika Player/Enemy Menembus Wall

**Problem:** Player atau Ghost bisa jalan menembus dinding

**Solution 1: Check Layer**
- Pastikan wall layer = "Wall"
- Pastikan WallCollisionPreventer wallLayer = "Wall"

**Solution 2: Adjust Detection**
```csharp
// Di WallCollisionPreventer Inspector
Detection Radius: 0.7  // Naikkan dari 0.5
Push Back Force: 3     // Naikkan dari 2
Auto Correct Position: ✓ // Pastikan centang
```

**Solution 3: Wall Collider**
- Pastikan wall punya Collider
- Pastikan Collider **NOT trigger** (is Trigger = unchecked)

---

### Jika Spawn Terlalu Dekat/Jauh

**Terlalu Dekat:**
```csharp
Min Distance Between Spawns: 3  // Naikkan dari 2
Min Distance From Player: 15    // Naikkan dari 10
```

**Terlalu Jauh (items susah dicari):**
```csharp
Min Distance Between Spawns: 1  // Kurangi dari 2
```

---

### Jika Labyrinth Bounds Tidak Sesuai

**Manual Override:**
```csharp
// Di LabyrinthSpawnManager Inspector
Labyrinth Center: (0, 0, 0)     // Sesuaikan dengan center labirin
Labyrinth Size: (100, 10, 100)  // Sesuaikan dengan ukuran actual
```

**Check Bounds Visual:**
1. Play Mode
2. Scene View
3. Lihat green wireframe cube
4. Adjust size sampai cube pas dengan labirin

---

## 🎯 SAFE PLAYER SPAWN ZONES (OPTIONAL)

Jika ingin player selalu spawn di area tertentu (misal dekat entrance):

```csharp
// Di LabyrinthSpawnManager Inspector
Safe Player Spawn Zones:
  Size: 3
  Element 0: (5, 0, 5)    // Near entrance
  Element 1: (0, 0, 0)    // Center
  Element 2: (-5, 0, -5)  // Other safe area
```

Player akan spawn random di salah satu safe zone ini.

---

## 📊 ARCHITECTURE DIAGRAM

```
Labyrinth
    │
    ├─── Walls (Layer: Wall)
    ├─── Floor
    └─── Ceiling
    
SpawnManager (LabyrinthSpawnManager)
    │
    ├─── DetectLabyrinthBounds()
    ├─── SpawnAllObjects()
    │     ├─── SpawnPlayer()
    │     ├─── SpawnItems(30)
    │     └─── SpawnEnemies(3)
    │
    └─── For each spawn:
          ├─── FindValidSpawnPosition()
          ├─── IsValidSpawnPosition()
          │     ├─── IsInsideLabyrinthBounds() ✓
          │     ├─── IsPositionBlocked() ✓
          │     ├─── HasGroundBelow() ✓
          │     └─── Check GridBuilder walkable ✓
          └─── Instantiate prefab

Player/Ghost (WallCollisionPreventer)
    │
    └─── Every frame:
          ├─── IsInsideWall() ?
          │     └─── YES → CorrectPosition()
          │           ├─── Return to lastValidPosition
          │           └─── OR PushAwayFromWall()
          └─── Update lastValidPosition
```

---

## ✅ CHECKLIST FINAL

### Setup Labyrinth:
- [ ] Layer "Wall" dibuat
- [ ] Semua wall assigned layer "Wall"
- [ ] Labyrinth parent GameObject dibuat
- [ ] Semua wall/floor/ceiling sebagai child
- [ ] Semua wall punya Collider (is Trigger = unchecked)

### Setup Spawn Manager:
- [ ] SpawnManager GameObject dibuat
- [ ] LabyrinthSpawnManager component added
- [ ] Labyrinth Parent assigned
- [ ] Player Prefab assigned
- [ ] Item Prefab assigned
- [ ] Enemy Prefab assigned
- [ ] Wall Layer = "Wall"

### Setup Player:
- [ ] WallCollisionPreventer added
- [ ] Wall Layer = "Wall"
- [ ] Detection Radius set (0.5)
- [ ] Tag = "Player"

### Setup Ghost:
- [ ] WallCollisionPreventer added
- [ ] Wall Layer = "Wall"
- [ ] Rigidbody (Is Kinematic, Freeze Rotation)
- [ ] Tag = "Ghost"

### Setup GameManager:
- [ ] Spawn Manager reference assigned
- [ ] Respawn menggunakan SpawnManager

### Testing:
- [ ] Play Mode → Check spawns
- [ ] Semua objects spawn di DALAM labirin
- [ ] TIDAK ada objects di luar labirin
- [ ] Player tidak menembus wall
- [ ] Enemy tidak menembus wall
- [ ] Respawn bekerja (posisi random aman)

---

## 🐛 TROUBLESHOOTING

### Problem: "Spawned 0/30 items"
- ✅ Check Wall Layer assigned ke walls
- ✅ Check Labyrinth Bounds sesuai
- ✅ Naikkan Max Spawn Attempts
- ✅ Kurangi Wall Check Radius

### Problem: Items spawn di luar labirin
- ✅ Check Labyrinth Size di Inspector
- ✅ Visualize bounds (green wireframe cube)
- ✅ Adjust Labyrinth Center dan Size

### Problem: Player menembus wall
- ✅ Check wall punya Collider
- ✅ Check wall layer = "Wall"
- ✅ Check WallCollisionPreventer active
- ✅ Naikkan Detection Radius

### Problem: Enemy stuck di wall
- ✅ Check GridBuilder grid coverage
- ✅ Check Ghost AI path finding
- ✅ Add WallCollisionPreventer ke Ghost
- ✅ Check wall punya Collider yang proper

---

## 🎓 ADVANCED FEATURES

### Custom Spawn Zones per Difficulty

```csharp
// Di LabyrinthSpawnManager, tambahkan:
public Transform[] easySpawnZones;
public Transform[] hardSpawnZones;

void SpawnByDifficulty()
{
    int difficulty = PlayerPrefs.GetInt("Difficulty", 0);
    
    Transform[] zones = difficulty == 0 ? easySpawnZones : hardSpawnZones;
    // Spawn di zones tertentu
}
```

### Dynamic Spawn based on Player Progress

```csharp
// Spawn item tambahan saat collect checkpoint
void OnCheckpointReached()
{
    spawnManager.SpawnSingleItem(checkpointPosition);
}
```

### Respawn Animation

```csharp
// Di GameManager, tambahkan fade before respawn
IEnumerator RespawnWithFade()
{
    fadeTransition.FadeOut();
    yield return new WaitForSeconds(1f);
    
    Vector3 respawnPos = spawnManager.GetPlayerRespawnPosition();
    player.transform.position = respawnPos;
    
    fadeTransition.FadeIn();
}
```

---

**System siap digunakan! Semua spawn akan aman, random, dan TIDAK menembus dinding! 🎮✨**
