using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 9f;
    public LayerMask groundLayer; // set this to whatever layer your ground/platforms are on

    [Header("Ground Check")]
    public Transform groundCheck;   // empty child GameObject placed at the player's feet
    public float groundCheckRadius = 0.15f;

    [Header("Knockback")]
    public float knockbackDuration = 0.25f; // how long the auto-run override is paused after a hit
    private float knockbackTimer = 0f;

    private Rigidbody2D rb;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Ground check via small overlap circle at the player's feet
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Only accept jump input while the game is actively being played
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // reset vertical velocity first
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    private void FixedUpdate()
    {
        // While knocked back, let physics carry the impulse instead of forcing run speed.
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        // Auto-run: horizontal speed is driven entirely by GameManager's ramping speed value.
        // While waiting to start or game over, horizontal speed is locked to 0.
        float targetSpeedX = 0f;

        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            targetSpeedX = GameManager.Instance.CurrentSpeed;
        }

        rb.linearVelocity = new Vector2(targetSpeedX, rb.linearVelocity.y);
    }

    // Called by Barrel.cs to apply a knockback impulse
    public void ApplyKnockback(Vector2 force)
    {
        knockbackTimer = knockbackDuration; // pause auto-run override so the impulse is actually visible
        rb.linearVelocity = Vector2.zero; // cancel current momentum so the knockback feels consistent
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}