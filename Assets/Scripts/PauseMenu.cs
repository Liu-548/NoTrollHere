using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("=== THAM CHIẾU ===")]
    public GameObject panelPause;        // Kéo Panel_Pause vào đây
    public TextMeshProUGUI txtDeathInfo; // Có thể để trống nếu đã bỏ

    [Header("=== CÀI ĐẶT ===")]
    public string tenSceneMainMenu = "MainMenu";

    private int soLanChetManNay = 0;
    private bool dangPause = false;

    public static PauseMenu instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Đảm bảo panel ẩn lúc đầu
        if (panelPause != null)
            panelPause.SetActive(false);
    }

    void Update()
    {
        // Escape hoặc P để pause/resume
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (dangPause) NutResume();
            else NutPause();
        }
    }

    // Gọi từ DeathEffect khi player chết
    public void ThemMotLanChet()
    {
        soLanChetManNay++;
    }

    // === MỞ PAUSE ===
    public void NutPause()
    {
        dangPause = true;
        Time.timeScale = 0f;
        panelPause.SetActive(true);

        // Cập nhật text nếu có
        if (txtDeathInfo != null)
            txtDeathInfo.text = "you've died <color=#F5C842>" +
                soLanChetManNay + "</color> times on this level";
    }

    // === RESUME ===
    public void NutResume()
    {
        dangPause = false;
        Time.timeScale = 1f;
        panelPause.SetActive(false);
    }

    // === RESTART ===
    public void NutRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // === MAIN MENU ===
    public void NutMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(tenSceneMainMenu);
    }

    // Reset timeScale nếu scene bị destroy
    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}