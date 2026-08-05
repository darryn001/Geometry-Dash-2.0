using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    private const string StartSceneName = "StartScreen";

    [Header("UI References")]
    public GameObject pausePanel;
    public GameObject soundPanel;

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool allowPKey = true;

    public bool IsPaused { get; private set; }

    private void Start()
    {
        SetPanelActive(pausePanel, false);
        SetPanelActive(soundPanel, false);
    }

    private void Update()
    {
        if (!CanPause()) return;

        bool pressedPause = Input.GetKeyDown(pauseKey) || (allowPKey && Input.GetKeyDown(KeyCode.P));

        if (pressedPause)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (!CanPause()) return;

        IsPaused = true;
        Time.timeScale = 0f;
        SetPanelActive(pausePanel, true);
        SetPanelActive(soundPanel, false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPauseSound();
        }
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SetPanelActive(pausePanel, false);
        SetPanelActive(soundPanel, false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
            return;
        }

        SceneManager.LoadScene(StartSceneName);
    }

    public void OpenSoundPanel()
    {
        SetPanelActive(soundPanel, true);
    }

    public void CloseSoundPanel()
    {
        SetPanelActive(soundPanel, false);
    }

    private bool CanPause()
    {
        return GameManager.Instance != null
            && GameManager.Instance.CurrentState != GameManager.GameState.GameOver
            && GameManager.Instance.CurrentState != GameManager.GameState.LevelComplete;
    }

    private void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }
}
