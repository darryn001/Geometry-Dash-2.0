using UnityEngine;

// Attach to any barrel/hazard GameObject that should knock the player back and cost 1 heart
// (as opposed to Spikes, which end the run immediately with no health involved).
// Requires a Collider2D on this object set to "Is Trigger".
public class Barrel : MonoBehaviour
{
    [Header("Knockback")]
    public Vector2 knockbackForce = new Vector2(-6f, 6f); // pushes player back and slightly up

    private Collider2D barrelCollider;

    private void Awake()
    {
        barrelCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ApplyKnockback(knockbackForce);
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayHitSound();

        // No argument needed - GameManager.TakeDamage() always takes 1 heart,
        // and handles the invincibility cooldown internally.
        GameManager.Instance.TakeDamage();

        // Prevent the barrel from triggering repeatedly while the player is inside it
        barrelCollider.enabled = false;
    }

}
