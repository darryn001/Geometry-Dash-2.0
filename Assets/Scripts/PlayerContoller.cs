using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 9.5f;
    public float airSpeedMultiplier = 0.68f;
    public float railSpeedMultiplier = 0.55f;
    public float railMountJumpSpeedMultiplier = 0.1f;
    public float railMountJumpDuration = 0.28f;
    public float coyoteTime = 0.18f;
    public LayerMask groundLayer; // set this to whatever layer your ground/platforms are on

    [Header("Ground Check")]
    public Transform groundCheck;   // empty child GameObject placed at the player's feet
    public float groundCheckRadius = 0.15f;

    [Header("Hazard Check")]
    public float spikeFootCheckWidthScale = 0.45f;
    public float spikeFootCheckHeight = 0.06f;
    public float railSpikeProtectionGrace = 0.25f;

    [Header("Knockback")]
    public float knockbackDuration = 0.25f; // how long the auto-run override is paused after a hit
    private float knockbackTimer = 0f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Collider2D[] bypassRailColliders;
    private bool isGrounded;
    private float railProtectionTimer;
    private float coyoteTimer;
    private float railMountJumpTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        CacheBypassRailColliders();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Ground check via small overlap circle at the player's feet.
        // Thin rails count too, otherwise the player can land there but not jump from it.
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) || IsStandingOnBypassRail();
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else if (coyoteTimer > 0f)
        {
            coyoteTimer -= Time.deltaTime;
        }

        UpdateRailProtectionTimer();

        // Only accept jump input while the game is actively being played
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Space) && coyoteTimer > 0f)
            {
                if (IsBlockedByRailFront())
                {
                    railMountJumpTimer = railMountJumpDuration;
                }

                coyoteTimer = 0f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // reset vertical velocity first
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayJumpSound();
                }
            }
        }

        UpdateRailCollisionBypass();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null) return;

        TryTriggerOverlappingSpikeLoseState();

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
            if (railMountJumpTimer > 0f)
            {
                railMountJumpTimer -= Time.fixedDeltaTime;
                targetSpeedX *= railMountJumpSpeedMultiplier;
            }
            else if (!isGrounded)
            {
                targetSpeedX *= airSpeedMultiplier;
            }
            else if (IsStandingOnBypassRail())
            {
                targetSpeedX *= railSpeedMultiplier;
            }
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

    private void CacheBypassRailColliders()
    {
        Collider2D[] sceneColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        System.Collections.Generic.List<Collider2D> railColliders = new System.Collections.Generic.List<Collider2D>();

        foreach (Collider2D sceneCollider in sceneColliders)
        {
            if (sceneCollider == playerCollider) continue;
            if (IsJumpBypassRail(sceneCollider))
            {
                railColliders.Add(sceneCollider);
            }
        }

        bypassRailColliders = railColliders.ToArray();
    }

    private bool IsJumpBypassRail(Collider2D other)
    {
        if (other == null) return false;
        if (!other.gameObject.name.StartsWith("Platform")) return false;

        bool isOnGroundLayer = (groundLayer.value & (1 << other.gameObject.layer)) != 0;
        return isOnGroundLayer && other.bounds.size.y <= 0.7f;
    }

    private void UpdateRailCollisionBypass()
    {
        if (playerCollider == null || bypassRailColliders == null) return;

        Bounds playerBounds = playerCollider.bounds;

        foreach (Collider2D railCollider in bypassRailColliders)
        {
            if (railCollider != null)
            {
                Bounds railBounds = railCollider.bounds;
                bool playerFeetAreBelowRailTop = playerBounds.min.y < railBounds.max.y - 0.03f;
                bool shouldBypassRails = rb.linearVelocity.y > 0.05f && playerFeetAreBelowRailTop;
                Physics2D.IgnoreCollision(playerCollider, railCollider, shouldBypassRails);
            }
        }
    }

    private void UpdateRailProtectionTimer()
    {
        if (IsStandingOnBypassRail())
        {
            railProtectionTimer = railSpikeProtectionGrace;
        }
        else if (railProtectionTimer > 0f)
        {
            railProtectionTimer -= Time.deltaTime;
        }
    }

    private bool IsStandingOnBypassRail()
    {
        if (playerCollider == null || bypassRailColliders == null) return false;

        Bounds playerBounds = playerCollider.bounds;

        foreach (Collider2D railCollider in bypassRailColliders)
        {
            if (railCollider == null) continue;

            Bounds railBounds = railCollider.bounds;
            bool feetAreOnRail = Mathf.Abs(playerBounds.min.y - railBounds.max.y) <= 0.18f;
            bool playerOverlapsRailX = playerBounds.max.x > railBounds.min.x && playerBounds.min.x < railBounds.max.x;

            if (feetAreOnRail && playerOverlapsRailX)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlockedByRailFront()
    {
        if (playerCollider == null || bypassRailColliders == null) return false;

        Bounds playerBounds = playerCollider.bounds;

        foreach (Collider2D railCollider in bypassRailColliders)
        {
            if (railCollider == null) continue;

            Bounds railBounds = railCollider.bounds;
            bool playerIsBelowRailTop = playerBounds.min.y < railBounds.max.y - 0.03f;
            bool playerIsAtRailFront = Mathf.Abs(playerBounds.max.x - railBounds.min.x) <= 0.2f;
            bool playerOverlapsRailHeight = playerBounds.max.y > railBounds.min.y && playerBounds.min.y < railBounds.max.y;

            if (playerIsBelowRailTop && playerIsAtRailFront && playerOverlapsRailHeight)
            {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTriggerSpikeLoseState(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTriggerSpikeLoseState(collision.collider);
    }

    private void TryTriggerSpikeLoseState(Collider2D other)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (other.GetComponentInParent<Spike>() == null) return;
        if (!ShouldTakeSpikeDamage(other)) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHitSound();
        }

        GameManager.Instance.TriggerGameOver();
    }

    public bool ShouldTakeSpikeDamage(Collider2D spikeCollider)
    {
        if (IsProtectedFromSpike(spikeCollider)) return false;
        return true;
    }

    public bool IsProtectedFromSpike(Collider2D spikeCollider)
    {
        if (playerCollider == null || spikeCollider == null || bypassRailColliders == null) return false;

        Bounds playerBounds = playerCollider.bounds;
        Bounds spikeBounds = spikeCollider.bounds;

        foreach (Collider2D railCollider in bypassRailColliders)
        {
            if (railCollider == null) continue;

            Bounds railBounds = railCollider.bounds;
            bool playerIsAboveRail = playerBounds.min.y >= railBounds.center.y;
            bool feetAreOnRail = Mathf.Abs(playerBounds.min.y - railBounds.max.y) <= 0.25f;
            bool playerOverlapsRailX = playerBounds.max.x > railBounds.min.x && playerBounds.min.x < railBounds.max.x;
            bool railSitsAboveSpike = railBounds.max.y > spikeBounds.center.y;

            if (playerIsAboveRail && feetAreOnRail && playerOverlapsRailX && railSitsAboveSpike)
            {
                return true;
            }
        }

        return false;
    }

    private void TryTriggerOverlappingSpikeLoseState()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (playerCollider == null) return;

        Bounds playerBounds = playerCollider.bounds;
        Vector2 checkSize = GetSpikeFootCheckSize(playerBounds);
        Vector2 checkCenter = GetSpikeFootCheckCenter(playerBounds, checkSize);
        Collider2D[] hits = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == playerCollider) continue;
            if (hit.GetComponentInParent<Spike>() != null)
            {
                TryTriggerSpikeLoseState(hit);
                return;
            }
        }
    }

    private bool IsSpikeInFootSensor(Collider2D spikeCollider)
    {
        if (playerCollider == null || spikeCollider == null) return false;

        Bounds playerBounds = playerCollider.bounds;
        Vector2 checkSize = GetSpikeFootCheckSize(playerBounds);
        Vector2 checkCenter = GetSpikeFootCheckCenter(playerBounds, checkSize);
        Collider2D[] hits = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == playerCollider) continue;
            if (hit == spikeCollider || hit.GetComponentInParent<Spike>() == spikeCollider.GetComponentInParent<Spike>())
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 GetSpikeFootCheckSize(Bounds playerBounds)
    {
        return new Vector2(playerBounds.size.x * spikeFootCheckWidthScale, spikeFootCheckHeight);
    }

    private Vector2 GetSpikeFootCheckCenter(Bounds playerBounds, Vector2 checkSize)
    {
        return new Vector2(playerBounds.center.x, playerBounds.min.y + checkSize.y * 0.5f);
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
