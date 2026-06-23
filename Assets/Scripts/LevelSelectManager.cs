using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class LevelSelectManager : MonoBehaviour
{
    [Header("=== UI THAM CHIẾU ===")]
    public TextMeshProUGUI txtTenChuong;
    public TextMeshProUGUI txtMoTaChuong;
    public GameObject[] cacNutLevel;       // 8 ô cố định cho ch1-4
    public Button nutTrai;
    public Button nutPhai;
    public Button nutBack;

    [Header("=== SPECIAL CHAPTER SCROLL ===")]
    // Không cần gán tay — tự tạo runtime
    [HideInInspector] public ScrollRect scrollViewSpecial;
    [HideInInspector] public Transform scrollContent;

    // Runtime-generated scroll panel cho Special chapter
    private GameObject specialScrollPanel;
    private Transform specialGridContent;
    private List<GameObject> cacCellSpecial = new List<GameObject>();

    [Header("=== CÀI ĐẶT ===")]
    public string tenSceneMainMenu = "MainMenu";

    // =============================================================
    // THỨ TỰ CHƯƠNG: Special(0 trái) ← Chapter1(1 giữa) → Chapter2(2) → Chapter3(3)...
    // Index 0 = Special, Index 1 = Ch1, Index 2 = Ch2, ...
    // =============================================================
    private readonly string[] tenCacChuong =
    {
        "SPECIAL",      // 0 — trái nhất
        "CHAPTER 1",    // 1 — mặc định khi mở
        "CHAPTER 2",    // 2
        "CHAPTER 3",    // 3
        "CHAPTER 4",    // 4
    };

    private readonly string[] moTaCacChuong =
    {
        "something else entirely",
        "choose your suffering",
        "into the forest",
        "the volcano awaits",
        "into the abyss",
    };

    private readonly string[] prefixCacChuong =
    {
        "Level_S_",
        "Level_1_",
        "Level_2_",
        "Level_3_",
        "Level_4_",
    };

    private readonly int[] soManCacChuong =
    {
        2,  // Special: Level_S_0 (Tutorial) + Level_S_1 (không giới hạn, thêm dần)
        8,  // Chapter 1
        8,  // Chapter 2
        8,  // Chapter 3
        8,  // Chapter 4
    };

    // Tên level thực tế cho chương Special — thêm vào đây khi tạo level mới
    // Format: "Level_S_X" với X là số (0, 1, 2, ...)
    // private để tránh Unity serialize rỗng trong scene
    private readonly string[] tenLevelSpecial = { "Level_S_0", "Level_S_1" };

    private readonly Color[] mauAccentChuong =
    {
        new Color(0.75f, 0.52f, 0.99f, 1f), // Special: tím #C084FC
        new Color(0.96f, 0.78f, 0.26f, 1f), // Ch1: vàng #F5C842
        new Color(0.29f, 0.87f, 0.50f, 1f), // Ch2: xanh lá #4ADE80
        new Color(0.98f, 0.57f, 0.24f, 1f), // Ch3: cam #FB923C
        new Color(0.38f, 0.65f, 0.98f, 1f), // Ch4: xanh dương #60A5FA
    };

    private readonly Color[] mauNenDangChoiChuong =
    {
        new Color(0.10f, 0.05f, 0.15f, 1f), // Special: tím tối
        new Color(0.16f, 0.13f, 0.00f, 1f), // Ch1: vàng tối
        new Color(0.05f, 0.13f, 0.07f, 1f), // Ch2: xanh tối
        new Color(0.15f, 0.06f, 0.02f, 1f), // Ch3: cam tối
        new Color(0.03f, 0.08f, 0.15f, 1f), // Ch4: xanh dương tối
    };

    // Màu chung
    private readonly Color mauNenDaMo = new Color(0.18f, 0.18f, 0.18f, 1f);
    private readonly Color mauNenLock = new Color(0.11f, 0.11f, 0.11f, 1f);
    private readonly Color mauVienDaMo = new Color(0.23f, 0.23f, 0.23f, 1f);
    private readonly Color mauVienLock = new Color(0.14f, 0.14f, 0.14f, 1f);
    private readonly Color mauChuDaMo = new Color(0.91f, 0.91f, 0.82f, 1f);
    private readonly Color mauChuLock = new Color(0.20f, 0.20f, 0.20f, 1f);

    // Index 1 = Chapter 1 = mặc định khi mở
    private int chuongHienTai = 1;
    private string levelMoiNhat = "Level_1_1";

    void Start()
    {
        if (GameManager.instance != null)
            levelMoiNhat = GameManager.instance.LayLevelMoiNhat();
        else
            levelMoiNhat = PlayerPrefs.GetString("LatestLevel", "Level_1_1");

        // Tự wire lại tất cả buttons theo tên — bỏ qua giá trị Inspector để tránh gán sai
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
            {
                switch (btn.gameObject.name)
                {
                    case "Btn_Trai": nutTrai = btn; break;
                    case "Btn_Phai": nutPhai = btn; break;
                    case "Btn_Back": nutBack = btn; break;
                }
            }
        }

        WireButton(nutTrai, NutTrai);
        WireButton(nutPhai, NutPhai);
        WireButton(nutBack, NutBack);

        // Luôn mở ở Chapter 1 (index 1) khi vào Level Select
        chuongHienTai = 1;
        HienThiChuong(chuongHienTai);
    }

    // =============================================================
    // HIỂN THỊ CHƯƠNG
    // =============================================================
    void HienThiChuong(int idx)
    {
        if (ChapterBackground.instance != null)
            ChapterBackground.instance.DoiChuong(idx);

        bool duocXem = KiemTraDuocXem(idx);

        if (txtTenChuong != null)
        {
            txtTenChuong.text = "— " + tenCacChuong[idx] + " —";
            txtTenChuong.color = duocXem ? mauAccentChuong[idx] : mauChuLock;
        }
        if (txtMoTaChuong != null)
            txtMoTaChuong.text = duocXem ? moTaCacChuong[idx] : "???";

        CapNhatMuiTen(idx);

        // === CHƯƠNG SPECIAL — scroll, tạo ô động ===
        if (idx == 0)
        {
            HienThiChuongSpecial(duocXem);
            return;
        }

        // === CHƯƠNG THƯỜNG — ẩn special scroll, hiện grid cố định ===
        if (scrollViewSpecial != null) scrollViewSpecial.gameObject.SetActive(false);
        if (specialScrollPanel != null) specialScrollPanel.SetActive(false);

        // Khôi phục Grid_Levels container (có thể bị ẩn khi xem Special)
        if (cacNutLevel != null && cacNutLevel.Length > 0 && cacNutLevel[0] != null)
            cacNutLevel[0].transform.parent?.gameObject.SetActive(true);

        if (cacNutLevel == null || cacNutLevel.Length == 0)
        {
            Debug.LogError("[LevelSelectManager] cacNutLevel chưa gán! Chạy NoTrollHere > Build Level Select UI.");
            return;
        }

        string prefix = prefixCacChuong[idx];
        int soMan = soManCacChuong[idx];

        for (int i = 0; i < cacNutLevel.Length; i++)
        {
            bool hienThi = i < soMan;
            cacNutLevel[i].SetActive(hienThi);
            if (!hienThi) continue;

            int soThuTu = i + 1;
            string tenLevel = prefix + soThuTu;
            string soHienThi = soThuTu.ToString();

            bool daUnlock = duocXem && KiemTraUnlock(tenLevel);
            bool daPassed = PlayerPrefs.GetInt(tenLevel + "_passed", 0) == 1;
            bool laMoiNhat = tenLevel == levelMoiNhat;

            Image img = cacNutLevel[i].GetComponent<Image>();
            Outline outline = cacNutLevel[i].GetComponent<Outline>();
            Button btn = cacNutLevel[i].GetComponent<Button>();
            TextMeshProUGUI txtSo = cacNutLevel[i]
                .transform.Find("Txt_So")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI txtIcon = cacNutLevel[i]
                .transform.Find("Txt_Icon")?.GetComponent<TextMeshProUGUI>();

            Color accent = mauAccentChuong[idx];
            Color nenActive = mauNenDangChoiChuong[idx];

            if (!daUnlock)
            {
                SetNut(img, outline, btn, txtSo, txtIcon,
                    mauNenLock, mauVienLock, mauChuLock, soHienThi, "", false);
            }
            else if (daPassed)
            {
                SetNut(img, outline, btn, txtSo, txtIcon,
                    mauNenDaMo, mauVienDaMo, mauChuDaMo, soHienThi, "*", true);
                GanOnClick(btn, tenLevel);
            }
            else if (laMoiNhat)
            {
                SetNut(img, outline, btn, txtSo, txtIcon,
                    nenActive, accent, accent, soHienThi, "", true);
                GanOnClick(btn, tenLevel);
            }
            else
            {
                // UNLOCK CHƯA CHƠI — không có icon
                SetNut(img, outline, btn, txtSo, txtIcon,
                    mauNenDaMo, mauVienDaMo, mauChuDaMo,
                    soHienThi, "", true);
                GanOnClick(btn, tenLevel);
            }
        }
    }

    // =============================================================
    // HELPERS
    // =============================================================
    void SetNut(Image img, Outline outline, Button btn,
        TextMeshProUGUI txtSo, TextMeshProUGUI txtIcon,
        Color mauNen, Color mauVien, Color mauChu,
        string so, string icon, bool tuongTac)
    {
        if (img != null) img.color = mauNen;
        if (outline != null) outline.effectColor = mauVien;
        btn.interactable = tuongTac;
        if (txtSo != null) { txtSo.text = so; txtSo.color = mauChu; }
        if (txtIcon != null) { txtIcon.text = icon; txtIcon.color = mauChu; }
    }

    void GanOnClick(Button btn, string tenLevel)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SceneManager.LoadScene(tenLevel));
    }

    void CapNhatMuiTen(int idx)
    {
        // Trái — về Special (idx 0) hoặc các chương trước
        if (nutTrai != null)
        {
            bool coTrai = idx > 0;
            nutTrai.interactable = coTrai;
            CanvasGroup cg = nutTrai.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = coTrai ? 1f : 0.25f;
        }

        // Phải — sang chương sau (chỉ nếu đã unlock)
        if (nutPhai != null)
        {
            bool coTiep = idx < tenCacChuong.Length - 1;
            bool tiepDaUnlock = coTiep && KiemTraDuocXem(idx + 1);
            nutPhai.interactable = coTiep;
            CanvasGroup cg = nutPhai.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = tiepDaUnlock ? 1f : 0.25f;
        }
    }

    // =============================================================
    // KIỂM TRA UNLOCK
    // =============================================================
    bool KiemTraDuocXem(int idx)
    {
        // Chapter 1 luôn xem được
        if (idx == 1) return true;

        // Special (idx 0) — unlock sau khi pass Ch1 màn 1-9
        if (idx == 0)
        {
            if (GameManager.instance != null)
                return GameManager.instance.LaChapterSpecialDaMo();
            return PlayerPrefs.GetInt("Chapter_Special_unlocked", 0) == 1;
        }

        // Chapter 2, 3, 4 — unlock sau khi pass màn 8 của chương trước
        // idx=2 → Ch2 → cần pass Level_1_8
        // idx=3 → Ch3 → cần pass Level_2_8
        // idx=4 → Ch4 → cần pass Level_3_8
        int chuongTruoc = idx - 1;
        string keyMan8 = $"Level_{chuongTruoc}_8_passed";
        return PlayerPrefs.GetInt(keyMan8, 0) == 1;
    }

    bool KiemTraUnlock(string tenLevel)
    {
        // Level_S_0 luôn unlock
        if (tenLevel == "Level_S_0") return true;

        // Level_S_* khác → unlock khi Special chapter mở
        if (tenLevel.StartsWith("Level_S_"))
            return PlayerPrefs.GetInt("Chapter_Special_unlocked", 0) == 1;

        if (GameManager.instance != null)
            return GameManager.instance.LaLevelDaUnlock(tenLevel);
        return PlayerPrefs.GetInt(tenLevel + "_unlocked", 0) == 1;
    }

    // =============================================================
    // HIỂN THỊ CHƯƠNG SPECIAL — scroll panel tạo runtime
    // =============================================================
    void HienThiChuongSpecial(bool duocXem)
    {
        if (specialScrollPanel != null) specialScrollPanel.SetActive(false);

        // Nếu có ScrollView pre-built từ builder → dùng scroll
        if (scrollViewSpecial != null && scrollContent != null)
        {
            HienThiSpecialScroll();
            return;
        }

        // Fallback: dùng cacNutLevel trực tiếp (≤ 8 level)
        HienThiSpecialGrid();
    }

    // Scroll approach — dùng khi scrollViewSpecial được assign từ builder
    void HienThiSpecialScroll()
    {
        // Ẩn grid thường — bao gồm container cha để tránh raycast blocking
        if (cacNutLevel != null)
        {
            foreach (var nut in cacNutLevel) if (nut != null) nut.SetActive(false);
            // Grid_Levels parent có Image raycastTarget=true → ẩn cả container
            if (cacNutLevel.Length > 0 && cacNutLevel[0] != null)
                cacNutLevel[0].transform.parent?.gameObject.SetActive(false);
        }

        scrollViewSpecial.gameObject.SetActive(true);

        // Xóa cells cũ
        foreach (var c in cacCellSpecial) if (c != null) c.SetActive(false);

        if (tenLevelSpecial == null) return;

        Color accent    = mauAccentChuong[0];
        Color nenActive = mauNenDangChoiChuong[0];

        // Tạo đủ cells
        while (cacCellSpecial.Count < tenLevelSpecial.Length)
            cacCellSpecial.Add(TaoSpecialCell(scrollContent));

        for (int i = 0; i < tenLevelSpecial.Length; i++)
        {
            GameObject cell = cacCellSpecial[i];
            cell.SetActive(true);
            cell.transform.SetParent(scrollContent, false);

            string tenLevel = tenLevelSpecial[i];
            string label    = tenLevel.Replace("Level_S_", "");
            bool daUnlock   = KiemTraUnlock(tenLevel);
            bool daPassed   = PlayerPrefs.GetInt(tenLevel + "_passed", 0) == 1;
            bool laMoiNhat  = tenLevel == levelMoiNhat;

            Image img   = cell.GetComponent<Image>();
            Outline ol  = cell.GetComponent<Outline>();
            Button btn  = cell.GetComponent<Button>();
            TextMeshProUGUI txtSo   = cell.transform.Find("Txt_So")
                                        ?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI txtIcon = cell.transform.Find("Txt_Icon")
                                        ?.GetComponent<TextMeshProUGUI>();

            if (!daUnlock)
                SetNut(img, ol, btn, txtSo, txtIcon, mauNenLock, mauVienLock, mauChuLock, label, "", false);
            else if (daPassed)
            { SetNut(img, ol, btn, txtSo, txtIcon, mauNenDaMo, mauVienDaMo, mauChuDaMo, label, "*", true); GanOnClick(btn, tenLevel); }
            else if (laMoiNhat)
            { SetNut(img, ol, btn, txtSo, txtIcon, nenActive, accent, accent, label, "", true); GanOnClick(btn, tenLevel); }
            else
            { SetNut(img, ol, btn, txtSo, txtIcon, mauNenDaMo, mauVienDaMo, mauChuDaMo, label, "", true); GanOnClick(btn, tenLevel); }
        }

        // Rebuild layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            scrollContent.GetComponent<RectTransform>());
    }

    // Grid approach — fallback khi chưa có scrollView (≤ cacNutLevel.Length)
    void HienThiSpecialGrid()
    {
        if (cacNutLevel == null || tenLevelSpecial == null) return;

        Color accent    = mauAccentChuong[0];
        Color nenActive = mauNenDangChoiChuong[0];

        for (int i = 0; i < cacNutLevel.Length; i++)
        {
            if (cacNutLevel[i] == null) continue;
            bool hienThi = i < tenLevelSpecial.Length;
            cacNutLevel[i].SetActive(hienThi);
            if (!hienThi) continue;

            string tenLevel = tenLevelSpecial[i];
            string label    = tenLevel.Replace("Level_S_", "");
            bool daUnlock   = KiemTraUnlock(tenLevel);
            bool daPassed   = PlayerPrefs.GetInt(tenLevel + "_passed", 0) == 1;
            bool laMoiNhat  = tenLevel == levelMoiNhat;

            Image img   = cacNutLevel[i].GetComponent<Image>();
            Outline ol  = cacNutLevel[i].GetComponent<Outline>();
            Button btn  = cacNutLevel[i].GetComponent<Button>();
            TextMeshProUGUI txtSo   = cacNutLevel[i].transform.Find("Txt_So")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI txtIcon = cacNutLevel[i].transform.Find("Txt_Icon")?.GetComponent<TextMeshProUGUI>();

            if (!daUnlock)
                SetNut(img, ol, btn, txtSo, txtIcon, mauNenLock, mauVienLock, mauChuLock, label, "", false);
            else if (daPassed)
            { SetNut(img, ol, btn, txtSo, txtIcon, mauNenDaMo, mauVienDaMo, mauChuDaMo, label, "*", true); GanOnClick(btn, tenLevel); }
            else if (laMoiNhat)
            { SetNut(img, ol, btn, txtSo, txtIcon, nenActive, accent, accent, label, "", true); GanOnClick(btn, tenLevel); }
            else
            { SetNut(img, ol, btn, txtSo, txtIcon, mauNenDaMo, mauVienDaMo, mauChuDaMo, label, "", true); GanOnClick(btn, tenLevel); }
        }
    }

    void AnchorVaoGrid(RectTransform rt, RectTransform gridRect)
    {
        // Đặt panel khớp đúng vị trí và kích thước với Grid_Levels
        rt.anchorMin = gridRect.anchorMin;
        rt.anchorMax = gridRect.anchorMax;
        rt.pivot     = gridRect.pivot;
        rt.anchoredPosition = gridRect.anchoredPosition;
        rt.sizeDelta = gridRect.sizeDelta;
    }

    void TaoSpecialScrollPanel()
    {
        // Tìm Canvas
        Canvas canvas = null;
        if (cacNutLevel != null && cacNutLevel.Length > 0 && cacNutLevel[0] != null)
            canvas = cacNutLevel[0].GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("[Special] Không tìm thấy Canvas!"); return; }

        // Lấy vị trí Grid_Levels làm tham chiếu
        RectTransform gridRect = cacNutLevel?[0]?.transform.parent
                                              ?.GetComponent<RectTransform>();

        // === PANEL CHÍNH (ScrollRect) ===
        specialScrollPanel = new GameObject("SpecialScrollPanel");
        specialScrollPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRT = specialScrollPanel.AddComponent<RectTransform>();
        if (gridRect != null)
            AnchorVaoGrid(panelRT, gridRect);
        else
        {
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(512f, 248f);
            panelRT.anchoredPosition = new Vector2(0f, -20f);
        }
        specialScrollPanel.AddComponent<Image>().color = Color.clear;
        ScrollRect sr = specialScrollPanel.AddComponent<ScrollRect>();
        sr.horizontal = false; sr.vertical = true;
        sr.scrollSensitivity = 30f;
        sr.movementType = ScrollRect.MovementType.Clamped;

        // === VIEWPORT ===
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(specialScrollPanel.transform, false);
        RectTransform vRT = viewport.AddComponent<RectTransform>();
        vRT.anchorMin = Vector2.zero; vRT.anchorMax = Vector2.one;
        vRT.offsetMin = Vector2.zero; vRT.offsetMax = Vector2.zero;
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = vRT;

        // === CONTENT — không dùng ContentSizeFitter, tính size thủ công ===
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform cRT = content.AddComponent<RectTransform>();
        // Anchor top-stretch, pivot top-center
        cRT.anchorMin = new Vector2(0f, 1f);
        cRT.anchorMax = new Vector2(1f, 1f);
        cRT.pivot = new Vector2(0.5f, 1f);
        cRT.offsetMin = Vector2.zero;
        cRT.offsetMax = Vector2.zero;
        cRT.sizeDelta = Vector2.zero; // width = stretch, height = 0 ban đầu
        content.AddComponent<Image>().color = Color.clear;
        sr.content = cRT;

        specialGridContent = content.transform;
        specialScrollPanel.SetActive(false);
    }

    GameObject TaoSpecialCell(Transform parent)
    {
        GameObject cell = new GameObject("Cell");
        cell.transform.SetParent(parent, false);
        RectTransform rt = cell.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(116f, 116f);
        cell.AddComponent<Image>().color = new Color(0.11f, 0.11f, 0.11f);
        cell.AddComponent<Button>();
        Outline ol = cell.AddComponent<Outline>();
        ol.effectColor    = new Color(0.75f, 0.52f, 0.99f, 0.5f);
        ol.effectDistance = new Vector2(1f, -1f);

        // Txt_So
        GameObject goSo = new GameObject("Txt_So");
        goSo.transform.SetParent(cell.transform, false);
        RectTransform rtSo = goSo.AddComponent<RectTransform>();
        rtSo.anchorMin = Vector2.zero; rtSo.anchorMax = Vector2.one;
        rtSo.offsetMin = Vector2.zero; rtSo.offsetMax = Vector2.zero;
        rtSo.anchoredPosition = new Vector2(0f, 10f);
        var tSo = goSo.AddComponent<TextMeshProUGUI>();
        tSo.fontSize = 32f;
        tSo.alignment = TextAlignmentOptions.Center;
        tSo.fontStyle = FontStyles.Bold;

        // Txt_Icon
        GameObject goIcon = new GameObject("Txt_Icon");
        goIcon.transform.SetParent(cell.transform, false);
        RectTransform rtIcon = goIcon.AddComponent<RectTransform>();
        rtIcon.anchorMin = Vector2.zero; rtIcon.anchorMax = Vector2.one;
        rtIcon.offsetMin = Vector2.zero; rtIcon.offsetMax = Vector2.zero;
        rtIcon.anchoredPosition = new Vector2(0f, -18f);
        var tIcon = goIcon.AddComponent<TextMeshProUGUI>();
        tIcon.fontSize = 12f;
        tIcon.alignment = TextAlignmentOptions.Center;

        return cell;
    }

    // =============================================================
    // NÚT MŨI TÊN
    // =============================================================
    void WireButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        if (btn.onClick.GetPersistentEventCount() == 0)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    public void NutTrai()
    {
        if (chuongHienTai > 0)
        {
            chuongHienTai--;
            HienThiChuong(chuongHienTai);
        }
    }

    public void NutPhai()
    {
        if (chuongHienTai < tenCacChuong.Length - 1)
        {
            chuongHienTai++;
            HienThiChuong(chuongHienTai);
        }
    }

    public void NutBack()
    {
        SceneManager.LoadScene(tenSceneMainMenu);
    }
}