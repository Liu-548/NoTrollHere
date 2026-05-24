using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("=== PANEL ===")]
    public GameObject panelSettings;

    [Header("=== UI ===")]
    public Slider sliderSFX;
    public Slider sliderMusic;
    public Toggle toggleFullscreen;

    public static SettingsMenu instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (panelSettings != null)
            panelSettings.SetActive(false);
        DocSettings();
    }

    void DocSettings()
    {
        float sfx = PlayerPrefs.GetFloat("Vol_SFX", 1f);
        float music = PlayerPrefs.GetFloat("Vol_Music", 0.5f);
        bool full = PlayerPrefs.GetInt("Fullscreen",
            Screen.fullScreen ? 1 : 0) == 1;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.volumeSFX = sfx;
            SoundManager.instance.volumeMusic = music;
        }

        Screen.fullScreen = full;

        if (sliderSFX != null) sliderSFX.value = sfx;
        if (sliderMusic != null) sliderMusic.value = music;
        if (toggleFullscreen != null) toggleFullscreen.isOn = full;
    }

    // === NÚT SETTINGS (gắn vào Btn_Settings ở Main Menu) ===
    public void NutMoSettings()
    {
        if (panelSettings != null)
            panelSettings.SetActive(true);
    }

    public void NutDongSettings()
    {
        if (panelSettings != null)
            panelSettings.SetActive(false);
    }

    // === SLIDER / TOGGLE EVENTS ===
    public void DoiVolumeSFX(float value)
    {
        if (SoundManager.instance != null)
            SoundManager.instance.volumeSFX = value;
        PlayerPrefs.SetFloat("Vol_SFX", value);
        PlayerPrefs.Save();
    }

    public void DoiVolumeMusic(float value)
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.volumeMusic = value;
            SoundManager.instance.CapNhatVolumeMusic();
        }
        PlayerPrefs.SetFloat("Vol_Music", value);
        PlayerPrefs.Save();
    }

    public void DoiFullscreen(bool value)
    {
        Screen.fullScreen = value;
        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
        PlayerPrefs.Save();
    }
}