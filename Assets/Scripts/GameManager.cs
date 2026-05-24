using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int soLanChet = 0;
    private int tongSoLanChetSession = 0;

    [Header("=== CẤU HÌNH LEVEL ===")]
    public int soManMoiChuong = 9;
    // Màn 1-9 để thắng chương, màn 10 là đặc biệt
    public int soManChuong1 = 10;
    public string tenSceneTutorial = "Level_S_0";
    public string tenSceneMainMenu = "MainMenu";
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
        KhoiTaoUnlock();
    }

    void KhoiTaoUnlock()
    {
        if (PlayerPrefs.GetInt("Level_1_1_unlocked", 0) == 0)
        {
            PlayerPrefs.SetInt("Level_1_1_unlocked", 1);
            PlayerPrefs.Save();
        }
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

        // Lưu tổng số chết vĩnh viễn
        int tongHienTai = PlayerPrefs.GetInt("Deaths_Total", 0);
        PlayerPrefs.SetInt("Deaths_Total", tongHienTai + 1);

        // Lưu số chết theo từng level
        string tenLevel = UnityEngine.SceneManagement
            .SceneManager.GetActiveScene().name;
        int chetLevelNay = PlayerPrefs.GetInt("Deaths_" + tenLevel, 0);
        PlayerPrefs.SetInt("Deaths_" + tenLevel, chetLevelNay + 1);
        PlayerPrefs.Save();

        if (PauseMenu.instance != null)
            PauseMenu.instance.ThemMotLanChet();

        if (AchievementManager.instance != null)
            AchievementManager.instance.KiemTraSauKhiChet("");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // =============================================================
    // THẮNG MÀN
    // =============================================================
    public void ThangMan(string tenManTiepTheo)
    {
        string tenManHienTai = SceneManager.GetActiveScene().name;

        // Đánh dấu màn hiện tại đã pass
        DanhDauDaPass(tenManHienTai);

        // Unlock màn tiếp theo
        PlayerPrefs.SetInt(tenManTiepTheo + "_unlocked", 1);
        PlayerPrefs.SetString("LatestLevel", tenManTiepTheo);
        PlayerPrefs.Save();

        // Kiểm tra mở chương tiếp theo
        KiemTraMoChuongTiepTheo(tenManHienTai);

        if (AchievementManager.instance != null)
            AchievementManager.instance.KiemTraSauKhiThang(tenManHienTai);

        soLanChet = 0;
        SceneManager.LoadScene(tenManTiepTheo);
    }

    // =============================================================
    // KIỂM TRA MỞ CHƯƠNG TIẾP THEO
    // Logic: thắng màn 1-9 của chương X → mở màn 10 (đặc biệt)
    //        thắng màn 10 → mở chương tiếp theo
    // =============================================================
    void KiemTraMoChuongTiepTheo(string tenManHienTai)
    {
        // Parse tên màn — format: Level_X_Y
        // X = số chương, Y = số màn
        if (!tenManHienTai.StartsWith("Level_")) return;

        string[] parts = tenManHienTai.Split('_');
        if (parts.Length < 3) return;

        // Xử lý chương Special riêng
        if (parts[1] == "S") return;

        if (!int.TryParse(parts[1], out int soChuong)) return;
        if (!int.TryParse(parts[2], out int soMan)) return;

        // Kiểm tra đã pass màn 1-9 của chương này chưa
        if (soMan == soManMoiChuong)
        {
            // Đã pass màn 9 → kiểm tra tất cả 1-9 đã pass chưa
            bool hoanThanhChuong = true;
            for (int i = 1; i <= soManMoiChuong; i++)
            {
                string key = $"Level_{soChuong}_{i}_passed";
                if (PlayerPrefs.GetInt(key, 0) == 0)
                {
                    hoanThanhChuong = false;
                    break;
                }
            }

            if (hoanThanhChuong)
            {
                // Mở màn 10 (đặc biệt) của chương này
                string man10 = $"Level_{soChuong}_10";
                PlayerPrefs.SetInt(man10 + "_unlocked", 1);
                PlayerPrefs.Save();
                Debug.Log($"Mở màn đặc biệt: {man10}");

                // Nếu là chương 1 → mở chương Special
                if (soChuong == 1)
                {
                    PlayerPrefs.SetInt("Chapter_Special_unlocked", 1);
                    PlayerPrefs.SetInt(tenSceneTutorial + "_unlocked", 1);
                    PlayerPrefs.Save();
                    Debug.Log("Chương Special đã mở!");
                }
            }
        }

        // Thắng màn 10 → mở chương tiếp theo
        if (soMan == 10)
        {
            int chuongTiep = soChuong + 1;
            string man1ChuongTiep = $"Level_{chuongTiep}_1";
            PlayerPrefs.SetInt(man1ChuongTiep + "_unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log($"Mở chương {chuongTiep}: {man1ChuongTiep}");
        }
    }

    // =============================================================
    // ĐÁNH DẤU MÀN ĐÃ PASS
    // =============================================================
    public void DanhDauDaPass(string tenMan)
    {
        PlayerPrefs.SetInt(tenMan + "_passed", 1);
        PlayerPrefs.Save();
    }

    public string LayLevelMoiNhat()
        => PlayerPrefs.GetString("LatestLevel", "Level_1_1");

    public bool LaLevelDaUnlock(string tenLevel)
        => PlayerPrefs.GetInt(tenLevel + "_unlocked", 0) == 1;

    public bool LaChapterSpecialDaMo()
        => PlayerPrefs.GetInt("Chapter_Special_unlocked", 0) == 1;

    public int LaySoLanChet() => soLanChet;
    public int LayTongSoLanChetSession() => tongSoLanChetSession;

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        KhoiTaoUnlock();
        Debug.Log("Đã reset toàn bộ progress!");
    }

    // Tổng số lần chết toàn bộ — đọc từ PlayerPrefs
    public int LayTongSoLanChetTatCa()
    {
        return PlayerPrefs.GetInt("Deaths_Total", 0);
    }
}