using UnityEngine;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private SoundCatalogSO soundCatalog;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Limits")]
    [Range(0f, 1f)][SerializeField] private float musicMaxCap = 0.4f;
    [Range(0f, 1f)][SerializeField] private float sfxMaxCap = 0.6f;  

    [Header("Settings")]
    public float musicFadeDuration = 1f;

    private SoundType _currentMusic = (SoundType)(-1);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(SoundType type)
    {
        if (_currentMusic == type && musicSource.isPlaying) return;

        SoundRecord record = soundCatalog.GetSound(type);
        if (record == null || record.clip == null)
        {
            Debug.LogWarning($"[SoundManager] Music not found: {type}");
            return;
        }

        _currentMusic = type;

        if (musicSource.isPlaying)
        {
            musicSource.DOFade(0f, musicFadeDuration).OnComplete(() =>
            {
                musicSource.clip = record.clip;
                musicSource.volume = 0f;
                musicSource.pitch = 1f;
                musicSource.Play();
                musicSource.DOFade(record.volume, musicFadeDuration);
            }).SetLink(gameObject);
        }
        else
        {
            musicSource.clip = record.clip;
            musicSource.volume = 0f;
            musicSource.pitch = 1f;
            musicSource.Play();
            musicSource.DOFade(record.volume, musicFadeDuration).SetLink(gameObject);
        }
    }

    public void PlaySFX(SoundType type)
    {
        SoundRecord record = soundCatalog.GetSound(type);
        if (record == null || record.clip == null)
        {
            Debug.LogWarning($"[SoundManager] SFX not found: {type}");
            return;
        }

        if (record.useRandomPitch)
        {
            sfxSource.pitch = Random.Range(record.minPitch, record.maxPitch);
        }
        else
        {
            sfxSource.pitch = 1f;
        }

        float sfxSliderValue = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float finalVolume = record.volume * sfxSliderValue * sfxMaxCap;

        sfxSource.PlayOneShot(record.clip, record.volume);
    }

    public void PlaySFX(SoundType type, float delay)
    {
        if (delay <= 0f)
        {
            PlaySFX(type);
            return;
        }

        DOVirtual.DelayedCall(delay, () =>
        {
            PlaySFX(type);
        }).SetLink(gameObject);
    }

    public void SetMusicVolume(float volume)
    {
        float finalVolume = volume * musicMaxCap;
        musicSource.volume = finalVolume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        sfxSource.volume = volume * sfxMaxCap;
    }
    public void MuteAll(bool isMuted)
    {
        musicSource.mute = isMuted;
        sfxSource.mute = isMuted;
    }
}