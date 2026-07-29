using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameState
    {
        WaitingToStart,
        Playing,
        GameOver,
        LevelComplete
    }

    public GameState CurrentState { get; private set; } = GameState.WaitingToStart;

    [Header("Speed Settings")]
    public float startSpeed = 5f;
    public float maxSpeed = 14f;
    public float speedRampRate = 0.15f;
    public float CurrentSpeed { get; private set; }

    [Header("UI References")]
    public GameObject startPromptUI;
    public GameObject gameOverPanelUI;
    public GameObject levelCompletePanelUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        CurrentSpeed = startSpeed;

        if (startPromptUI != null)
            startPromptUI.SetActive(true);

        if (gameOverPanelUI != null)
            gameOverPanelUI.SetActive(false);

        if (levelCompletePanelUI != null)
            levelCompletePanelUI.SetActive(false);

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

                CurrentSpeed = Mathf.Min(
                    maxSpeed,
                    CurrentSpeed + speedRampRate * Time.deltaTime);

                break;
        }
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;

        if (startPromptUI != null)
            startPromptUI.SetActive(false);
    }

    // Called when the player has lost all hearts
    public void GameOver()
    {
        CurrentState = GameState.GameOver;

        if (gameOverPanelUI != null)
            gameOverPanelUI.SetActive(true);

        Time.timeScale = 0f;
    }

    public void TriggerLevelComplete()
    {
        CurrentState = GameState.LevelComplete;

        if (levelCompletePanelUI != null)
            levelCompletePanelUI.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartGame()
    {
        Retry();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}