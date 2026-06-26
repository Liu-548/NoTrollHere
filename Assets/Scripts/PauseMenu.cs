using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

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

    // Nút Pause/Resume ngoài HUD — tự tìm theo tên nếu Inspector chưa gán
    private Button nutPauseHUD;

    // === KEYBOARD NAVIGATION ===
    private Button[] cacNutPauseCache;
    private int nutPauseDangChon = -1;
    private readonly MenuKeyHold holdLen   = new MenuKeyHold(UnityEngine.InputSystem.Key.W, UnityEngine.InputSystem.Key.UpArrow);
    private readonly MenuKeyHold holdXuong = new MenuKeyHold(UnityEngine.InputSystem.Key.S, UnityEngine.InputSystem.Key.DownArrow);

    // === PLAYER FREEZE ===
    private PlayerController cachedPlayer;

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

        // Đồng bộ màu nút Main Menu = Restart Level (nút cuối = nút giữa)
        SyncMauNutPause();

        // Tìm Btn_Pause trong scene và wire onClick → NutTogglePause
        var goP = GameObject.Find("Btn_Pause");
        if (goP != null)
        {
            nutPauseHUD = goP.GetComponent<Button>();
            if (nutPauseHUD != null)
            {
                // Thay toàn bộ event (xóa cả persistent listener trong Inspector)
                nutPauseHUD.onClick = new Button.ButtonClickedEvent();
                nutPauseHUD.onClick.AddListener(NutTogglePause);
            }
        }
    }

    void Update()
    {
        if (GameInput.instance != null && GameInput.instance.PausePressed)
        {
            NutTogglePause();
            return;
        }

        // W/S điều hướng nút khi pause đang mở (không trong settings)
        if (!dangPause || dangSettings) return;

        float dt     = Time.unscaledDeltaTime;   // timeScale = 0 khi pause
        bool diLen   = holdLen.Update(dt);
        bool diXuong = holdXuong.Update(dt);
        bool ok      = GameInput.instance != null && GameInput.instance.ConfirmDown;

        if (diLen || diXuong)
        {
            if (cacNutPauseCache == null || cacNutPauseCache.Length == 0) return;
            int soNut = cacNutPauseCache.Length;
            if (nutPauseDangChon < 0)
                nutPauseDangChon = diXuong ? 0 : soNut - 1;
            else
                nutPauseDangChon = diXuong
                    ? (nutPauseDangChon + 1) % soNut
                    : (nutPauseDangChon - 1 + soNut) % soNut;
            var btn = cacNutPauseCache[nutPauseDangChon];
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(btn.gameObject);
            MenuSelectionFrame.ChonNut(btn.GetComponent<RectTransform>(), tronGoc: true);
        }

        if (ok && nutPauseDangChon >= 0
            && cacNutPauseCache != null && nutPauseDangChon < cacNutPauseCache.Length)
            cacNutPauseCache[nutPauseDangChon].onClick.Invoke();
    }

    /// <summary>Đồng bộ màu nền và màu chữ của nút Main Menu = nút Restart Level.</summary>
    void SyncMauNutPause()
    {
        var buttons = LayNutTrongPause();
        // buttons[0]=Resume, [1]=Restart Level, [2]=Main Menu
        if (buttons == null || buttons.Length < 3) return;

        var imgRestart  = buttons[1].GetComponent<Image>();
        var imgMainMenu = buttons[2].GetComponent<Image>();
        if (imgRestart != null && imgMainMenu != null)
            imgMainMenu.color = imgRestart.color;

        var txtRestart  = buttons[1].GetComponentInChildren<TMPro.TextMeshProUGUI>();
        var txtMainMenu = buttons[2].GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (txtRestart != null && txtMainMenu != null)
            txtMainMenu.color = txtRestart.color;
    }

    // Lấy các nút trong panelPause theo thứ tự từ trên xuống
    Button[] LayNutTrongPause()
    {
        if (panelPause == null) return null;
        var buttons = panelPause.GetComponentsInChildren<Button>(false);
        System.Array.Sort(buttons, (a, b) =>
        {
            float yA = a.GetComponent<RectTransform>()?.anchoredPosition.y ?? 0f;
            float yB = b.GetComponent<RectTransform>()?.anchoredPosition.y ?? 0f;
            return yB.CompareTo(yA); // Y cao hơn = ở trên = ưu tiên trước
        });
        return buttons;
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

    /// <summary>Toggle: mở nếu đang chơi, đóng nếu đang pause. Wire Btn_Pause vào đây.</summary>
    public void NutTogglePause()
    {
        if (dangSettings) NutDongSettings();
        else if (dangPause) NutResume();
        else NutPause();
    }

    public void NutPause()
    {
        dangPause = true;
        Time.timeScale = 0f;
        panelPause.SetActive(true);

        // Đóng băng toàn bộ input nhân vật (timeScale=0 chặn physics nhưng Update vẫn chạy)
        if (cachedPlayer == null) cachedPlayer = FindFirstObjectByType<PlayerController>();
        if (cachedPlayer != null) cachedPlayer.enabled = false;

        if (txtDeathInfo != null)
        {
            int soLanChet = GameManager.LaySoLanChet();
            txtDeathInfo.text = "you've died <color=#F5C842>" +
                soLanChet + "</color> times on this level";
        }

        // Đưa Btn_Pause lên trên panelPause để vẫn click được khi panel đang mở
        if (nutPauseHUD != null)
            nutPauseHUD.transform.SetAsLastSibling();

        nutPauseDangChon = 0;
        MenuSelectionFrame.An(); // ẩn khung cũ ngay, tránh flash sai vị trí

        // Đợi 1 frame để Unity tính xong layout → vị trí/kích thước nút mới chính xác
        StartCoroutine(ChonNutPauseSauMotFrame());
    }

    IEnumerator ChonNutPauseSauMotFrame()
    {
        yield return null; // 1 frame = layout xong, dù timeScale = 0 vẫn chạy

        cacNutPauseCache = LayNutTrongPause();
        nutPauseDangChon = 0;

        if (cacNutPauseCache != null && cacNutPauseCache.Length > 0)
        {
            var firstBtn = cacNutPauseCache[0];
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstBtn.gameObject);
            MenuSelectionFrame.ChonNut(firstBtn.GetComponent<RectTransform>(), tronGoc: true);
        }
    }

    public void NutResume()
    {
        dangPause = false;
        Time.timeScale = 1f;
        panelPause.SetActive(false);
        MenuSelectionFrame.An();
        nutPauseDangChon = -1;

        // Trả lại quyền điều khiển cho nhân vật
        if (cachedPlayer != null) cachedPlayer.enabled = true;
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