using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // remove this line if you're not using TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState { WaitingToStart, Playing, GameOver, LevelComplete }
    public GameState CurrentState { get; private set; } = GameState.WaitingToStart;

    [Header("Speed Settings")]
    public float startSpeed = 5f;
    public float maxSpeed = 14f;
    public float speedRampRate = 0.15f; // how much speed increases per second while playing
    public float CurrentSpeed { get; private set; }

    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;
    public float invincibleTime = 1f; // brief invincibility window after taking damage
    private bool canTakeDamage = true;

    [Header("UI References")]
    public GameObject startPromptUI;      // "Press Space to Start" text
    public GameObject gameOverPanelUI;    // Panel with Restart / Quit buttons
    public GameObject levelCompletePanelUI; // Panel shown when the player reaches the checkpoint
    public HeartUI heartUI;               // Heart-icon health display (merged from PlayerHealth.cs)

    private void Awake()
    {
        // Simple singleton so other scripts can call GameManager.Instance
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        CurrentSpeed = startSpeed;
        if (heartUI != null) heartUI.UpdateHearts(currentHealth);

        if (startPromptUI != null) startPromptUI.SetActive(true);
        if (gameOverPanelUI != null) gameOverPanelUI.SetActive(false);
        if (levelCompletePanelUI != null) levelCompletePanelUI.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case GameState.WaitingToStart:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartGame();
                }
                break;

            case GameState.Playing:
                // Gradually ramp speed up over time, capped at maxSpeed.
                // This avoids needing a separate "timer" object - it just uses Time.deltaTime.
                CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + speedRampRate * Time.deltaTime);
                break;

            case GameState.GameOver:
                // No per-frame logic needed; waiting on button input from the Game Over panel.
                break;
        }
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        if (startPromptUI != null) startPromptUI.SetActive(false);
    }

    public void TakeDamage()
    {
        if (CurrentState != GameState.Playing) return;
        if (!canTakeDamage || currentHealth <= 0) return;

        canTakeDamage = false;
        currentHealth--;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (heartUI != null) heartUI.UpdateHearts(currentHealth);

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            StartCoroutine(DamageCooldown());
        }
    }

    private IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(invincibleTime);
        canTakeDamage = true;
    }

    public void TriggerGameOver()
    {
        CurrentState = GameState.GameOver;
        if (gameOverPanelUI != null) gameOverPanelUI.SetActive(true);
        // Optional: freeze time so nothing keeps moving behind the menu
        Time.timeScale = 0f;
    }

    public void TriggerLevelComplete()
    {
        // Movement stops automatically: PlayerController only applies horizontal speed
        // while CurrentState == Playing, so switching state here halts the player.
        CurrentState = GameState.LevelComplete;
        if (levelCompletePanelUI != null) levelCompletePanelUI.SetActive(true);
        // Freeze time so the player, camera, and any moving hazards all stop in place
        Time.timeScale = 0f;
    }

    // --- Hook these up to your Game Over panel's buttons ---

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Called by SpeedPortal.cs to instantly jump speed to a new value (like GD's speed portals).
    // The gradual ramp keeps running afterward, continuing to build from this new baseline.
    public void SetSpeed(float newSpeed)
    {
        CurrentSpeed = newSpeed;

        // If the portal speed exceeds the current ramp ceiling, raise the ceiling too -
        // otherwise the next frame's ramp calculation (Mathf.Min(maxSpeed, ...)) would
        // instantly clamp the speed back down right after the portal.
        if (newSpeed > maxSpeed)
        {
            maxSpeed = newSpeed;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        // Note: Application.Quit() does nothing in the Editor.
        
    }
}