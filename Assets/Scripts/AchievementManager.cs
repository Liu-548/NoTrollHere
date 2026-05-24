using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    // =============================================================
    // ĐỊNH NGHĨA THÀNH TỰU
    // =============================================================
    public class ThanhTuu
    {
        public string id;
        public string ten;
        public string moTa;
        public string nhanXet; // câu hài hước hiện khi đạt
        public string skinMo;  // skin mở kèm (nếu có)
        public bool daUnlock;

        public ThanhTuu(string id, string ten, string moTa,
            string nhanXet, string skinMo = "")
        {
            this.id = id;
            this.ten = ten;
            this.moTa = moTa;
            this.nhanXet = nhanXet;
            this.skinMo = skinMo;
            this.daUnlock = PlayerPrefs.GetInt("ACH_" + id, 0) == 1;
        }
    }

    private List<ThanhTuu> danhSach = new List<ThanhTuu>();

    // Theo dõi bẫy liên tiếp
    private string bayVuaChet = "";
    private int soLanChetBayLienTiep = 0;

    // Theo dõi không chết trong màn
    private bool chetTrongManNay = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            KhoiTaoDanhSach();
        }
        else Destroy(gameObject);
    }

    void KhoiTaoDanhSach()
    {
        danhSach.Clear();

        // === CHẾT MILESTONE ===
        danhSach.Add(new ThanhTuu(
            "DEATH_100",
            "Century Club",
            "Chết 100 lần",
            "Bạn đã quen với cảm giác này rồi phải không?",
            "Ghost"
        ));
        danhSach.Add(new ThanhTuu(
            "DEATH_500",
            "Are You Okay?",
            "Chết 500 lần",
            "Xin hỏi... bạn có ổn không? Thật sự ấy.",
            "Skeleton"
        ));
        danhSach.Add(new ThanhTuu(
            "DEATH_1000",
            "Pain Enthusiast",
            "Chết 1000 lần",
            "Tôi không còn gì để nói nữa. Respect.",
            ""
        ));

        // === HOÀN THÀNH CHƯƠNG (1-9) ===
        for (int ch = 1; ch <= 4; ch++)
        {
            int chuong = ch;
            string[] tenChuong = { "", "NoWhere", "The Forest", "The Volcano", "The Abyss" };
            string[] nhanXet = {
                "",
                "Vậy là bạn đã sống sót qua NoWhere. Chúc mừng... tạm thời.",
                "Rừng không giết được bạn. Nhưng tôi vẫn còn nhiều ý tưởng.",
                "Núi lửa thất bại. Tôi thất vọng.",
                "Vực thẳm cũng không đủ. Bạn là ai vậy?"
            };
            string[] skinChuong = { "", "Forest", "Ember", "Void", "" };

            danhSach.Add(new ThanhTuu(
                $"CLEAR_CH{chuong}_1TO9",
                $"Chapter {chuong} Complete",
                $"Hoàn thành Chapter {chuong} — {tenChuong[chuong]} (màn 1-9)",
                nhanXet[chuong],
                skinChuong[chuong]
            ));
        }

        // === HOÀN THÀNH MÀN 10 (boss) ===
        for (int ch = 1; ch <= 4; ch++)
        {
            int chuong = ch;
            string[] nhanXet = {
                "",
                "The Betrayal không phá nổi bạn. Tôi cần thiết kế lại.",
                "Khu rừng đã cúi đầu. Lần này tôi thật sự nghiêng mình.",
                "Bạn đi qua núi lửa như đi dạo. Kinh khủng.",
                "Vực thẳm... vực thẳm đã thua.믿을 수 없어."
            };
            string[] skinBonus = { "", "Golden", "", "", "" };

            danhSach.Add(new ThanhTuu(
                $"CLEAR_CH{chuong}_10",
                $"The Betrayal — Chapter {chuong}",
                $"Hoàn thành màn đặc biệt Chapter {chuong}",
                nhanXet[chuong],
                skinBonus[chuong]
            ));
        }

        // === QUA MÀN KHÔNG CHẾT ===
        danhSach.Add(new ThanhTuu(
            "NO_DEATH_CLEAR",
            "Speedrunner",
            "Qua 1 màn mà không chết lần nào",
            "Ủa, bạn có gian lận không đấy?",
            ""
        ));

        // === CHẾT VÌ 1 LOẠI BẪY 10 LẦN LIÊN TIẾP ===
        danhSach.Add(new ThanhTuu(
            "TRAP_STREAK_FAKE",
            "Kẻ Ngây Thơ",
            "Chết vì FakePlatform 10 lần liên tiếp",
            "Platform đó đã biến mất 10 lần. Lần 11 bạn vẫn nhảy lên.",
            ""
        ));
        danhSach.Add(new ThanhTuu(
            "TRAP_STREAK_SPIKE",
            "Nhím Người",
            "Chết vì Spike 10 lần liên tiếp",
            "Spike không tự đến với bạn. Nhưng bạn cứ chạy vào.",
            ""
        ));
        danhSach.Add(new ThanhTuu(
            "TRAP_STREAK_FALLING",
            "Đội Trưởng Nhìn Lên",
            "Chết vì FallingBrick 10 lần liên tiếp",
            "Gạch rơi từ trên xuống. Từ. Trên. Xuống. Nhìn lên đi bạn ơi.",
            ""
        ));
        danhSach.Add(new ThanhTuu(
            "TRAP_STREAK_SPRING",
            "Người Yêu Lò Xo",
            "Chết vì SpringTrap 10 lần liên tiếp",
            "Lò xo đó bắn bạn lên 10 lần. Có vẻ bạn thích bay.",
            ""
        ));
        danhSach.Add(new ThanhTuu(
            "TRAP_STREAK_WALL",
            "Ôm Tường",
            "Chết vì WallSpike 10 lần liên tiếp",
            "Tường có spike. Bạn biết điều đó. Nhưng vẫn tiếp tục.",
            ""
        ));
        danhSach.Add(new ThanhTuu(
            "TRAP_STREAK_BETRAYING",
            "Tín Đồ Platform",
            "Chết vì BetrayingPlatform 10 lần liên tiếp",
            "Platform phản bội bạn 10 lần. Vậy mà bạn vẫn tin tưởng nó.",
            ""
        ));
    }

    // =============================================================
    // CHECK SAU KHI CHẾT
    // =============================================================
    public void KiemTraSauKhiChet(string loaiBay)
    {
        chetTrongManNay = true;
        int tongChet = PlayerPrefs.GetInt("Deaths_Total", 0);

        // Death milestone
        KiemTraVaMo("DEATH_100", tongChet >= 100);
        KiemTraVaMo("DEATH_500", tongChet >= 500);
        KiemTraVaMo("DEATH_1000", tongChet >= 1000);

        // Bẫy liên tiếp
        if (!string.IsNullOrEmpty(loaiBay))
        {
            if (loaiBay == bayVuaChet)
                soLanChetBayLienTiep++;
            else
            {
                bayVuaChet = loaiBay;
                soLanChetBayLienTiep = 1;
            }

            if (soLanChetBayLienTiep >= 10)
            {
                switch (loaiBay)
                {
                    case "FakePlatform":
                        KiemTraVaMo("TRAP_STREAK_FAKE", true); break;
                    case "Spike":
                    case "HiddenSpike":
                        KiemTraVaMo("TRAP_STREAK_SPIKE", true); break;
                    case "FallingBrick":
                        KiemTraVaMo("TRAP_STREAK_FALLING", true); break;
                    case "SpringTrap":
                        KiemTraVaMo("TRAP_STREAK_SPRING", true); break;
                    case "WallSpike":
                        KiemTraVaMo("TRAP_STREAK_WALL", true); break;
                    case "BetrayingPlatform":
                        KiemTraVaMo("TRAP_STREAK_BETRAYING", true); break;
                }
                soLanChetBayLienTiep = 0;
            }
        }
        else
        {
            // Chết không rõ nguyên nhân → reset streak
            bayVuaChet = "";
            soLanChetBayLienTiep = 0;
        }
    }

    // =============================================================
    // CHECK SAU KHI THẮNG MÀN
    // =============================================================
    public void KiemTraSauKhiThang(string tenMan)
    {
        // Không chết trong màn
        if (!chetTrongManNay)
            KiemTraVaMo("NO_DEATH_CLEAR", true);

        chetTrongManNay = false;
        bayVuaChet = "";
        soLanChetBayLienTiep = 0;

        // Parse tên màn Level_X_Y
        if (!tenMan.StartsWith("Level_")) return;
        string[] parts = tenMan.Split('_');
        if (parts.Length < 3) return;
        if (parts[1] == "S") return;
        if (!int.TryParse(parts[1], out int soChuong)) return;
        if (!int.TryParse(parts[2], out int soMan)) return;
        if (soChuong < 1 || soChuong > 4) return;

        // Hoàn thành màn 9 → check clear 1-9
        if (soMan == 9)
        {
            bool hoanThanh19 = true;
            for (int i = 1; i <= 9; i++)
            {
                if (PlayerPrefs.GetInt($"Level_{soChuong}_{i}_passed", 0) == 0)
                { hoanThanh19 = false; break; }
            }
            if (hoanThanh19)
                KiemTraVaMo($"CLEAR_CH{soChuong}_1TO9", true);
        }

        // Hoàn thành màn 10
        if (soMan == 10)
            KiemTraVaMo($"CLEAR_CH{soChuong}_10", true);
    }

    // =============================================================
    // MỞ THÀNH TỰU
    // =============================================================
    void KiemTraVaMo(string id, bool dieuKien)
    {
        if (!dieuKien) return;

        ThanhTuu tt = danhSach.Find(t => t.id == id);
        if (tt == null || tt.daUnlock) return;

        tt.daUnlock = true;
        PlayerPrefs.SetInt("ACH_" + id, 1);
        PlayerPrefs.Save();

        // Mở skin nếu có
        if (!string.IsNullOrEmpty(tt.skinMo))
            MoSkin(tt.skinMo);

        // Hiện popup
        if (AchievementPopup.instance != null)
            AchievementPopup.instance.HienThiPopup(tt.ten, tt.moTa, tt.nhanXet);

        Debug.Log($"🏆 Thành tựu mở: {tt.ten}");
    }

    void MoSkin(string tenSkin)
    {
        PlayerPrefs.SetInt("Skin_" + tenSkin + "_unlocked", 1);
        PlayerPrefs.Save();

        // Thêm dòng này
        if (SkinManager.instance != null)
            SkinManager.instance.MoSkin(tenSkin);

        Debug.Log($"🎨 Mở skin: {tenSkin}");
    }

    // =============================================================
    // LẤY DANH SÁCH (cho màn xem tất cả)
    // =============================================================
    public List<ThanhTuu> LayDanhSach() => danhSach;

    public int LaySoLuongDaMo()
        => danhSach.FindAll(t => t.daUnlock).Count;
}