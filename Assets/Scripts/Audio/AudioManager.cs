using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private const string DefaultMenuSceneName = "MenuScene";
    private const string DefaultGameplaySceneName = "GamePlayScene";

    public static AudioManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string menuSceneName = DefaultMenuSceneName;
    [SerializeField] private string gameplaySceneName = DefaultGameplaySceneName;

    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private bool loopMusic = true;

    [Header("SFX")]
    [SerializeField] private AudioClip gameOverSfx;
    [SerializeField] private AudioClip playerMoveSfx;
    [SerializeField] private AudioClip playerHurtSfx;
    [SerializeField] private AudioClip hitEnemySfx;
    [SerializeField] private AudioClip expPickupSfx;
    [SerializeField] private AudioClip levelUpSfx;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float moveSfxVolume = 5.0f;

    [Header("Sources (Optional)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSfxSource;

    private float lastExpPickupTime = -1f;
    private const float ExpPickupCooldown = 0.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        HandleSceneMusic(SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (loopSfxSource == null)
        {
            loopSfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = loopMusic;
        musicSource.volume = Mathf.Clamp01(musicVolume);
        musicSource.priority = 0; // Highest priority, prevent engine from culling BGM

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = Mathf.Clamp01(sfxVolume);
        sfxSource.priority = 128; // Default, lower than BGM

        loopSfxSource.playOnAwake = false;
        loopSfxSource.loop = true;
        loopSfxSource.volume = Mathf.Clamp01(moveSfxVolume);
        loopSfxSource.priority = 128;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneMusic(scene.name);
    }

    private void HandleSceneMusic(string sceneName)
    {
        if (string.Equals(sceneName, menuSceneName))
        {
            PlayMusic(menuMusic);
            return;
        }

        if (string.Equals(sceneName, gameplaySceneName))
        {
            PlayMusic(gameplayMusic);
            return;
        }

        StopMusic();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
        {
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.loop = loopMusic;
        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(musicVolume);
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.Stop();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.volume = Mathf.Clamp01(sfxVolume);
        sfxSource.PlayOneShot(clip);

    }

    public void PlaySfxWithVolume(AudioClip clip, float volume)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }
        sfxSource.volume = Mathf.Clamp01(volume);
        sfxSource.PlayOneShot(clip);
    }

    public void PlayGameOverSfx()
    {
        PlaySfxWithVolume(gameOverSfx, Mathf.Min(1f, sfxVolume * 2f));
    }

    public void PlayPlayerHurtSfx()
    {
        PlaySfx(playerHurtSfx);
    }

    public void PlayHitEnemySfx()
    {
        PlaySfx(hitEnemySfx);
    }

    public void PlayExpPickupSfx()
    {
        if (Time.unscaledTime - lastExpPickupTime >= ExpPickupCooldown)
        {
            PlaySfx(expPickupSfx);
            lastExpPickupTime = Time.unscaledTime;
        }
    }

    public void PlayLevelUpSfx()
    {
        PlaySfx(levelUpSfx);
    }

    public void SetPlayerMoveSfxActive(bool active)
    {
        if (loopSfxSource == null)
        {
            return;
        }

        if (!active)
        {
            if (loopSfxSource.isPlaying)
            {
                loopSfxSource.Stop();
            }

            return;
        }

        if (playerMoveSfx == null)
        {
            return;
        }

        if (loopSfxSource.clip != playerMoveSfx)
        {
            loopSfxSource.clip = playerMoveSfx;
        }

        loopSfxSource.volume = Mathf.Clamp01(moveSfxVolume);

        if (!loopSfxSource.isPlaying)
        {
            loopSfxSource.Play();
        }
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void SetMoveSfxVolume(float value)
    {
        moveSfxVolume = Mathf.Clamp01(value);

        if (loopSfxSource != null)
        {
            loopSfxSource.volume = moveSfxVolume;
        }
    }

    public void MuteAll(bool muted)
    {
        AudioListener.pause = muted;
    }
}
