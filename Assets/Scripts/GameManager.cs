using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

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

    [Header("UI References")]
    public GameObject startPromptUI;      // "Press Space to Start" text
    public GameObject gameOverPanelUI;   
    public GameObject levelCompletePanelUI;
    public Slider healthBarSlider;        
    

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
        UpdateHealthUI();

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
                // Gradually ramp speed up over time, capped at maxSpeed
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

    public void TakeDamage(int amount)
    {
        if (CurrentState != GameState.Playing) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            TriggerGameOver();
        }
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

    private void UpdateHealthUI()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
        // if (healthText != null) healthText.text = $"Health: {currentHealth}/{maxHealth}";
    }


    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
       
    }
}
