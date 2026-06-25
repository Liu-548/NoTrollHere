using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public string tenSceneLevelSelect = "LevelSelect";

    [Header("=== ANIMATION ===")]
    public float thoiGianFadeIn = 0.8f;

    [Header("=== THAM CHIẾU UI ===")]
    public CanvasGroup canvasGroup;

    void Start()
    {
        StartCoroutine(FadeIn());

        // Play nhạc menu
        if (SoundManager.instance != null)
            SoundManager.instance.PlayNhacMenuCh1();
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