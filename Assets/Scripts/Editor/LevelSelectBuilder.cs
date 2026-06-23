using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class LevelSelectBuilder : MonoBehaviour
{
    [MenuItem("NoTrollHere/Build Level Select UI")]
    static void BuildLevelSelectUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        if (canvas.GetComponent<CanvasGroup>() == null)
            canvas.gameObject.AddComponent<CanvasGroup>();

        // === CANVAS SCALER ===
        UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // === BACKGROUND ===
        GameObject bg = TaoImage("Background", canvas.transform,
            new Color(0.102f, 0.102f, 0.102f));
        CaiDatStretchFull(bg.GetComponent<RectTransform>());
        bg.transform.SetSiblingIndex(0);

        // === CHALK PARTICLE (giống MainMenu) ===
        GameObject particles = new GameObject("ChalkParticleSystem");
        particles.transform.SetParent(canvas.transform, false);
        RectTransform pRect = particles.AddComponent<RectTransform>();
        CaiDatStretchFull(pRect);
        particles.AddComponent<ChalkParticle>();

        // === TÊN CHƯƠNG ===
        GameObject txtChuong = TaoTMP("Txt_TenChuong", canvas.transform,
            "— CHAPTER 1 —", 24f, new Color(0.96f, 0.78f, 0.26f));
        RectTransform rcChuong = txtChuong.GetComponent<RectTransform>();
        rcChuong.anchorMin = new Vector2(0.5f, 1f);
        rcChuong.anchorMax = new Vector2(0.5f, 1f);
        rcChuong.pivot = new Vector2(0.5f, 1f);
        rcChuong.sizeDelta = new Vector2(500f, 55f);
        rcChuong.anchoredPosition = new Vector2(0f, -50f);

        // === MÔ TẢ CHƯƠNG ===
        GameObject txtMota = TaoTMP("Txt_MoTaChuong", canvas.transform,
            "choose your suffering", 14f, new Color(0.27f, 0.27f, 0.27f));
        RectTransform rcMota = txtMota.GetComponent<RectTransform>();
        rcMota.anchorMin = new Vector2(0.5f, 1f);
        rcMota.anchorMax = new Vector2(0.5f, 1f);
        rcMota.pivot = new Vector2(0.5f, 1f);
        rcMota.sizeDelta = new Vector2(500f, 35f);
        rcMota.anchoredPosition = new Vector2(0f, -108f);

        // === MŨI TÊN TRÁI ===
        GameObject btnTrai = TaoNut("Btn_Trai", canvas.transform, "<", 60f, 60f);
        RectTransform rcTrai = btnTrai.GetComponent<RectTransform>();
        rcTrai.anchorMin = new Vector2(0f, 0.5f);
        rcTrai.anchorMax = new Vector2(0f, 0.5f);
        rcTrai.pivot = new Vector2(0f, 0.5f);
        rcTrai.anchoredPosition = new Vector2(20f, 0f);
        btnTrai.AddComponent<CanvasGroup>();

        // === MŨI TÊN PHẢI ===
        GameObject btnPhai = TaoNut("Btn_Phai", canvas.transform, ">", 60f, 60f);
        RectTransform rcPhai = btnPhai.GetComponent<RectTransform>();
        rcPhai.anchorMin = new Vector2(1f, 0.5f);
        rcPhai.anchorMax = new Vector2(1f, 0.5f);
        rcPhai.pivot = new Vector2(1f, 0.5f);
        rcPhai.anchoredPosition = new Vector2(-20f, 0f);
        btnPhai.AddComponent<CanvasGroup>();

        // === SCROLL VIEW CHO CHƯƠNG SPECIAL ===
        // Cùng vị trí/kích thước với Grid_Levels để overlap đúng chỗ
        GameObject scrollView = new GameObject("ScrollView_Special");
        scrollView.transform.SetParent(canvas.transform, false);
        RectTransform rcScroll = scrollView.AddComponent<RectTransform>();
        rcScroll.anchorMin        = new Vector2(0.5f, 0.5f);
        rcScroll.anchorMax        = new Vector2(0.5f, 0.5f);
        rcScroll.pivot            = new Vector2(0.5f, 0.5f);
        rcScroll.sizeDelta        = new Vector2(512f, 248f);
        rcScroll.anchoredPosition = new Vector2(0f, -20f);
        scrollView.AddComponent<Image>().color = Color.clear;
        ScrollRect sr = scrollView.AddComponent<ScrollRect>();
        sr.horizontal       = false;
        sr.vertical         = true;
        sr.scrollSensitivity = 30f;
        sr.movementType     = ScrollRect.MovementType.Clamped;

        // Viewport — phủ toàn bộ scrollView
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform rcViewport = viewport.AddComponent<RectTransform>();
        CaiDatStretchFull(rcViewport);
        Image vpImg = viewport.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0.01f); // gần trong suốt nhưng đủ để Mask hoạt động
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = rcViewport;

        // Content — anchor top, tự giãn theo số cell
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform rcContent = content.AddComponent<RectTransform>();
        rcContent.anchorMin = new Vector2(0f, 1f);
        rcContent.anchorMax = new Vector2(1f, 1f);
        rcContent.pivot     = new Vector2(0.5f, 1f);
        rcContent.offsetMin = Vector2.zero;
        rcContent.offsetMax = Vector2.zero;
        rcContent.sizeDelta = Vector2.zero;
        content.AddComponent<Image>().color = Color.clear;
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        GridLayoutGroup glgS = content.AddComponent<GridLayoutGroup>();
        glgS.cellSize        = new Vector2(116f, 116f);
        glgS.spacing         = new Vector2(16f, 16f);
        glgS.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glgS.constraintCount = 4;
        glgS.childAlignment  = TextAnchor.UpperCenter;
        glgS.padding         = new RectOffset(8, 8, 8, 8);
        sr.content = rcContent;

        scrollView.SetActive(false); // ẩn mặc định

        // === GRID LEVELS — 4 cột × 2 hàng cho ch1-4 ===
        // 4 × 116 + 3 × 16 = 512  |  2 × 116 + 1 × 16 = 248
        GameObject grid = new GameObject("Grid_Levels");
        grid.transform.SetParent(canvas.transform, false);
        RectTransform rcGrid = grid.AddComponent<RectTransform>();
        rcGrid.anchorMin = new Vector2(0.5f, 0.5f);
        rcGrid.anchorMax = new Vector2(0.5f, 0.5f);
        rcGrid.pivot = new Vector2(0.5f, 0.5f);
        rcGrid.sizeDelta = new Vector2(512f, 248f);
        rcGrid.anchoredPosition = new Vector2(0f, -20f);
        grid.AddComponent<Image>().color = Color.clear;

        GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(116f, 116f);
        glg.spacing = new Vector2(16f, 16f);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 4;
        glg.childAlignment = TextAnchor.MiddleCenter;

        // === 8 NÚT LEVEL ===
        for (int i = 1; i <= 8; i++)
            TaoLevelCell("LevelCell_" + i, grid.transform, i);

        // === NÚT BACK ===
        GameObject btnBack = TaoNut("Btn_Back", canvas.transform, "< Menu", 130f, 40f);
        RectTransform rcBack = btnBack.GetComponent<RectTransform>();
        rcBack.anchorMin = new Vector2(0f, 0f);
        rcBack.anchorMax = new Vector2(0f, 0f);
        rcBack.pivot = new Vector2(0f, 0f);
        rcBack.anchoredPosition = new Vector2(30f, 30f);

        // === MANAGER ===
        GameObject manager = new GameObject("LevelSelectManager");
        manager.transform.SetParent(canvas.transform.parent, false);
        LevelSelectManager lsm = manager.AddComponent<LevelSelectManager>();

        lsm.txtTenChuong = txtChuong.GetComponent<TextMeshProUGUI>();
        lsm.txtMoTaChuong = txtMota.GetComponent<TextMeshProUGUI>();
        lsm.nutTrai = btnTrai.GetComponent<Button>();
        lsm.nutPhai = btnPhai.GetComponent<Button>();

        lsm.cacNutLevel = new GameObject[8];
        for (int i = 0; i < 8; i++)
            lsm.cacNutLevel[i] = grid.transform.GetChild(i).gameObject;

        lsm.scrollViewSpecial = scrollView.GetComponent<ScrollRect>();
        lsm.scrollContent     = content.transform;

        btnTrai.GetComponent<Button>().onClick.AddListener(lsm.NutTrai);
        btnPhai.GetComponent<Button>().onClick.AddListener(lsm.NutPhai);
        btnBack.GetComponent<Button>().onClick.AddListener(lsm.NutBack);

        Debug.Log("✅ Level Select UI da duoc tao!");
        EditorUtility.SetDirty(canvas.gameObject);
    }

    // === HELPER: Level Cell ===
    static GameObject TaoLevelCell(string ten, Transform parent, int soThuTu)
    {
        GameObject cell = new GameObject(ten);
        cell.transform.SetParent(parent, false);
        cell.AddComponent<Image>().color = new Color(0.11f, 0.11f, 0.11f);
        cell.AddComponent<Button>();

        Outline outline = cell.AddComponent<Outline>();
        outline.effectColor = new Color(0.13f, 0.13f, 0.13f);
        outline.effectDistance = new Vector2(1f, -1f);

        // Số level
        GameObject txtSo = new GameObject("Txt_So");
        txtSo.transform.SetParent(cell.transform, false);
        RectTransform rtSo = txtSo.AddComponent<RectTransform>();
        CaiDatStretchFull(rtSo);
        rtSo.anchoredPosition = new Vector2(0f, 10f);
        TextMeshProUGUI tmpSo = txtSo.AddComponent<TextMeshProUGUI>();
        tmpSo.text = soThuTu.ToString();
        tmpSo.fontSize = 32f;
        tmpSo.color = new Color(0.16f, 0.16f, 0.16f);
        tmpSo.alignment = TextAlignmentOptions.Center;
        tmpSo.fontStyle = FontStyles.Bold;

        // Icon
        GameObject txtIcon = new GameObject("Txt_Icon");
        txtIcon.transform.SetParent(cell.transform, false);
        RectTransform rtIcon = txtIcon.AddComponent<RectTransform>();
        CaiDatStretchFull(rtIcon);
        rtIcon.anchoredPosition = new Vector2(0f, -18f);
        TextMeshProUGUI tmpIcon = txtIcon.AddComponent<TextMeshProUGUI>();
        tmpIcon.text = "X";
        tmpIcon.fontSize = 12f;
        tmpIcon.color = new Color(0.16f, 0.16f, 0.16f);
        tmpIcon.alignment = TextAlignmentOptions.Center;

        return cell;
    }

    // === HELPER: Image ===
    static GameObject TaoImage(string ten, Transform parent, Color mau)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = mau;
        return go;
    }

    // === HELPER: TMP ===
    static GameObject TaoTMP(string ten, Transform parent,
        string text, float size, Color mau)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = mau;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    // === HELPER: Button ===
    static GameObject TaoNut(string ten, Transform parent,
        string label, float w, float h)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(w, h);
        go.AddComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f);
        go.AddComponent<Button>();

        GameObject txt = new GameObject("Text");
        txt.transform.SetParent(go.transform, false);
        RectTransform rtxt = txt.AddComponent<RectTransform>();
        CaiDatStretchFull(rtxt);
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20f;
        tmp.color = new Color(0.91f, 0.91f, 0.82f);
        tmp.alignment = TextAlignmentOptions.Center;

        return go;
    }

    // === HELPER: Stretch full ===
    static void CaiDatStretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}