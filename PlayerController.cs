// PlayerController.cs
// Mengatur pergerakan karakter utama (Saipul) dalam mode First-Person
// - Menggunakan CharacterController
// - Input WASD, Jump (Space)
// - Gravity handling dan mencegah menembus dinding (CharacterController menanganinya)
// - Terhubung dengan CameraController melalui inspector (cameraTransform)

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpForce = 2f;

    [Header("References")]
    public Transform cameraTransform; // assign main camera (child of player) in inspector

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    
    // Collision control
    private bool isCollidingWithEnemy = false;
    private float collisionCooldown = 0f;

    // Ground check helpers (optional: better ground detection)
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Update collision cooldown
        if (collisionCooldown > 0)
        {
            collisionCooldown -= Time.deltaTime;
            if (collisionCooldown <= 0)
            {
                isCollidingWithEnemy = false;
            }
        }
        
        // Mengecek apakah player menyentuh tanah
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            // fallback ke CharacterController.isGrounded
            isGrounded = controller.isGrounded;
        }

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // kecilkan untuk menjaga kontak tanah

        // Input arah (lokal terhadap orientasi player)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        // Normalisasi diagonal movement jika diperlukan (CharacterController.Move menangani deltaTime)
        if (move.magnitude > 1f) move = move.normalized;
        
        // Kurangi speed saat collision dengan enemy untuk mencegah stuck
        float currentSpeed = isCollidingWithEnemy ? speed * 0.3f : speed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Lompat
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        // Gravitasi
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Deteksi collision dengan CharacterController
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check jika menabrak ghost
        if (hit.gameObject.CompareTag("Ghost"))
        {
            isCollidingWithEnemy = true;
            collisionCooldown = 0.3f; // Reset cooldown
            
            // Push player away dari ghost
            Vector3 pushDirection = (transform.position - hit.transform.position).normalized;
            pushDirection.y = 0;
            controller.Move(pushDirection * 2f * Time.deltaTime);
        }
    }

    // Optional: public API to disable movement (for cutscenes)
    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
}
