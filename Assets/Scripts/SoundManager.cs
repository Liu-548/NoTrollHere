using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("=== VOLUME ===")]
    [Range(0f, 1f)] public float volumeSFX = 1f;
    [Range(0f, 1f)] public float volumeMusic = 0.5f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip clipNhay;
    private AudioClip clipChay;
    private AudioClip clipChet;
    private AudioClip clipThang;
    private AudioClip clipTeleport;
    private AudioClip clipNhacMenuCh1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            KhoiTaoAudioSources();
            TaoTatCaClip();
        }
        else Destroy(gameObject);
    }

    void KhoiTaoAudioSources()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = volumeMusic;
    }

    void TaoTatCaClip()
    {
        clipNhay = TaoClipNhay();
        clipChay = TaoClipChay();
        clipChet = TaoClipChet();
        clipThang = TaoClipThang();
        clipTeleport = TaoClipTeleport();
        clipNhacMenuCh1 = TaoNhacMenuCh1();
    }

    // =============================================================
    // PLAY SFX
    // =============================================================
    public void PlayNhay() => sfxSource.PlayOneShot(clipNhay, volumeSFX);
    public void PlayChay() => sfxSource.PlayOneShot(clipChay, volumeSFX * 0.4f);
    public void PlayChet() => sfxSource.PlayOneShot(clipChet, volumeSFX);
    public void PlayThang() => sfxSource.PlayOneShot(clipThang, volumeSFX);
    public void PlayTeleport() => sfxSource.PlayOneShot(clipTeleport, volumeSFX * 0.3f);

    // =============================================================
    // PLAY MUSIC
    // =============================================================
    public void PlayNhacMenuCh1()
    {
        if (musicSource.clip == clipNhacMenuCh1 && musicSource.isPlaying) return;
        musicSource.clip = clipNhacMenuCh1;
        musicSource.volume = volumeMusic;
        musicSource.Play();
    }

    public void DungNhac() => musicSource.Stop();

    public void FadeOutNhac(float thoiGian = 1f)
    {
        StartCoroutine(FadeOut(thoiGian));
    }

    IEnumerator FadeOut(float thoiGian)
    {
        float startVol = musicSource.volume;
        float t = 0f;
        while (t < thoiGian)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / thoiGian);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = volumeMusic;
    }

    // =============================================================
    // SFX — NHẢY
    // =============================================================
    AudioClip TaoClipNhay()
    {
        int sampleRate = 44100;
        float duration = 0.15f;
        int samples = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Nhay", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;
            float freq = Mathf.Lerp(300f, 500f, progress);
            float envelope = Mathf.Exp(-progress * 8f);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    // =============================================================
    // SFX — CHẠY (footstep)
    // =============================================================
    AudioClip TaoClipChay()
    {
        int sampleRate = 44100;
        float duration = 0.05f;
        int samples = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Chay", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        System.Random rng = new System.Random(42);
        for (int i = 0; i < samples; i++)
        {
            float progress = (float)i / samples;
            float noise = (float)(rng.NextDouble() * 2 - 1);
            float envelope = Mathf.Exp(-progress * 20f);
            data[i] = noise * envelope * 0.25f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    // =============================================================
    // SFX — CHẾT
    // =============================================================
    AudioClip TaoClipChet()
    {
        int sampleRate = 44100;
        float duration = 0.4f;
        int samples = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Chet", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        System.Random rng = new System.Random(7);
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;
            float freq = Mathf.Lerp(350f, 80f, Mathf.Pow(progress, 0.3f));
            float envelope = Mathf.Exp(-progress * 5f);
            float noise = (float)(rng.NextDouble() * 2 - 1) * 0.15f;
            data[i] = (Mathf.Sin(2f * Mathf.PI * freq * t) + noise)
                      * envelope * 0.6f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    // =============================================================
    // SFX — THẮNG (mềm, dịu)
    // =============================================================
    AudioClip TaoClipThang()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int samples = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Thang", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        float[] freqs = { 261.63f, 329.63f, 392.00f };
        float[] starts = { 0f, 0.28f, 0.56f };

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float val = 0f;

            for (int n = 0; n < freqs.Length; n++)
            {
                float elapsed = t - starts[n];
                if (elapsed >= 0f && elapsed < 0.28f)
                {
                    float env = Mathf.Exp(-elapsed * 4f);
                    val += Mathf.Sin(2f * Mathf.PI * freqs[n] * elapsed) * env * 0.2f;
                }
            }

            // Soft clipping — tránh clipping cứng
            data[i] = Mathf.Clamp(val * 0.7f, -0.8f, 0.8f);
        }

        clip.SetData(data, 0);
        return clip;
    }

    // =============================================================
    // SFX — TELEPORT
    // =============================================================
    AudioClip TaoClipTeleport()
    {
        int sampleRate = 44100;
        float duration = 0.5f;
        int samples = (int)(sampleRate * duration);
        AudioClip clip = AudioClip.Create("Teleport", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;
            float freq = Mathf.Lerp(200f, 1200f, progress);
            float envelope = Mathf.Sin(Mathf.PI * progress);
            float tremolo = 1f + 0.2f * Mathf.Sin(2f * Mathf.PI * 20f * t);
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t)
                      * envelope * tremolo * 0.4f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    // =============================================================
    // NHẠC NỀN — MAIN MENU + CHAPTER 1 (dịu, mềm)
    // =============================================================
    AudioClip TaoNhacMenuCh1()
    {
        int sampleRate = 44100;
        float bpm = 70f;
        float beatDuration = 60f / bpm;
        float loopDuration = beatDuration * 8f;
        int samples = (int)(sampleRate * loopDuration);
        AudioClip clip = AudioClip.Create("NhacMenuCh1", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        float[] notes = { 130.81f, 146.83f, 155.56f, 196.00f, 207.65f };
        int[] pattern = { 0, 2, 4, 2, 1, 3, 2, 0 };

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float val = 0f;

            // Bass — rất nhỏ
            float beatProgress = (t % beatDuration) / beatDuration;
            float bassEnv = Mathf.Exp(-beatProgress * 6f);
            val += Mathf.Sin(2f * Mathf.PI * 65.41f * t) * bassEnv * 0.12f;

            // Melody — nhỏ, decay chậm
            int beatIndex = (int)(t / beatDuration) % pattern.Length;
            float noteFreq = notes[pattern[beatIndex]];
            float noteProgress = (t % beatDuration) / beatDuration;
            float noteEnv = Mathf.Exp(-noteProgress * 2.5f) * 0.35f;
            val += Mathf.Sin(2f * Mathf.PI * noteFreq * t) * noteEnv * 0.1f;

            // Pad — rất nhỏ
            val += Mathf.Sin(2f * Mathf.PI * 130.81f * t) * 0.02f;
            val += Mathf.Sin(2f * Mathf.PI * 155.56f * t) * 0.015f;
            val += Mathf.Sin(2f * Mathf.PI * 196.00f * t) * 0.015f;

            // Soft clipping
            data[i] = Mathf.Clamp(val * 0.8f, -0.75f, 0.75f);
        }

        clip.SetData(data, 0);
        return clip;
    }

    public void CapNhatVolumeMusic()
    {
        if (musicSource == null) return;
        musicSource.volume = volumeMusic;
        // Nếu nhạc đã có clip nhưng bị dừng (do volume=0 trước đó), play lại
        if (!musicSource.isPlaying && musicSource.clip != null)
            musicSource.Play();
    }
}