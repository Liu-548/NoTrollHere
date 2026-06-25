using UnityEngine;
using System.Collections.Generic;

public class SkinManager : MonoBehaviour
{
    public static SkinManager instance;

    // =============================================================
    // ĐỊNH NGHĨA SKIN
    // =============================================================
    public class Skin
    {
        public string id;
        public string ten;
        public string moTa;
        public string dieuKienMo;
        public bool daUnlock;

        public Skin(string id, string ten, string moTa, string dieuKienMo)
        {
            this.id = id;
            this.ten = ten;
            this.moTa = moTa;
            this.dieuKienMo = dieuKienMo;
            this.daUnlock = PlayerPrefs.GetInt("Skin_" + id + "_unlocked", 0) == 1;
        }
    }

    [System.Serializable]
    public class SkinSprite
    {
        public string id;
        public Sprite sprite; // frame đầu tiên — dùng làm preview
    }

    [System.Serializable]
    public class SkinAnimator
    {
        public string id;
        public RuntimeAnimatorController controller;
        // Default dùng AnimatorController gốc
        // Skin khác dùng AnimatorOverrideController

        [Tooltip("Để (0,0) = giữ nguyên collider mặc định. Đặt giá trị nếu skin có sprite khác kích thước.")]
        public Vector2 colSize;
        [Tooltip("Offset của BoxCollider2D cho skin này. Để (0,0) = giữ nguyên.")]
        public Vector2 colOffset;
    }

    [Header("=== SPRITES (preview) ===")]
    public List<SkinSprite> cacSkinSprite = new List<SkinSprite>();

    [Header("=== ANIMATORS ===")]
    public List<SkinAnimator> cacSkinAnimator = new List<SkinAnimator>();

    private List<Skin> danhSachSkin = new List<Skin>();
    private string skinDangDung = "Default";

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
        danhSachSkin.Clear();

        // Default luôn mở, các skin còn lại unlock qua achievement
        ThemSkin("Default",   "Default",  "Nhân vật chalk nguyên bản",  "Mặc định",              true);
        ThemSkin("Ghost",     "Ghost",    "Trong suốt như... ma",        "Chết 100 lần",           false);
        ThemSkin("Forest",    "Forest",   "Hòa mình vào rừng xanh",     "Hoàn thành Chapter 1",   false);
        ThemSkin("Ember",     "Ember",    "Tôi luyện qua lửa",          "Hoàn thành Chapter 2",   false);
        ThemSkin("Void",      "Void",     "Từ vực thẳm trở về",         "Hoàn thành Chapter 3",   false);
        ThemSkin("Skeleton",  "Skeleton", "Chỉ còn lại xương",          "Chết 500 lần",           false);
        ThemSkin("Golden",    "Golden",   "Huyền thoại thực sự",        "Hoàn thành Chapter 4",   false);
        ThemSkin("IceMan",    "Ice",      "Băng giá vĩnh cửu",          "Hoàn thành Chapter 4",   false);
        ThemSkin("Lava",      "Lava",     "Dung nham sục sôi",          "Hoàn thành Chapter 3",   false);
        ThemSkin("Robot",     "Robot",    "Kim loại lạnh lùng",         "Qua màn không chết",     false);

        skinDangDung = PlayerPrefs.GetString("Skin_DangDung", "Default");
    }

    void ThemSkin(string id, string ten, string moTa,
        string dieuKien, bool moNgay)
    {
        var skin = new Skin(id, ten, moTa, dieuKien);
        if (moNgay)
        {
            skin.daUnlock = true;
            PlayerPrefs.SetInt("Skin_" + id + "_unlocked", 1);
        }
        danhSachSkin.Add(skin);
    }

    // =============================================================
    // LẤY SPRITE PREVIEW
    // =============================================================
    public Sprite LaySpriteSkin(string id)
    {
        SkinSprite ss = cacSkinSprite.Find(s => s.id == id);
        if (ss != null && ss.sprite != null) return ss.sprite;

        // Fallback Default
        SkinSprite def = cacSkinSprite.Find(s => s.id == "Default");
        return def?.sprite;
    }

    // =============================================================
    // CHỌN SKIN
    // =============================================================
    public void ChonSkin(string id)
    {
        Skin skin = danhSachSkin.Find(s => s.id == id);
        if (skin == null || !skin.daUnlock) return;

        skinDangDung = id;
        PlayerPrefs.SetString("Skin_DangDung", id);
        PlayerPrefs.Save();

        ApSkinLenPlayer();
        Debug.Log($"Đang dùng skin: {id}");
    }

    // =============================================================
    // ÁP SKIN LÊN PLAYER
    // =============================================================
    public void ApSkinLenPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Đổi sprite frame đầu
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        Sprite sprite = LaySpriteSkin(skinDangDung);
        if (sr != null && sprite != null)
            sr.sprite = sprite;

        // Đổi Animator Controller
        Animator anim = player.GetComponent<Animator>();
        if (anim == null) return;

        SkinAnimator sa = cacSkinAnimator.Find(s => s.id == skinDangDung);
        if (sa != null && sa.controller != null)
            anim.runtimeAnimatorController = sa.controller;
        else
        {
            // Fallback về Default
            SkinAnimator def = cacSkinAnimator.Find(s => s.id == "Default");
            if (def != null && def.controller != null)
                anim.runtimeAnimatorController = def.controller;
        }

        // Root motion có thể đẩy nhân vật nổi lên — luôn tắt sau khi đổi controller
        anim.applyRootMotion = false;

        // Cập nhật BoxCollider2D theo cấu hình thủ công của skin (nếu có)
        BoxCollider2D col2 = player.GetComponent<BoxCollider2D>();
        PlayerController pc2 = player.GetComponent<PlayerController>();
        if (sa != null && (sa.colSize != Vector2.zero || sa.colOffset != Vector2.zero))
        {
            if (col2 != null && pc2 != null)
            {
                if (sa.colSize   != Vector2.zero) { col2.size   = sa.colSize;   pc2.kichThuocColGoc = sa.colSize;   }
                if (sa.colOffset != Vector2.zero) { col2.offset = sa.colOffset; pc2.offsetColGoc    = sa.colOffset; }
            }
        }
        else if (pc2 != null && col2 != null)
        {
            // Reset về giá trị gốc khi đổi sang skin không có colSize/colOffset
            col2.size   = pc2.kichThuocColGoc;
            col2.offset = pc2.offsetColGoc;
        }

    }

    public string LaySkinDangDung() => skinDangDung;
    public List<Skin> LayDanhSach() => danhSachSkin;

    public void ReloadUnlocks()
    {
        foreach (var skin in danhSachSkin)
            skin.daUnlock = PlayerPrefs.GetInt("Skin_" + skin.id + "_unlocked", 0) == 1;
    }

    // =============================================================
    // MỞ SKIN — gọi từ AchievementManager
    // =============================================================
    public void MoSkin(string id)
    {
        Skin skin = danhSachSkin.Find(s => s.id == id);
        if (skin == null || skin.daUnlock) return;

        skin.daUnlock = true;
        PlayerPrefs.SetInt("Skin_" + id + "_unlocked", 1);
        PlayerPrefs.Save();
        Debug.Log($"🎨 Mở skin: {id}");
    }
}