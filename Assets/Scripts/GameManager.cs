using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // =============================================================
    // SINGLETON
    // =============================================================
    public static GameManager instance;

    // Số lần chết trong màn hiện tại
    private int soLanChet = 0;

    // Tổng số lần chết toàn session (cho Main Menu footer)
    private int tongSoLanChetSession = 0;

    // =============================================================
    // CẤU HÌNH CHƯƠNG — chỉnh trong Inspector hoặc hardcode
    // =============================================================
    [Header("=== CẤU HÌNH LEVEL ===")]
    // Số màn trong chương 1
    public int soManChuong1 = 10;
    // Tên scene tutorial (chương Special)
    public string tenSceneTutorial = "Level_S_0";
    // Tên scene Main Menu
    public string tenSceneMainMenu = "MainMenu";
    // Tên scene Level Select
    public string tenSceneLevelSelect = "LevelSelect";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Đảm bảo Level_1_1 luôn unlock
        KhoiTaoUnlock();
    }

    // =============================================================
    // KHỞI TẠO — đảm bảo level đầu luôn unlock
    // =============================================================
    void KhoiTaoUnlock()
    {
        if (PlayerPrefs.GetInt("Level_1_1_unlocked", 0) == 0)
        {
            PlayerPrefs.SetInt("Level_1_1_unlocked", 1);
            PlayerPrefs.Save();
        }

        // Đảm bảo LatestLevel luôn có giá trị mặc định
        if (!PlayerPrefs.HasKey("LatestLevel"))
        {
            PlayerPrefs.SetString("LatestLevel", "Level_1_1");
            PlayerPrefs.Save();
        }
    }

    // =============================================================
    // PLAYER CHẾT
    // =============================================================
    public void PlayerChet()
    {
        soLanChet++;
        tongSoLanChetSession++;

        // Thông báo cho PauseMenu đếm chết theo màn
        if (PauseMenu.instance != null)
            PauseMenu.instance.ThemMotLanChet();

        Debug.Log("Số lần chết: " + soLanChet);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // =============================================================
    // THẮNG MÀN
    // =============================================================
    public void ThangMan(string tenManTiepTheo)
    {
        Debug.Log("Thắng màn! Chuyển sang: " + tenManTiepTheo);

        // Unlock màn tiếp theo
        PlayerPrefs.SetInt(tenManTiepTheo + "_unlocked", 1);
        PlayerPrefs.Save();

        // Lưu level mới nhất đã mở (cho nút Play)
        PlayerPrefs.SetString("LatestLevel", tenManTiepTheo);
        PlayerPrefs.Save();

        // Kiểm tra hoàn thành chương 1 → unlock Special
        KiemTraMoSpecial();

        // Reset số chết màn
        soLanChet = 0;

        SceneManager.LoadScene(tenManTiepTheo);
    }

    // =============================================================
    // KIỂM TRA MỞ CHƯƠNG SPECIAL
    // =============================================================
    void KiemTraMoSpecial()
    {
        // Kiểm tra tất cả 10 màn chương 1 đã pass chưa
        bool hoanThanhChuong1 = true;
        for (int i = 1; i <= soManChuong1; i++)
        {
            // Level tiếp theo của màn cuối = LevelSelect → đã pass
            // Dùng key "_unlocked" của màn TIẾP THEO để biết màn này đã pass
            string keyManNay = "Level_1_" + i + "_passed";
            if (PlayerPrefs.GetInt(keyManNay, 0) == 0)
            {
                hoanThanhChuong1 = false;
                break;
            }
        }

        if (hoanThanhChuong1)
        {
            PlayerPrefs.SetInt("Chapter_Special_unlocked", 1);
            PlayerPrefs.SetInt(tenSceneTutorial + "_unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log("Chương Special đã mở!");
        }
    }

    // =============================================================
    // ĐÁNH DẤU MÀN ĐÃ PASS (gọi từ Goal.cs)
    // =============================================================
    public void DanhDauDaPass(string tenManHienTai)
    {
        PlayerPrefs.SetInt(tenManHienTai + "_passed", 1);
        PlayerPrefs.Save();
    }

    // =============================================================
    // LẤY LEVEL MỚI NHẤT (cho nút Play)
    // =============================================================
    public string LayLevelMoiNhat()
    {
        // Mặc định là Level_1_1 nếu chưa có progress
        return PlayerPrefs.GetString("LatestLevel", "Level_1_1");
    }

    // =============================================================
    // KIỂM TRA LEVEL ĐÃ UNLOCK
    // =============================================================
    public bool LaLevelDaUnlock(string tenLevel)
    {
        return PlayerPrefs.GetInt(tenLevel + "_unlocked", 0) == 1;
    }

    // =============================================================
    // KIỂM TRA CHƯƠNG SPECIAL ĐÃ MỞ
    // =============================================================
    public bool LaChapterSpecialDaMo()
    {
        return PlayerPrefs.GetInt("Chapter_Special_unlocked", 0) == 1;
    }

    // =============================================================
    // LẤY SỐ LẦN CHẾT
    // =============================================================
    public int LaySoLanChet() => soLanChet;
    public int LayTongSoLanChetSession() => tongSoLanChetSession;

    // =============================================================
    // RESET TOÀN BỘ PROGRESS (dùng khi test)
    // =============================================================
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        KhoiTaoUnlock();
        Debug.Log("Đã reset toàn bộ progress!");
    }
}