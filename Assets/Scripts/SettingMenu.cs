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
        TimPanelNeuChua();
        TuTimUITrongPanel();
        GanListener();

        if (panelSettings != null)
            panelSettings.SetActive(false);
        DocSettings();
    }

    void TuTimUITrongPanel()
    {
        if (panelSettings == null) return;

        // Tìm tất cả Slider trong panel, gán theo thứ tự nếu chưa gán
        Slider[] sliders = panelSettings.GetComponentsInChildren<Slider>(true);
        if (sliderSFX == null && sliders.Length > 0) sliderSFX = sliders[0];
        if (sliderMusic == null && sliders.Length > 1) sliderMusic = sliders[1];

        // Tìm Toggle trong panel
        if (toggleFullscreen == null)
            toggleFullscreen = panelSettings.GetComponentInChildren<Toggle>(true);
    }

    void GanListener()
    {
        if (sliderSFX != null)
        {
            sliderSFX.minValue = 0f;
            sliderSFX.onValueChanged.RemoveAllListeners();
            sliderSFX.onValueChanged.AddListener(DoiVolumeSFX);
        }
        if (sliderMusic != null)
        {
            sliderMusic.minValue = 0f;
            sliderMusic.onValueChanged.RemoveAllListeners();
            sliderMusic.onValueChanged.AddListener(DoiVolumeMusic);
        }
        if (toggleFullscreen != null)
        {
            toggleFullscreen.onValueChanged.RemoveAllListeners();
            toggleFullscreen.onValueChanged.AddListener(DoiFullscreen);
        }
    }

    void DocSettings()
    {
        float sfx   = PlayerPrefs.GetFloat("Vol_SFX",   1f);
        float music = PlayerPrefs.GetFloat("Vol_Music", 0.5f);
        bool full   = PlayerPrefs.GetInt("Fullscreen", 0) == 1;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.volumeSFX   = sfx;
            SoundManager.instance.volumeMusic  = music;
            SoundManager.instance.CapNhatVolumeMusic();
        }

        ApplyFullscreen(full);

        if (sliderSFX   != null) sliderSFX.value   = sfx;
        if (sliderMusic != null) sliderMusic.value  = music;
        if (toggleFullscreen != null) toggleFullscreen.isOn = full;
    }

    // === NÚT SETTINGS ===
    public void NutMoSettings()
    {
        TimPanelNeuChua();
        if (panelSettings != null)
            panelSettings.SetActive(true);
        else
            Debug.LogWarning("[SettingsMenu] panelSettings chưa gán!");
    }

    public void NutDongSettings()
    {
        if (panelSettings != null)
            panelSettings.SetActive(false);
    }

    void TimPanelNeuChua()
    {
        if (panelSettings != null) return;
        Transform found = transform.Find("Panel_Settings");
        if (found == null)
        {
            var go = GameObject.Find("Panel_Settings");
            if (go != null) panelSettings = go;
        }
        else panelSettings = found.gameObject;
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
        ApplyFullscreen(value);
        PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
        PlayerPrefs.Save();
    }

    // === XOÁ SAVE (dùng khi test, có thể ẩn nút này trong build thật) ===
    public void NutXoaSave()
    {
        if (GameManager.instance != null)
            GameManager.instance.ResetProgress();
        else
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("Level_S_0_unlocked", 1);
            PlayerPrefs.SetString("LatestLevel", "Level_S_0");
            PlayerPrefs.Save();
        }
        Debug.Log("[Settings] Đã xoá toàn bộ save!");
    }

    // Bật/tắt fullscreen đúng cách trên Windows build
    static void ApplyFullscreen(bool value)
    {
        if (value)
            Screen.SetResolution(Screen.currentResolution.width,
                                 Screen.currentResolution.height,
                                 FullScreenMode.FullScreenWindow);
        else
            Screen.fullScreenMode = FullScreenMode.Windowed;
    }
}
