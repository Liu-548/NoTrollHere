using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public string tenSceneLevelSelect = "LevelSelect";

    [Header("=== ANIMATION ===")]
    public float thoiGianFadeIn = 0.8f;

    [Header("=== THAM CHIẾU UI ===")]
    public CanvasGroup canvasGroup;

    // === KEYBOARD NAVIGATION ===
    private Button[] cacNutMenu;
    private int nutDangChon = -1;
    private static readonly string[] TEN_NUT_MENU =
        { "Btn_Play", "Btn_LevelSelect", "Btn_Skins", "Btn_Achievements", "Btn_Settings" };
    private readonly MenuKeyHold holdLen   = new MenuKeyHold(UnityEngine.InputSystem.Key.W, UnityEngine.InputSystem.Key.UpArrow);
    private readonly MenuKeyHold holdXuong = new MenuKeyHold(UnityEngine.InputSystem.Key.S, UnityEngine.InputSystem.Key.DownArrow);

    void Start()
    {
        StartCoroutine(FadeIn());

        // Play nhạc menu
        if (SoundManager.instance != null)
            SoundManager.instance.PlayNhacMenuCh1();

        KhoiTaoNavBanPhim();
        StartCoroutine(ChonPlayMacDinh());
    }

    // Chờ 1 frame cho VerticalLayoutGroup tính toán xong vị trí nút
    IEnumerator ChonPlayMacDinh()
    {
        yield return null;
        if (cacNutMenu != null && cacNutMenu.Length > 0)
        {
            nutDangChon = 0;
            ChonNutMenu(0);
        }
    }

    void KhoiTaoNavBanPhim()
    {
        var ds = new List<Button>();
        foreach (string ten in TEN_NUT_MENU)
        {
            var go = GameObject.Find(ten);
            if (go != null)
            {
                var btn = go.GetComponent<Button>();
                if (btn != null) ds.Add(btn);
            }
        }
        cacNutMenu = ds.ToArray();

        // Đồng bộ màu chữ nút Settings với các nút khác (#E8E8D0)
        var goSettings = GameObject.Find("Btn_Settings");
        if (goSettings != null)
        {
            var txt = goSettings.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null) txt.color = new Color(0.91f, 0.91f, 0.82f);
        }
    }

    void Update()
    {
        SettingsMenu sm = SettingsMenu.instance;
        bool settingsMo = sm != null && sm.panelSettings != null && sm.panelSettings.activeSelf;

        if (GameInput.instance != null && GameInput.instance.EscapeDown)
        {
            if (settingsMo) sm.NutDongSettings();
            nutDangChon = -1;
            MenuSelectionFrame.An();
            return;
        }

        if (settingsMo || cacNutMenu == null || cacNutMenu.Length == 0) return;

        float dt     = Time.unscaledDeltaTime;
        bool diLen   = holdLen.Update(dt);
        bool diXuong = holdXuong.Update(dt);
        bool ok      = GameInput.instance != null && GameInput.instance.ConfirmDown;

        if (diLen || diXuong)
        {
            if (nutDangChon < 0)
                nutDangChon = diXuong ? 0 : cacNutMenu.Length - 1;
            else
                nutDangChon = diXuong
                    ? (nutDangChon + 1) % cacNutMenu.Length
                    : (nutDangChon - 1 + cacNutMenu.Length) % cacNutMenu.Length;
            ChonNutMenu(nutDangChon);
        }

        if (ok && nutDangChon >= 0 && nutDangChon < cacNutMenu.Length)
            cacNutMenu[nutDangChon].onClick.Invoke();
    }

    void ChonNutMenu(int idx)
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(cacNutMenu[idx].gameObject);
        MenuSelectionFrame.ChonNut(cacNutMenu[idx].GetComponent<RectTransform>());
    }

    // === NÚT PLAY ===
    public void NutPlay()
    {
        Time.timeScale = 1f;

        string levelMoiNhat;
        if (GameManager.instance != null)
            levelMoiNhat = GameManager.instance.LayLevelMoiNhat();
        else
            levelMoiNhat = PlayerPrefs.GetString("LatestLevel", "Level_S_0");

        // Nếu level mới nhất chưa được unlock (save data cũ/lỗi), về S_0
        bool daUnlock = PlayerPrefs.GetInt(levelMoiNhat + "_unlocked", 0) == 1;
        if (!daUnlock) levelMoiNhat = "Level_S_0";

        GameManager.ResetSoLanChet();
        StartCoroutine(FadeOutRoi(() =>
            SceneManager.LoadScene(levelMoiNhat)));
    }

    // === NÚT LEVEL SELECT ===
    public void NutLevelSelect()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeOutRoi(() =>
            SceneManager.LoadScene(tenSceneLevelSelect)));
    }

    // === NÚT SETTINGS ===
    public void NutSettings()
    {
        SettingsMenu sm = SettingsMenu.instance;
        if (sm == null) sm = FindFirstObjectByType<SettingsMenu>();
        if (sm != null)
            sm.NutMoSettings();
        else
            Debug.LogWarning("[MainMenu] Không tìm thấy SettingsMenu trong scene!" +
                " Hãy thêm GameObject có SettingsMenu component vào scene MainMenu.");
    }

    // === NÚT QUIT ===
    public void NutQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // === FADE IN ===
    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < thoiGianFadeIn)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / thoiGianFadeIn);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    // === FADE OUT rồi thực hiện action ===
    IEnumerator FadeOutRoi(System.Action action)
    {
        if (canvasGroup != null)
        {
            float t = 0f;
            float thoiGian = 0.4f;
            while (t < thoiGian)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(1f - t / thoiGian);
                yield return null;
            }
        }
        action?.Invoke();
    }

    public void NutSkinSelect()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeOutRoi(() =>
            SceneManager.LoadScene("SkinSelect")));
    }

    public void NutThanhTuu()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeOutRoi(() =>
            SceneManager.LoadScene("Achievements")));
    }
}