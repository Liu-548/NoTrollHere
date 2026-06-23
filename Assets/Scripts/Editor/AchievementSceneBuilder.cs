using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// NoTrollHere/Build Achievement UI
/// Chạy từ scene Achievements (đã có Canvas).
/// Tạo toàn bộ UI cho màn xem thành tựu.
/// </summary>
public class AchievementSceneBuilder : MonoBehaviour
{
    [MenuItem("NoTrollHere/Build Achievement UI")]
    static void Build()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[AchievementSceneBuilder] Không tìm thấy Canvas trong scene!");
            return;
        }

        // Cài CanvasScaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight  = 0.5f;
        }
        if (canvas.GetComponent<CanvasGroup>() == null)
            canvas.gameObject.AddComponent<CanvasGroup>();

        // === BACKGROUND ===
        GameObject bg = TaoImage("Background", canvas.transform,
            new Color(0.102f, 0.102f, 0.102f));
        StretchFull(bg.GetComponent<RectTransform>());
        bg.transform.SetSiblingIndex(0);

        // === CHALK PARTICLE ===
        GameObject particles = new GameObject("ChalkParticleSystem");
        particles.transform.SetParent(canvas.transform, false);
        StretchFull(particles.AddComponent<RectTransform>());
        particles.AddComponent<ChalkParticle>();

        // === TIÊU ĐỀ ===
        GameObject txtTitle = TaoTMP("Txt_Title", canvas.transform,
            "— THÀNH TỰU —", 22f, new Color(0.96f, 0.78f, 0.26f));
        RectTransform rcTitle = txtTitle.GetComponent<RectTransform>();
        rcTitle.anchorMin        = new Vector2(0.5f, 1f);
        rcTitle.anchorMax        = new Vector2(0.5f, 1f);
        rcTitle.pivot            = new Vector2(0.5f, 1f);
        rcTitle.sizeDelta        = new Vector2(600f, 50f);
        rcTitle.anchoredPosition = new Vector2(0f, -40f);

        // === TỔNG KẾT X/Y ===
        GameObject txtTongKet = TaoTMP("Txt_TongKet", canvas.transform,
            "0 / 0 Thành tựu", 14f, new Color(0.45f, 0.45f, 0.45f));
        RectTransform rcTK = txtTongKet.GetComponent<RectTransform>();
        rcTK.anchorMin        = new Vector2(0.5f, 1f);
        rcTK.anchorMax        = new Vector2(0.5f, 1f);
        rcTK.pivot            = new Vector2(0.5f, 1f);
        rcTK.sizeDelta        = new Vector2(400f, 30f);
        rcTK.anchoredPosition = new Vector2(0f, -96f);

        // === SCROLL VIEW ===
        GameObject scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(canvas.transform, false);
        RectTransform rcScroll = scrollGO.AddComponent<RectTransform>();
        rcScroll.anchorMin = new Vector2(0.1f, 0f);
        rcScroll.anchorMax = new Vector2(0.9f, 1f);
        rcScroll.offsetMin = new Vector2(0f, 80f);
        rcScroll.offsetMax = new Vector2(0f, -140f);
        scrollGO.AddComponent<Image>().color = Color.clear;
        ScrollRect sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal        = false;
        sr.vertical          = true;
        sr.scrollSensitivity = 30f;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollGO.transform, false);
        RectTransform rcVP = viewport.AddComponent<RectTransform>();
        StretchFull(rcVP);
        // RectMask2D thay Mask: không dùng stencil → hoạt động đúng với mọi nền, không bị cull trong Unity 6
        viewport.AddComponent<RectMask2D>();
        sr.viewport = rcVP;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform rcContent = content.AddComponent<RectTransform>();
        rcContent.anchorMin = new Vector2(0f, 1f);
        rcContent.anchorMax = new Vector2(1f, 1f);
        rcContent.pivot     = new Vector2(0.5f, 1f);
        rcContent.sizeDelta = new Vector2(0f, 0f);
        content.AddComponent<Image>().color = Color.clear;
        // Không dùng VLG/CSF — AchievementSceneManager tự tính vị trí và chiều cao
        sr.content = rcContent;

        // === NÚT BACK ===
        GameObject btnBack = TaoNut("Btn_Back", canvas.transform, "< Menu", 130f, 40f);
        RectTransform rcBack = btnBack.GetComponent<RectTransform>();
        rcBack.anchorMin        = new Vector2(0f, 0f);
        rcBack.anchorMax        = new Vector2(0f, 0f);
        rcBack.pivot            = new Vector2(0f, 0f);
        rcBack.anchoredPosition = new Vector2(30f, 30f);

        // === ACHIEVEMENT SCENE MANAGER ===
        GameObject mgrGO = new GameObject("AchievementSceneManager");
        mgrGO.transform.SetParent(canvas.transform.parent, false);
        AchievementSceneManager asm = mgrGO.AddComponent<AchievementSceneManager>();
        asm.contentParent = rcContent;
        asm.txtTongKet    = txtTongKet.GetComponent<TextMeshProUGUI>();
        asm.nutBack       = btnBack.GetComponent<Button>();

        btnBack.GetComponent<Button>().onClick.AddListener(asm.NutBack);

        Debug.Log("✅ Achievement UI đã tạo xong!");
        EditorUtility.SetDirty(canvas.gameObject);
    }

    // ─── Helpers ───────────────────────────────────────────────────

    static GameObject TaoImage(string ten, Transform parent, Color mau)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = mau;
        return go;
    }

    static GameObject TaoTMP(string ten, Transform parent, string text,
        float size, Color mau)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = mau;
        tmp.alignment = TextAlignmentOptions.Center;
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
        StretchFull(txt.AddComponent<RectTransform>());
        TextMeshProUGUI tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 16f;
        tmp.color     = new Color(0.91f, 0.91f, 0.82f);
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
#endif
