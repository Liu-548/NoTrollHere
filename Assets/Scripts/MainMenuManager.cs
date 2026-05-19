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
        // Fallback nếu không có GameManager
        string levelMoiNhat = "Level_1_1";

        if (GameManager.instance != null)
            levelMoiNhat = GameManager.instance.LayLevelMoiNhat();
        else
            levelMoiNhat = PlayerPrefs.GetString("LatestLevel", "Level_1_1");

        StartCoroutine(FadeOutRoi(() =>
            SceneManager.LoadScene(levelMoiNhat)));
    }

    // === NÚT LEVEL SELECT ===
    public void NutLevelSelect()
    {
        StartCoroutine(FadeOutRoi(() =>
            SceneManager.LoadScene(tenSceneLevelSelect)));
    }

    // === NÚT SETTINGS (placeholder) ===
    public void NutSettings()
    {
        Debug.Log("Settings: coming soon");
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
}