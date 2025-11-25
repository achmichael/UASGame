// CameraController.cs
// Mengatur rotasi kamera First-Person berdasarkan input mouse
// - Batasi rotasi vertikal agar tidak flip 360 derajat
// - Rotasi horizontal memutar badan player (playerBody)

using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public Transform playerBody; // assign player root transform (not camera)
    public bool lockCursorOnStart = true;
    
    // Collision control
    private bool isRotationLocked = false;
    private float rotationLockTimer = 0f;

    float xRotation = 0f;

    void Start()
    {
        if (lockCursorOnStart)
            Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Update rotation lock timer
        if (rotationLockTimer > 0)
        {
            rotationLockTimer -= Time.deltaTime;
            if (rotationLockTimer <= 0)
            {
                isRotationLocked = false;
            }
        }
        
        // Check jika player collision dengan ghost
        PlayerController pc = playerBody?.GetComponent<PlayerController>();
        if (pc != null)
        {
            // Akses via reflection atau public property jika ada collision
            // Untuk sekarang, kita kurangi sensitivity saja
        }
        
        // Kurangi sensitivity jika sedang rotation locked
        float currentSensitivity = isRotationLocked ? mouseSensitivity * 0.2f : mouseSensitivity;
        
        // Ambil input mouse (per frame)
        float mouseX = Input.GetAxis("Mouse X") * currentSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * currentSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // batas vertikal

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        if (playerBody != null && !isRotationLocked)
        {
            // Smooth rotation untuk mencegah jerk
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
    
    // Method untuk lock rotation dari external script
    public void LockRotation(float duration)
    {
        isRotationLocked = true;
        rotationLockTimer = duration;
    }
}
