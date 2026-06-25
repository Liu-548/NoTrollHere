using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("=== THAM CHIẾU ===")]
    public GameObject panelPause;
    public GameObject panelSettings;    // Panel Settings lồng trong Pause
    public TextMeshProUGUI txtDeathInfo;

    [Header("=== SETTINGS UI ===")]
    public Slider sliderSFX;
    public Slider sliderMusic;
    public Toggle toggleFullscreen;

    [Header("=== CÀI ĐẶT ===")]
    public string tenSceneMainMenu = "MainMenu";

    private int soLanChetManNay = 0;
    private bool dangPause = false;
    private bool dangSettings = false;

    public static PauseMenu instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (panelPause != null)
            panelPause.SetActive(false);
        if (panelSettings != null)
            panelSettings.SetActive(false);

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

        // Load settings đã lưu
        DocSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (dangSettings) NutDongSettings();
            else if (dangPause) NutResume();
            else NutPause();
        }
    }

    // =============================================================
    // LOAD / LƯU SETTINGS
    // =============================================================
    void DocSettings()
    {
        float sfx = PlayerPrefs.GetFloat("Vol_SFX", 1f);
        float music = PlayerPrefs.GetFloat("Vol_Music", 0.5f);
        bool full = PlayerPrefs.GetInt("Fullscreen",
            Screen.fullScreen ? 1 : 0) == 1;

        // Áp vào SoundManager
        if (SoundManager.instance != null)
        {
            SoundManager.instance.volumeSFX = sfx;
            SoundManager.instance.volumeMusic = music;
        }

        Screen.fullScreen = full;

        // Áp vào UI
        if (sliderSFX != null) sliderSFX.value = sfx;
        if (sliderMusic != null) sliderMusic.value = music;
        if (toggleFullscreen != null)
            toggleFullscreen.isOn = full;
    }

    // =============================================================
    // GỌI TỪ SLIDER / TOGGLE (gắn OnValueChanged trong Inspector)
    // =============================================================
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
            // Áp ngay vào nhạc đang phát
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

    // =============================================================
    // PAUSE
    // =============================================================
    public void ThemMotLanChet() => soLanChetManNay++;

    public void NutPause()
    {
        dangPause = true;
        Time.timeScale = 0f;
        panelPause.SetActive(true);

        if (txtDeathInfo != null)
            txtDeathInfo.text = "you've died <color=#F5C842>" +
                soLanChetManNay + "</color> times on this level";
    }

    public void NutResume()
    {
        dangPause = false;
        Time.timeScale = 1f;
        panelPause.SetActive(false);
    }

    public void NutRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NutMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneMainMenu);
    }

    // =============================================================
    // SETTINGS — mở từ Pause Menu
    // =============================================================
    public void NutMoSettings()
    {
        if (panelSettings != null)
        {
            panelSettings.SetActive(true);
            dangSettings = true;
        }
    }

    public void NutDongSettings()
    {
        if (panelSettings != null)
        {
            panelSettings.SetActive(false);
            dangSettings = false;
        }
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}