using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int soLanChet = 0;
    private int tongSoLanChetSession = 0;

    [Header("=== CẤU HÌNH LEVEL ===")]
    public int soManMoiChuong = 8; // Mỗi chương có 8 màn, qua hết → mở chương tiếp
    public string tenSceneTutorial = "Level_S_0";
    public string tenSceneMainMenu = "MainMenu";
    public string tenSceneLevelSelect = "LevelSelect";

    [Header("=== GIỚI HẠN PHÁT HÀNH ===")]
    [Tooltip("Chapter cao nhất có trong build. Đặt = 2 để khóa Ch3+. Tăng lên khi thêm chapter mới.")]
    public int chuongToiDa = 2;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            DamBaoCoEventSystem();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        KhoiTaoUnlock();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mỗi scene tự có EventSystem riêng — chỉ tạo nếu scene không có
        DamBaoCoEventSystem();
    }

    void DamBaoCoEventSystem()
    {
        var tatCaES = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (tatCaES.Length == 0)
        {
            // Scene không có EventSystem → tạo mới cho scene này (KHÔNG DontDestroyOnLoad)
            GameObject goES = new GameObject("EventSystem");
            goES.AddComponent<EventSystem>();
            goES.AddComponent<StandaloneInputModule>();
            Debug.Log($"[GameManager] Tạo EventSystem cho scene: {SceneManager.GetActiveScene().name}");
        }
        else if (tatCaES.Length > 1)
        {
            // Có nhiều hơn 1 → giữ cái thuộc scene hiện tại, xóa các cái từ DDOL
            Scene activeScene = SceneManager.GetActiveScene();
            EventSystem giuLai = null;

            // Ưu tiên giữ cái thuộc scene đang active
            foreach (var es in tatCaES)
                if (es.gameObject.scene == activeScene) { giuLai = es; break; }

            // Fallback: giữ cái đầu tiên
            if (giuLai == null) giuLai = tatCaES[0];

            foreach (var es in tatCaES)
            {
                if (es != giuLai)
                {
                    Debug.Log($"[GameManager] Xóa EventSystem thừa: {es.gameObject.name} (scene: {es.gameObject.scene.name})");
                    Destroy(es.gameObject);
                }
            }
        }
    }

    void KhoiTaoUnlock()
    {
        // Lần đầu chơi (chưa có bất kỳ unlock nào) → tutorial trước
        bool coSaveNao = PlayerPrefs.HasKey("Level_S_0_unlocked")
                      || PlayerPrefs.HasKey("Level_1_1_unlocked");

        if (!coSaveNao)
        {
            // Lần đầu tiên: chỉ mở Level_S_0 (Tutorial)
            PlayerPrefs.SetInt("Level_S_0_unlocked", 1);
            PlayerPrefs.SetString("LatestLevel", "Level_S_0");
            PlayerPrefs.Save();
        }
        else
        {
            // Save cũ chưa có S_0 unlock → thêm vào (compat)
            if (PlayerPrefs.GetInt("Level_S_0_unlocked", 0) == 0)
                PlayerPrefs.SetInt("Level_S_0_unlocked", 1);

            // Đảm bảo luôn có LatestLevel
            if (!PlayerPrefs.HasKey("LatestLevel"))
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

        // Chỉ check milestone — KHÔNG reset streak (streak do KillZone quản lý)
        if (AchievementManager.instance != null)
            AchievementManager.instance.KiemTraMilestonesChet();

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
    // Logic: mỗi chương có 8 màn, pass đủ 8 → mở chương tiếp
    //        Chương Special (S) không giới hạn, không trigger unlock
    // =============================================================
    void KiemTraMoChuongTiepTheo(string tenManHienTai)
    {
        if (!tenManHienTai.StartsWith("Level_")) return;
        string[] parts = tenManHienTai.Split('_');
        if (parts.Length < 3) return;

        // Chương Special
        if (parts[1] == "S")
        {
            // Level_S_0 (Tutorial) xong → unlock Level_1_1
            if (parts[2] == "0")
            {
                PlayerPrefs.SetInt("Level_1_1_unlocked", 1);
                PlayerPrefs.Save();
                Debug.Log("[GameManager] Tutorial xong → Mở Level_1_1");
            }
            return;
        }

        if (!int.TryParse(parts[1], out int soChuong)) return;
        if (!int.TryParse(parts[2], out int soMan)) return;

        // Chỉ kiểm tra khi vừa xong màn cuối (màn 8)
        if (soMan != soManMoiChuong) return;

        // Kiểm tra tất cả 1 → soManMoiChuong đã pass
        bool hoanThanhChuong = true;
        for (int i = 1; i <= soManMoiChuong; i++)
        {
            if (PlayerPrefs.GetInt($"Level_{soChuong}_{i}_passed", 0) == 0)
            { hoanThanhChuong = false; break; }
        }

        if (!hoanThanhChuong)
        {
            Debug.Log($"[GameManager] Ch{soChuong} màn {soMan} xong nhưng chưa pass đủ {soManMoiChuong} màn.");
            return;
        }

        // Mở màn 1 chương tiếp theo — chỉ nếu chapter đó có trong build
        int chuongTiep = soChuong + 1;
        if (chuongTiep > chuongToiDa)
        {
            Debug.Log($"[GameManager] Ch{soChuong} hoàn thành — Ch{chuongTiep} chưa có trong bản này.");
            PlayerPrefs.Save();
            return;
        }
        string man1ChuongTiep = $"Level_{chuongTiep}_1";
        PlayerPrefs.SetInt(man1ChuongTiep + "_unlocked", 1);

        // Chương 1 hoàn thành → mở Special chapter + tutorial
        if (soChuong == 1)
        {
            PlayerPrefs.SetInt("Chapter_Special_unlocked", 1);
            PlayerPrefs.SetInt(tenSceneTutorial + "_unlocked", 1);
            Debug.Log("[GameManager] Ch1 8/8 xong → Mở Ch2 + Special chapter");
        }
        else
        {
            Debug.Log($"[GameManager] Ch{soChuong} 8/8 xong → Mở Level_{chuongTiep}_1");
        }

        PlayerPrefs.Save();
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
        => PlayerPrefs.GetString("LatestLevel", "Level_S_0");

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