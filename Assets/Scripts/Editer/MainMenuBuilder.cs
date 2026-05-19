using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Chạy script này 1 lần để tự động tạo toàn bộ UI Main Menu
// Sau khi chạy xong, xóa script này khỏi project
#if UNITY_EDITOR
using UnityEditor;

public class MainMenuBuilder : MonoBehaviour
{
    [MenuItem("NoTrollHere/Build Main Menu UI")]
    static void BuildMainMenuUI()
    {
        // === TÌM CANVAS ===
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas! Tạo Canvas trước.");
            return;
        }
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Thêm CanvasGroup nếu chưa có
        if (canvas.GetComponent<CanvasGroup>() == null)
            canvas.gameObject.AddComponent<CanvasGroup>();

        // === BACKGROUND ===
        GameObject bg = TaoPanel("Background", canvas.transform);
        CaiDatStretchFull(bg.GetComponent<RectTransform>());
        bg.GetComponent<Image>().color = HexToColor("1A1A1A");
        bg.transform.SetSiblingIndex(0);

        // === CHALK PARTICLE SYSTEM ===
        GameObject particles = new GameObject("ChalkParticleSystem");
        particles.transform.SetParent(canvas.transform, false);
        RectTransform pRect = particles.AddComponent<RectTransform>();
        CaiDatStretchFull(pRect);
        particles.AddComponent<ChalkParticle>();

        // === PANEL CENTER ===
        GameObject panelCenter = new GameObject("Panel_Center");
        panelCenter.transform.SetParent(canvas.transform, false);
        RectTransform pcRect = panelCenter.AddComponent<RectTransform>();
        // Anchor center
        pcRect.anchorMin = new Vector2(0.5f, 0.5f);
        pcRect.anchorMax = new Vector2(0.5f, 0.5f);
        pcRect.pivot = new Vector2(0.5f, 0.5f);
        pcRect.sizeDelta = new Vector2(320f, 420f);
        pcRect.anchoredPosition = Vector2.zero;
        // Vertical Layout
        VerticalLayoutGroup vlg = panelCenter.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 10f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        panelCenter.AddComponent<Image>().color = Color.clear;

        // --- Pretitle ---
        TaoTMP("Txt_Pretitle", panelCenter.transform,
            "a troll platformer", 13f, "444444", 30f, TextAlignmentOptions.Center);

        // --- Title ---
        GameObject titleGO = TaoTMP("Txt_Title", panelCenter.transform,
            "No<color=#F5C842>Troll</color>Here", 52f, "E8E8D0", 65f, TextAlignmentOptions.Center);
        titleGO.GetComponent<TextMeshProUGUI>().richText = true;
        titleGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // --- Separator ---
        GameObject sep = new GameObject("Img_Separator");
        sep.transform.SetParent(panelCenter.transform, false);
        RectTransform sepRect = sep.AddComponent<RectTransform>();
        sepRect.sizeDelta = new Vector2(60f, 2f);
        Image sepImg = sep.AddComponent<Image>();
        sepImg.color = HexToColor("333333");
        LayoutElement sepLE = sep.AddComponent<LayoutElement>();
        sepLE.preferredHeight = 2f;
        sepLE.preferredWidth = 60f;
        sepLE.flexibleWidth = 0f;

        // --- Tagline ---
        TaoTMP("Txt_Tagline", panelCenter.transform,
            "there are definitely no traps here", 12f, "444444", 25f, TextAlignmentOptions.Center);

        // --- Spacer ---
        TaoSpacer(panelCenter.transform, 20f);

        // --- Btn Play ---
        TaoNut("Btn_Play", panelCenter.transform,
            "\u25B6  Play", "F5C842", "1A1A1A", 48f, true);

        // --- Btn Level Select ---
        TaoNut("Btn_LevelSelect", panelCenter.transform,
            "Level Select", "1A1A1A", "E8E8D0", 44f, false);

        // --- Btn Settings ---
        TaoNut("Btn_Settings", panelCenter.transform,
            "Settings", "1A1A1A", "888888", 40f, false, true);

        // === FOOTER ===
        GameObject footer = TaoTMP("Txt_Footer", canvas.transform,
            "DEATHS THIS SESSION : 0", 11f, "2E2E2E", 25f, TextAlignmentOptions.Center);
        RectTransform footerRect = footer.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0f, 0f);
        footerRect.anchorMax = new Vector2(1f, 0f);
        footerRect.pivot = new Vector2(0.5f, 0f);
        footerRect.anchoredPosition = new Vector2(0f, 20f);
        footerRect.sizeDelta = new Vector2(0f, 25f);

        Debug.Log("✅ Main Menu UI đã được tạo thành công!");
        EditorUtility.SetDirty(canvas.gameObject);
    }

    // === HELPER: Tạo Panel với Image ===
    static GameObject TaoPanel(string ten, Transform parent)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>();
        return go;
    }

    // === HELPER: Tạo TextMeshPro ===
    static GameObject TaoTMP(string ten, Transform parent,
        string text, float size, string hex, float height, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300f, height);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = HexToColor(hex);
        tmp.alignment = align;
        tmp.richText = true;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        return go;
    }

    // === HELPER: Tạo nút ===
    static GameObject TaoNut(string ten, Transform parent,
        string label, string bgHex, string textHex,
        float height, bool solid, bool ghost = false)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200f, height);

        Image img = go.AddComponent<Image>();
        if (solid)
            img.color = HexToColor(bgHex);
        else
        {
            img.color = Color.clear;
            // Thêm outline
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = ghost ? HexToColor("2A2A2A") : HexToColor("3A3A3A");
            outline.effectDistance = new Vector2(1f, -1f);
        }

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
        cb.pressedColor = new Color(1f, 1f, 1f, 0.2f);
        btn.colors = cb;

        // Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        CaiDatStretchFull(textRect);
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = ghost ? 13f : 15f;
        tmp.color = HexToColor(textHex);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = solid ? FontStyles.Bold : FontStyles.Normal;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.preferredWidth = 200f;
        le.flexibleWidth = 0f;

        return go;
    }

    // === HELPER: Spacer ===
    static void TaoSpacer(Transform parent, float height)
    {
        GameObject go = new GameObject("Spacer");
        go.transform.SetParent(parent, false);
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
    }

    // === HELPER: Stretch full canvas ===
    static void CaiDatStretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // === HELPER: Hex to Color ===
    static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color c))
            return c;
        return Color.white;
    }
}
#endif