using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 20;
    public float damageCooldown = 2f;
    public float knockbackForce = 5f;  // Kekuatan dorongan mundur
    public float pushbackStrength = 2f; // Kekuatan push untuk separasi
    
    private float lastDamageTime;
    private Rigidbody ghostRb;

    void Start()
    {
        ghostRb = GetComponent<Rigidbody>();
        
        // Pastikan Rigidbody ghost ter-freeze rotasinya
        if (ghostRb != null)
        {
            ghostRb.freezeRotation = true; // Freeze semua rotasi
            ghostRb.constraints = RigidbodyConstraints.FreezeRotation; // Alternative
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Knockback player agar terdorong mundur, bukan berputar
            ApplyKnockback(collision);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Damage dengan cooldown
            if (Time.time - lastDamageTime > damageCooldown)
            {
                PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damageAmount);
                    lastDamageTime = Time.time;
                    Debug.Log($"Ghost damaged player for {damageAmount} HP!");
                    
                    // Apply knockback saat damage
                    ApplyKnockback(collision);
                }
                else
                {
                    Debug.LogWarning("PlayerHealth component tidak ditemukan di Player!");
                }
            }
            
            // Continuous pushback untuk separasi
            PushPlayerAway(collision.gameObject);
        }
    }
    
    void ApplyKnockback(Collision collision)
    {
        GameObject player = collision.gameObject;
        CharacterController cc = player.GetComponent<CharacterController>();
        
        if (cc != null)
        {
            // Hitung arah knockback (dari ghost ke player)
            Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
            knockbackDirection.y = 0; // Hanya horizontal knockback
            
            // Apply knockback via CharacterController.Move
            cc.Move(knockbackDirection * knockbackForce * Time.deltaTime);
        }
    }
    
    void PushPlayerAway(GameObject player)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        
        if (cc != null)
        {
            Vector3 pushDirection = (player.transform.position - transform.position).normalized;
            pushDirection.y = 0;
            
            // Gentle continuous push untuk separasi
            cc.Move(pushDirection * pushbackStrength * Time.deltaTime);
        }
    }
}
