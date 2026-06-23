using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class AchievementPopupBuilder : MonoBehaviour
{
    [MenuItem("NoTrollHere/Build Achievement Popup")]
    static void Build()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        // === PANEL POPUP — góc dưới phải ===
        GameObject panel = new GameObject("Panel_AchievementPopup");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(320f, 110f);
        rect.anchoredPosition = new Vector2(-20f, 20f);

        // Nền tối
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

        // Viền vàng trái
        GameObject vien = new GameObject("Vien_Trai");
        vien.transform.SetParent(panel.transform, false);
        RectTransform vienRect = vien.AddComponent<RectTransform>();
        vienRect.anchorMin = Vector2.zero;
        vienRect.anchorMax = new Vector2(0f, 1f);
        vienRect.offsetMin = Vector2.zero;
        vienRect.offsetMax = new Vector2(4f, 0f);
        vienRect.pivot = new Vector2(0f, 0.5f);
        vienRect.sizeDelta = new Vector2(4f, 0f);
        vien.AddComponent<Image>().color = new Color(0.96f, 0.78f, 0.26f);

        // Icon trophy
        GameObject icon = new GameObject("Txt_Icon");
        icon.transform.SetParent(panel.transform, false);
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 1f);
        iconRect.anchorMax = new Vector2(0f, 1f);
        iconRect.pivot = new Vector2(0f, 1f);
        iconRect.anchoredPosition = new Vector2(14f, -10f);
        iconRect.sizeDelta = new Vector2(30f, 30f);
        TextMeshProUGUI tmpIcon = icon.AddComponent<TextMeshProUGUI>();
        tmpIcon.text = "🏆";
        tmpIcon.fontSize = 18f;
        tmpIcon.alignment = TextAlignmentOptions.Center;

        // Txt tên thành tựu
        GameObject txtTen = new GameObject("Txt_Ten");
        txtTen.transform.SetParent(panel.transform, false);
        RectTransform tenRect = txtTen.AddComponent<RectTransform>();
        tenRect.anchorMin = new Vector2(0f, 1f);
        tenRect.anchorMax = new Vector2(1f, 1f);
        tenRect.pivot = new Vector2(0f, 1f);
        tenRect.anchoredPosition = new Vector2(50f, -10f);
        tenRect.sizeDelta = new Vector2(-60f, 26f);
        TextMeshProUGUI tmpTen = txtTen.AddComponent<TextMeshProUGUI>();
        tmpTen.text = "Century Club";
        tmpTen.fontSize = 14f;
        tmpTen.fontStyle = FontStyles.Bold;
        tmpTen.color = new Color(0.96f, 0.78f, 0.26f);
        tmpTen.alignment = TextAlignmentOptions.Left;

        // Txt mô tả
        GameObject txtMoTa = new GameObject("Txt_MoTa");
        txtMoTa.transform.SetParent(panel.transform, false);
        RectTransform moTaRect = txtMoTa.AddComponent<RectTransform>();
        moTaRect.anchorMin = new Vector2(0f, 1f);
        moTaRect.anchorMax = new Vector2(1f, 1f);
        moTaRect.pivot = new Vector2(0f, 1f);
        moTaRect.anchoredPosition = new Vector2(14f, -42f);
        moTaRect.sizeDelta = new Vector2(-24f, 22f);
        TextMeshProUGUI tmpMoTa = txtMoTa.AddComponent<TextMeshProUGUI>();
        tmpMoTa.text = "Chết 100 lần";
        tmpMoTa.fontSize = 11f;
        tmpMoTa.color = new Color(0.91f, 0.91f, 0.82f);
        tmpMoTa.alignment = TextAlignmentOptions.Left;

        // Txt nhận xét hài hước
        GameObject txtNhanXet = new GameObject("Txt_NhanXet");
        txtNhanXet.transform.SetParent(panel.transform, false);
        RectTransform nxRect = txtNhanXet.AddComponent<RectTransform>();
        nxRect.anchorMin = new Vector2(0f, 0f);
        nxRect.anchorMax = new Vector2(1f, 0f);
        nxRect.pivot = new Vector2(0f, 0f);
        nxRect.anchoredPosition = new Vector2(14f, 10f);
        nxRect.sizeDelta = new Vector2(-24f, 28f);
        TextMeshProUGUI tmpNX = txtNhanXet.AddComponent<TextMeshProUGUI>();
        tmpNX.text = "Bạn đã quen với cảm giác này rồi phải không?";
        tmpNX.fontSize = 10f;
        tmpNX.fontStyle = FontStyles.Italic;
        tmpNX.color = new Color(0.4f, 0.4f, 0.4f);
        tmpNX.alignment = TextAlignmentOptions.Left;

        // CanvasGroup để fade
        panel.AddComponent<CanvasGroup>();

        // Script
        AchievementPopup ap = panel.AddComponent<AchievementPopup>();
        ap.panelPopup = panel;
        ap.txtTen = tmpTen;
        ap.txtMoTa = tmpMoTa;
        ap.txtNhanXet = tmpNX;

        // Ẩn ban đầu
        panel.SetActive(false);

        Debug.Log("✅ Achievement Popup đã tạo!");
        EditorUtility.SetDirty(canvas.gameObject);
    }
}