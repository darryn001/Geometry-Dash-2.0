using UnityEngine;

// Attach to any spike/obstacle GameObject.
public class Spike : MonoBehaviour
{
    [Header("Hitbox")]
    public bool autoFitTrigger = false;
    public Vector2 minimumTriggerSize = new Vector2(0.13f, 0.08f);
    public float minimumTriggerYOffset = 0f;

    private void Awake()
    {
        if (!autoFitTrigger) return;

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null) return;

        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(
            Mathf.Max(boxCollider.size.x, minimumTriggerSize.x),
            Mathf.Max(boxCollider.size.y, minimumTriggerSize.y)
        );
        boxCollider.offset = new Vector2(boxCollider.offset.x, Mathf.Max(boxCollider.offset.y, minimumTriggerYOffset));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerPlayerLoseState(other);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            TriggerPlayerLoseState(collision.collider);
        }
    }

    private void TriggerPlayerLoseState(Collider2D playerCollider)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        PlayerController player = playerCollider.GetComponent<PlayerController>();
        Collider2D spikeCollider = GetComponent<Collider2D>();
        if (player != null && !player.ShouldTakeSpikeDamage(spikeCollider)) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayHitSound();
        GameManager.Instance.TriggerGameOver();
    }
}
