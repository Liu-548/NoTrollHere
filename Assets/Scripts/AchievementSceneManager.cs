using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class AchievementSceneManager : MonoBehaviour
{
    [Header("=== UI ===")]
    public RectTransform contentParent;   // Content của ScrollRect
    public TextMeshProUGUI txtTongKet;    // "X / Y Thành tựu"
    public Button nutBack;

    [Header("=== CÀI ĐẶT ===")]
    public string tenSceneMainMenu = "MainMenu";

    // Màu sắc
    static readonly Color mauVang    = new Color(0.96f, 0.78f, 0.26f);
    static readonly Color mauSang    = new Color(0.91f, 0.91f, 0.82f);
    static readonly Color mauXam     = new Color(0.40f, 0.40f, 0.40f);
    static readonly Color mauXamNhat = new Color(0.25f, 0.25f, 0.25f);
    static readonly Color mauNen     = new Color(0.18f, 0.18f, 0.18f);
    static readonly Color mauNenKhoa = new Color(0.13f, 0.13f, 0.13f);
    static readonly Color mauDuongKe = new Color(0.22f, 0.22f, 0.22f);

    const float ITEM_H  = 80f;
    const float SPACING = 4f;

    void Start()
    {
        // Tự tìm references nếu Inspector chưa gán đúng
        if (contentParent == null)
        {
            var sr = FindFirstObjectByType<ScrollRect>();
            if (sr != null) contentParent = sr.content;
        }
        if (txtTongKet == null)
            txtTongKet = GameObject.Find("Txt_TongKet")
                ?.GetComponent<TextMeshProUGUI>();
        if (nutBack == null)
        {
            var go = GameObject.Find("Btn_Back");
            if (go != null) nutBack = go.GetComponent<Button>();
        }
        if (nutBack != null)
            nutBack.onClick.AddListener(NutBack);

        // Fix Viewport Mask: Image.color = clear + Mask trong Unity 6 → stencil bị cull → mọi thứ bên trong ẩn
        // Đổi sang RectMask2D (không dùng stencil, hoạt động ổn với mọi màu nền)
        var mask = FindFirstObjectByType<Mask>();
        if (mask != null)
        {
            GameObject vp = mask.gameObject;
            DestroyImmediate(mask);                         // xoá Mask cũ
            var img = vp.GetComponent<Image>();
            if (img != null) DestroyImmediate(img);        // xoá Image cũ (không cần nữa)
            vp.AddComponent<RectMask2D>();                  // thay bằng RectMask2D
        }

        // Chờ 1 frame để Unity layout hoàn tất trước khi tạo item
        StartCoroutine(KhoiTaoSauMotFrame());
    }

    IEnumerator KhoiTaoSauMotFrame()
    {
        yield return null;
        TaiDanhSach();
    }

    void TaiDanhSach()
    {
        if (contentParent == null)
        {
            Debug.LogError("[AchievementScene] Không tìm được contentParent!");
            return;
        }
        if (AchievementManager.instance == null)
        {
            Debug.LogWarning("[AchievementScene] AchievementManager chưa tồn tại. Load từ MainMenu trước.");
            return;
        }

        // Xoá item cũ (phòng bị gọi lại)
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        var danhSach = AchievementManager.instance.LayDanhSach();
        int daMo     = AchievementManager.instance.LaySoLuongDaMo();

        if (txtTongKet != null)
            txtTongKet.text = $"{daMo} / {danhSach.Count} Thành tựu";

        float y = 0f;
        foreach (var tt in danhSach)
        {
            TaoItem(tt, y);
            y -= ITEM_H + SPACING;
        }

        // Đặt chiều cao Content để ScrollRect tính đúng phạm vi cuộn
        float tongCao = danhSach.Count * ITEM_H
                      + Mathf.Max(0, danhSach.Count - 1) * SPACING;
        contentParent.sizeDelta = new Vector2(contentParent.sizeDelta.x, tongCao);

        // Cuộn lên đầu danh sách
        var sr = contentParent.GetComponentInParent<ScrollRect>();
        if (sr != null) sr.verticalNormalizedPosition = 1f;
    }

    void TaoItem(AchievementManager.ThanhTuu tt, float y)
    {
        bool anHidden = tt.anDi && !tt.daUnlock;

        // === ROW ===
        GameObject row = new GameObject("Item_" + tt.id);
        row.transform.SetParent(contentParent, false);
        RectTransform rcRow = row.AddComponent<RectTransform>();
        rcRow.anchorMin        = new Vector2(0f, 1f);
        rcRow.anchorMax        = new Vector2(1f, 1f);
        rcRow.pivot            = new Vector2(0.5f, 1f);
        rcRow.sizeDelta        = new Vector2(0f, ITEM_H);
        rcRow.anchoredPosition = new Vector2(0f, y);
        row.AddComponent<Image>().color = tt.daUnlock ? mauNen : mauNenKhoa;

        // === ICON (chấm màu bên trái) ===
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(row.transform, false);
        RectTransform rcIcon = icon.AddComponent<RectTransform>();
        rcIcon.anchorMin        = new Vector2(0f, 0.5f);
        rcIcon.anchorMax        = new Vector2(0f, 0.5f);
        rcIcon.pivot            = new Vector2(0f, 0.5f);
        rcIcon.sizeDelta        = new Vector2(18f, 18f);
        rcIcon.anchoredPosition = new Vector2(24f, 0f);
        icon.AddComponent<Image>().color = tt.daUnlock
            ? mauVang
            : new Color(0.22f, 0.22f, 0.22f);

        // === TÊN ===
        GameObject goTen = new GameObject("Txt_Ten");
        goTen.transform.SetParent(row.transform, false);
        RectTransform rcTen = goTen.AddComponent<RectTransform>();
        rcTen.anchorMin = new Vector2(0f, 0.5f);
        rcTen.anchorMax = new Vector2(1f, 1f);
        rcTen.offsetMin = new Vector2(58f, 2f);
        rcTen.offsetMax = new Vector2(-20f, -4f);
        TextMeshProUGUI tmpTen = goTen.AddComponent<TextMeshProUGUI>();
        tmpTen.text          = anHidden ? "???" : tt.ten;
        tmpTen.fontSize      = 15f;
        tmpTen.color         = tt.daUnlock ? mauSang : mauXam;
        tmpTen.fontStyle     = FontStyles.Bold;
        tmpTen.alignment     = TextAlignmentOptions.BottomLeft;
        tmpTen.raycastTarget = false;

        // === MÔ TẢ ===
        GameObject goMoTa = new GameObject("Txt_MoTa");
        goMoTa.transform.SetParent(row.transform, false);
        RectTransform rcMoTa = goMoTa.AddComponent<RectTransform>();
        rcMoTa.anchorMin = new Vector2(0f, 0f);
        rcMoTa.anchorMax = new Vector2(1f, 0.5f);
        rcMoTa.offsetMin = new Vector2(58f, 4f);
        rcMoTa.offsetMax = new Vector2(-20f, -2f);
        TextMeshProUGUI tmpMoTa = goMoTa.AddComponent<TextMeshProUGUI>();
        tmpMoTa.text          = anHidden ? "Thành tựu bí mật" : tt.moTa;
        tmpMoTa.fontSize      = 12f;
        tmpMoTa.color         = anHidden ? mauXamNhat : mauXam;
        tmpMoTa.alignment     = TextAlignmentOptions.TopLeft;
        tmpMoTa.raycastTarget = false;

        // === ĐƯỜNG KẺ DƯỚI ===
        GameObject line = new GameObject("Separator");
        line.transform.SetParent(row.transform, false);
        RectTransform rcLine = line.AddComponent<RectTransform>();
        rcLine.anchorMin        = new Vector2(0f, 0f);
        rcLine.anchorMax        = new Vector2(1f, 0f);
        rcLine.pivot            = new Vector2(0.5f, 0f);
        rcLine.sizeDelta        = new Vector2(0f, 1f);
        rcLine.anchoredPosition = Vector2.zero;
        line.AddComponent<Image>().color = mauDuongKe;
    }

    public void NutBack()
    {
        SceneManager.LoadScene(tenSceneMainMenu);
    }
}
