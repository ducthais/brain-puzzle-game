using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("--- TRẠM PHÁT ÂM THANH ---")]
    public AudioSource musicSource; // Loa phát nhạc nền (Phát liên tục)
    public AudioSource sfxSource;   // Loa phát hiệu ứng (Phát 1 lần rồi tắt)

    [Header("--- KHO CHỨA ÂM THANH ---")]
    public AudioClip backgroundMusic;
    public AudioClip winSound;
    public AudioClip jumpSound;    // MỚI: Tiếng nhảy
    public AudioClip hurtSound;    // MỚI: Tiếng đau/chết
    public AudioClip clickSound;   // MỚI: Tiếng bấm nút

    // Quản lý riêng biệt 2 trạng thái
    [HideInInspector] public bool isMusicMuted = false;
    [HideInInspector] public bool isSfxMuted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sống sót qua mọi màn
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        isMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        isSfxMuted = PlayerPrefs.GetInt("SfxMuted", 0) == 1;

        ApplyAudioSettings();

        // Tự động phát nhạc nền khi vào game
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true; // Lặp lại nhạc nền
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && !isSfxMuted)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // MỚI: Hàm chuyên dụng để gắn vào các Nút bấm trên giao diện
    public void PlayClickSound()
    {
        PlaySFX(clickSound);
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        PlayerPrefs.SetInt("MusicMuted", isMusicMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyAudioSettings();
    }

    public void ToggleSFX()
    {
        isSfxMuted = !isSfxMuted;
        PlayerPrefs.SetInt("SfxMuted", isSfxMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        if (musicSource != null) musicSource.mute = isMusicMuted;
        if (sfxSource != null) sfxSource.mute = isSfxMuted;
    }
}