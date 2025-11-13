using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip mainMenuMusic;

    private const string KEY_MUSIC_VOLUME = "sm_music_volume";
    private const string KEY_SFX_VOLUME = "sm_sfx_volume";

    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved volumes
        musicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 1f);
        sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);

        if (musicSource != null)
            musicSource.volume = musicVolume;

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    private void Start()
    {
        // Play main menu music on start
        if (musicSource != null && mainMenuMusic != null)
        {
            musicSource.clip = mainMenuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // -------- Volume control --------

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        if (musicSource != null)
            musicSource.volume = musicVolume;

        PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, sfxVolume);
        PlayerPrefs.Save();
    }

    // -------- SFX helper (for later) --------

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
    }
}
