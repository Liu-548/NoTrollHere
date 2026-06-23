using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class SkinSelectBuilder : MonoBehaviour
{
    [MenuItem("NoTrollHere/Build Skin Select UI")]
    static void Build()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (canvas.GetComponent<CanvasGroup>() == null)
            canvas.gameObject.AddComponent<CanvasGroup>();

        // === BACKGROUND ===
        GameObject bg = TaoImage("Background", canvas.transform,
            new Color(0.102f, 0.102f, 0.102f));
        CaiDatStretchFull(bg.GetComponent<RectTransform>());
        bg.transform.SetSiblingIndex(0);

        // === CHALK PARTICLE ===
        GameObject particles = new GameObject("ChalkParticleSystem");
        particles.transform.SetParent(canvas.transform, false);
        CaiDatStretchFull(particles.AddComponent<RectTransform>());
        particles.AddComponent<ChalkParticle>();

        // === TIÊU ĐỀ ===
        GameObject txtTitle = TaoTMP("Txt_Title", canvas.transform,
            "— CHOOSE YOUR SKIN —", 22f, new Color(0.96f, 0.78f, 0.26f));
        RectTransform rcTitle = txtTitle.GetComponent<RectTransform>();
        rcTitle.anchorMin = new Vector2(0.5f, 1f);
        rcTitle.anchorMax = new Vector2(0.5f, 1f);
        rcTitle.pivot = new Vector2(0.5f, 1f);
        rcTitle.sizeDelta = new Vector2(600f, 50f);
        rcTitle.anchoredPosition = new Vector2(0f, -40f);

        // === PREVIEW SKIN (hình lớn giữa màn) ===
        GameObject preview = new GameObject("Preview_Skin");
        preview.transform.SetParent(canvas.transform, false);
        RectTransform rcPreview = preview.AddComponent<RectTransform>();
        rcPreview.anchorMin = new Vector2(0.5f, 0.5f);
        rcPreview.anchorMax = new Vector2(0.5f, 0.5f);
        rcPreview.pivot = new Vector2(0.5f, 0.5f);
        rcPreview.sizeDelta = new Vector2(80f, 160f);
        rcPreview.anchoredPosition = new Vector2(0f, 30f);
        Image previewImg = preview.AddComponent<Image>();
        previewImg.color = new Color(0.91f, 0.91f, 0.82f);
        // Viền vàng
        Outline previewOutline = preview.AddComponent<Outline>();
        previewOutline.effectColor = new Color(0.96f, 0.78f, 0.26f);
        previewOutline.effectDistance = new Vector2(3f, -3f);

        // === TÊN SKIN ===
        GameObject txtTenSkin = TaoTMP("Txt_TenSkin", canvas.transform,
            "Default", 18f, new Color(0.91f, 0.91f, 0.82f));
        RectTransform rcTen = txtTenSkin.GetComponent<RectTransform>();
        rcTen.anchorMin = new Vector2(0.5f, 0.5f);
        rcTen.anchorMax = new Vector2(0.5f, 0.5f);
        rcTen.pivot = new Vector2(0.5f, 0.5f);
        rcTen.sizeDelta = new Vector2(400f, 35f);
        rcTen.anchoredPosition = new Vector2(0f, -70f);

        // === MÔ TẢ SKIN ===
        GameObject txtMoTa = TaoTMP("Txt_MoTa", canvas.transform,
            "Nhân vật chalk nguyên bản", 13f,
            new Color(0.4f, 0.4f, 0.4f));
        RectTransform rcMoTa = txtMoTa.GetComponent<RectTransform>();
        rcMoTa.anchorMin = new Vector2(0.5f, 0.5f);
        rcMoTa.anchorMax = new Vector2(0.5f, 0.5f);
        rcMoTa.pivot = new Vector2(0.5f, 0.5f);
        rcMoTa.sizeDelta = new Vector2(500f, 30f);
        rcMoTa.anchoredPosition = new Vector2(0f, -102f);

        // === ĐIỀU KIỆN MỞ ===
        GameObject txtDieuKien = TaoTMP("Txt_DieuKien", canvas.transform,
            "", 11f, new Color(0.3f, 0.3f, 0.3f));
        RectTransform rcDK = txtDieuKien.GetComponent<RectTransform>();
        rcDK.anchorMin = new Vector2(0.5f, 0.5f);
        rcDK.anchorMax = new Vector2(0.5f, 0.5f);
        rcDK.pivot = new Vector2(0.5f, 0.5f);
        rcDK.sizeDelta = new Vector2(500f, 25f);
        rcDK.anchoredPosition = new Vector2(0f, -128f);

        // === NÚT TRÁI PHẢI ===
        GameObject btnTrai = TaoNut("Btn_Trai", canvas.transform, "<", 55f, 55f);
        RectTransform rcTrai = btnTrai.GetComponent<RectTransform>();
        rcTrai.anchorMin = new Vector2(0.5f, 0.5f);
        rcTrai.anchorMax = new Vector2(0.5f, 0.5f);
        rcTrai.pivot = new Vector2(0.5f, 0.5f);
        rcTrai.anchoredPosition = new Vector2(-160f, 30f);
        btnTrai.AddComponent<CanvasGroup>();

        GameObject btnPhai = TaoNut("Btn_Phai", canvas.transform, ">", 55f, 55f);
        RectTransform rcPhai = btnPhai.GetComponent<RectTransform>();
        rcPhai.anchorMin = new Vector2(0.5f, 0.5f);
        rcPhai.anchorMax = new Vector2(0.5f, 0.5f);
        rcPhai.pivot = new Vector2(0.5f, 0.5f);
        rcPhai.anchoredPosition = new Vector2(160f, 30f);
        btnPhai.AddComponent<CanvasGroup>();

        // === NÚT CHỌN ===
        GameObject btnChon = TaoNut("Btn_Chon", canvas.transform,
            "EQUIP", 200f, 48f);
        RectTransform rcChon = btnChon.GetComponent<RectTransform>();
        rcChon.anchorMin = new Vector2(0.5f, 0f);
        rcChon.anchorMax = new Vector2(0.5f, 0f);
        rcChon.pivot = new Vector2(0.5f, 0f);
        rcChon.anchoredPosition = new Vector2(0f, 80f);
        // Màu vàng
        btnChon.GetComponent<Image>().color = new Color(0.96f, 0.78f, 0.26f);
        btnChon.GetComponentInChildren<TextMeshProUGUI>().color
            = new Color(0.1f, 0.1f, 0.1f);
        btnChon.GetComponentInChildren<TextMeshProUGUI>().fontStyle
            = FontStyles.Bold;

        // === NÚT BACK ===
        GameObject btnBack = TaoNut("Btn_Back", canvas.transform,
            "< Menu", 130f, 40f);
        RectTransform rcBack = btnBack.GetComponent<RectTransform>();
        rcBack.anchorMin = new Vector2(0f, 0f);
        rcBack.anchorMax = new Vector2(0f, 0f);
        rcBack.pivot = new Vector2(0f, 0f);
        rcBack.anchoredPosition = new Vector2(30f, 30f);

        // === DOT INDICATORS (chấm tròn hiện skin nào đang chọn) ===
        GameObject dots = new GameObject("Dots_Indicator");
        dots.transform.SetParent(canvas.transform, false);
        RectTransform rcDots = dots.AddComponent<RectTransform>();
        rcDots.anchorMin = new Vector2(0.5f, 0.5f);
        rcDots.anchorMax = new Vector2(0.5f, 0.5f);
        rcDots.pivot = new Vector2(0.5f, 0.5f);
        rcDots.sizeDelta = new Vector2(200f, 12f);
        rcDots.anchoredPosition = new Vector2(0f, -155f);
        HorizontalLayoutGroup hlg = dots.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 8f;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        dots.AddComponent<Image>().color = Color.clear;

        // Tạo 5 chấm (cửa sổ cuộn — hỗ trợ bất kỳ số lượng skin)
        GameObject[] cacDot = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject dot = new GameObject("Dot_" + i);
            dot.transform.SetParent(dots.transform, false);
            RectTransform dotRect = dot.AddComponent<RectTransform>();
            dotRect.sizeDelta = new Vector2(8f, 8f);
            Image dotImg = dot.AddComponent<Image>();
            dotImg.color = i == 0
                ? new Color(0.96f, 0.78f, 0.26f)  // active
                : new Color(0.2f, 0.2f, 0.2f);     // inactive
            cacDot[i] = dot;
        }

        // === SKIN SELECT MANAGER ===
        GameObject smGO = new GameObject("SkinSelectManager");
        smGO.transform.SetParent(canvas.transform.parent, false);
        SkinSelectManager ssm = smGO.AddComponent<SkinSelectManager>();

        ssm.imgPreview = previewImg;
        ssm.txtTenSkin = txtTenSkin.GetComponent<TextMeshProUGUI>();
        ssm.txtMoTa = txtMoTa.GetComponent<TextMeshProUGUI>();
        ssm.txtDieuKien = txtDieuKien.GetComponent<TextMeshProUGUI>();
        ssm.nutTrai = btnTrai.GetComponent<Button>();
        ssm.nutPhai = btnPhai.GetComponent<Button>();
        ssm.nutChon = btnChon.GetComponent<Button>();
        ssm.nutBack = btnBack.GetComponent<Button>();
        ssm.cacDot = new Image[5];
        for (int i = 0; i < 5; i++)
            ssm.cacDot[i] = cacDot[i].GetComponent<Image>();

        btnTrai.GetComponent<Button>().onClick.AddListener(ssm.NutTrai);
        btnPhai.GetComponent<Button>().onClick.AddListener(ssm.NutPhai);
        btnChon.GetComponent<Button>().onClick.AddListener(ssm.NutChon);
        btnBack.GetComponent<Button>().onClick.AddListener(ssm.NutBack);

        Debug.Log("✅ Skin Select UI đã tạo!");
        EditorUtility.SetDirty(canvas.gameObject);
    }

    static GameObject TaoImage(string ten, Transform parent, Color mau)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = mau;
        return go;
    }

    static GameObject TaoTMP(string ten, Transform parent,
        string text, float size, Color mau)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.color = mau; tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

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
        CaiDatStretchFull(txt.AddComponent<RectTransform>());
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 18f;
        tmp.color = new Color(0.91f, 0.91f, 0.82f);
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    static void CaiDatStretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}