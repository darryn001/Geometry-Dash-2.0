using UnityEngine;

public class SoundPanelController : MonoBehaviour
{
    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    public void SetSfxVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicEnabled(enabled);
        }
    }

    public void SetSfxEnabled(bool enabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxEnabled(enabled);
        }
    }

    public void PlayButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
