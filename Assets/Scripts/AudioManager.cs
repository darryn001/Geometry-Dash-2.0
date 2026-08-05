using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource[] ambienceSources;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip[] ambienceClips;
    public AudioClip characterSfx;
    public AudioClip gameOverSfx;
    public AudioClip winSfx;
    public AudioClip buttonClickSfx;

    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume = 0.45f;
    [Range(0f, 1f)] public float ambienceVolume = 0.12f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private bool musicEnabled = true;
    private bool sfxEnabled = true;

    public bool IsMuted => !musicEnabled && !sfxEnabled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void BootstrapAudioManager()
    {
        if (Instance != null) return;

        GameObject audioManagerObject = new GameObject("AudioManager");
        audioManagerObject.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadDefaultClips();
        EnsureAudioSources();
    }

    private void Start()
    {
        ApplyVolumes();
        PlayBackgroundMusic();
    }

    private void OnValidate()
    {
        ApplyVolumes();
    }

    public void PlayBackgroundMusic()
    {
        if (!musicEnabled) return;

        PlayLoop(musicSource, backgroundMusic, musicVolume);

        for (int i = 0; i < ambienceSources.Length; i++)
        {
            AudioClip clip = i < ambienceClips.Length ? ambienceClips[i] : null;
            PlayLoop(ambienceSources[i], clip, ambienceVolume);
        }
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;

        if (musicEnabled)
        {
            PlayBackgroundMusic();
        }
        else
        {
            PauseLoopingSources();
        }

        ApplyVolumes();
    }

    public void SetSfxEnabled(bool enabled)
    {
        sfxEnabled = enabled;
        ApplyVolumes();
    }

    public void ToggleMute()
    {
        SetMuted(!IsMuted);
    }

    public void SetMuted(bool muted)
    {
        musicEnabled = !muted;
        sfxEnabled = !muted;

        if (musicEnabled)
        {
            PlayBackgroundMusic();
        }
        else
        {
            PauseLoopingSources();
        }

        ApplyVolumes();
    }

    public void PlayJumpSound()
    {
        PlayCharacterSound();
    }

    public void PlayPauseSound()
    {
        PlayCharacterSound();
    }

    public void PlayHitSound()
    {
        PlayCharacterSound();
    }

    public void PlayGameOverSound()
    {
        PlaySfx(gameOverSfx);
    }

    public void PlayWinSound()
    {
        PlaySfx(winSfx);
    }

    private void PlayCharacterSound()
    {
        PlaySfx(characterSfx);
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickSfx);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (!sfxEnabled || sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private void PlayLoop(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null) return;

        if (source.clip != clip)
        {
            source.clip = clip;
        }

        source.loop = true;
        source.volume = volume;

        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        int ambienceCount = ambienceClips != null && ambienceClips.Length > 0 ? ambienceClips.Length : 4;
        if (ambienceSources == null || ambienceSources.Length != ambienceCount)
        {
            ambienceSources = new AudioSource[ambienceCount];
        }

        for (int i = 0; i < ambienceSources.Length; i++)
        {
            if (ambienceSources[i] == null)
            {
                ambienceSources[i] = gameObject.AddComponent<AudioSource>();
            }
        }

        musicSource.playOnAwake = false;
        sfxSource.playOnAwake = false;

        for (int i = 0; i < ambienceSources.Length; i++)
        {
            ambienceSources[i].playOnAwake = false;
        }
    }

    private void PauseLoopingSources()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }

        if (ambienceSources == null) return;

        for (int i = 0; i < ambienceSources.Length; i++)
        {
            if (ambienceSources[i] != null)
            {
                ambienceSources[i].Pause();
            }
        }
    }

    private void LoadDefaultClips()
    {
        if (backgroundMusic == null)
        {
            backgroundMusic = Resources.Load<AudioClip>("Audio/alien_swamp");
        }

        if (ambienceClips == null || ambienceClips.Length == 0)
        {
            ambienceClips = new[]
            {
                Resources.Load<AudioClip>("Audio/amb_bird_1"),
                Resources.Load<AudioClip>("Audio/amb_bird_2"),
                Resources.Load<AudioClip>("Audio/amb_cricket_1"),
                Resources.Load<AudioClip>("Audio/amb_frog_1")
            };
        }

        if (characterSfx == null)
        {
            characterSfx = Resources.Load<AudioClip>("Audio/mutant_frog_2");
        }

        if (gameOverSfx == null)
        {
            gameOverSfx = Resources.Load<AudioClip>("Audio/game_over");
        }

        if (winSfx == null)
        {
            winSfx = Resources.Load<AudioClip>("Audio/jingle_win_00");
        }
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicEnabled ? musicVolume : 0f;
        }

        if (ambienceSources != null)
        {
            for (int i = 0; i < ambienceSources.Length; i++)
            {
                if (ambienceSources[i] != null)
                {
                    ambienceSources[i].volume = musicEnabled ? ambienceVolume : 0f;
                }
            }
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxEnabled ? sfxVolume : 0f;
        }
    }
}
