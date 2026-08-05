using UnityEngine;
using UnityEngine.UI;

public class MuteToggleButton : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite volumeSprite;
    [SerializeField] private Sprite muteSprite;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        RefreshIcon();
    }

    public void ToggleMute()
    {
        if (AudioManager.Instance == null) return;

        bool wasMuted = AudioManager.Instance.IsMuted;
        if (!wasMuted)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        AudioManager.Instance.ToggleMute();
        RefreshIcon();
    }

    private void RefreshIcon()
    {
        if (targetImage == null || AudioManager.Instance == null) return;

        targetImage.sprite = AudioManager.Instance.IsMuted ? muteSprite : volumeSprite;
        targetImage.preserveAspect = true;
    }
}
