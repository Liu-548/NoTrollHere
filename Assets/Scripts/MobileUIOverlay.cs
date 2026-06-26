using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// On-screen controls cho Android:
///   Trái/Phải: 2 nút bấm giữ (◀ ▶)
///   Nhảy: nút hình mũi tên lên
///   Ngồi: nút hình mũi tên xuống
/// Tự ẩn trên PC. Feed input vào GameInput.instance.
/// Dùng polling (RectTransformUtility) thay vì EventTrigger để tránh lỗi
/// PointerDown không kích hoạt trong Editor Android simulation.
/// </summary>
[DefaultExecutionOrder(-50)]
public class MobileUIOverlay : MonoBehaviour
{
    [Header("=== THAM CHIẾU ===")]
    [Tooltip("Panel root chứa overlay. Để trống = tự tạo runtime.")]
    public GameObject overlayRoot;

    // Trạng thái nút frame trước (để phát hiện rising edge)
    private bool leftHeld, rightHeld, jumpHeld, crouchHeld;

    // RectTransform của từng nút để polling
    private RectTransform rtLeft, rtRight, rtJump, rtCrouch;

    // Image của từng nút để đổi màu
    private Image imgLeft, imgRight, imgJump, imgCrouch;

    // Màu nút
    private static readonly Color COL_BTN       = new Color(1f, 1f, 1f, 0.15f);
    private static readonly Color COL_BTN_PRESS = new Color(1f, 1f, 1f, 0.42f);

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
#if UNITY_ANDROID
        // Vô hiệu hóa các instance cũ có thể sót từ DontDestroyOnLoad
        var all = FindObjectsByType<MobileUIOverlay>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var mo in all)
            if (mo != this) mo.enabled = false;
        GameInput.instance?.SetVirtualMove(Vector2.zero);
        GameInput.instance?.SetVirtualJump(false);
        GameInput.instance?.SetVirtualCrouch(false);

        if (overlayRoot == null) TaoPanelRoot();
        if (overlayRoot != null) TaoControls();
#endif
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        leftHeld = rightHeld = jumpHeld = crouchHeld = false;
        GameInput.instance?.SetVirtualMove(Vector2.zero);
        GameInput.instance?.SetVirtualJump(false);
        GameInput.instance?.SetVirtualCrouch(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset khi load scene mới — tránh trạng thái kẹt từ DontDestroyOnLoad
        leftHeld = rightHeld = jumpHeld = crouchHeld = false;
    }

    void Update()
    {
#if UNITY_ANDROID
        // ── Polling: đọc trạng thái thực tế của pointer/touch ──
        bool newLeft   = IsPointerInside(rtLeft);
        bool newRight  = IsPointerInside(rtRight);
        bool newJump   = IsPointerInside(rtJump);
        bool newCrouch = IsPointerInside(rtCrouch);

        // Rising edge → "pressed this frame"
        if (newJump && !jumpHeld)
            GameInput.instance?.FireVirtualJumpPress();

        // Cập nhật màu nút khi trạng thái thay đổi
        if (newLeft   != leftHeld   && imgLeft   != null) imgLeft.color   = newLeft   ? COL_BTN_PRESS : COL_BTN;
        if (newRight  != rightHeld  && imgRight  != null) imgRight.color  = newRight  ? COL_BTN_PRESS : COL_BTN;
        if (newJump   != jumpHeld   && imgJump   != null) imgJump.color   = newJump   ? COL_BTN_PRESS : COL_BTN;
        if (newCrouch != crouchHeld && imgCrouch != null) imgCrouch.color = newCrouch ? COL_BTN_PRESS : COL_BTN;

        leftHeld   = newLeft;
        rightHeld  = newRight;
        jumpHeld   = newJump;
        crouchHeld = newCrouch;

        float x = (rightHeld ? 1f : 0f) - (leftHeld   ? 1f : 0f);
        float y = (jumpHeld  ? 1f : 0f) - (crouchHeld ? 1f : 0f);
        GameInput.instance?.SetVirtualMove(new Vector2(x, y));
        GameInput.instance?.SetVirtualJump(jumpHeld);
        GameInput.instance?.SetVirtualCrouch(crouchHeld);
#endif
    }

    // ─── POLLING: kiểm tra pointer/touch có nằm trong button không ───
    bool IsPointerInside(RectTransform rt)
    {
        if (rt == null) return false;
        var canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;

        // Touch — New Input System (Android thực)
        var ts = Touchscreen.current;
        if (ts != null)
        {
            foreach (var touch in ts.touches)
                if (touch.press.isPressed &&
                    RectTransformUtility.RectangleContainsScreenPoint(rt, touch.position.ReadValue(), cam))
                    return true;
        }

        // Mouse — New Input System (Editor / PC)
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed &&
            RectTransformUtility.RectangleContainsScreenPoint(rt, mouse.position.ReadValue(), cam))
            return true;

        return false;
    }

    // ─── TẠO PANEL ROOT ───────────────────────────────────────────────
    void TaoPanelRoot()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        overlayRoot = new GameObject("MobileOverlay");
        overlayRoot.transform.SetParent(canvas.transform, false);
        var rt = overlayRoot.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ─── TẠO TẤT CẢ CONTROLS ─────────────────────────────────────────
    void TaoControls()
    {
        (rtLeft,   imgLeft)   = TaoNut("Btn_Left",   TaoSpriteMuiTen(Huong.Trai),
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f,   20f), new Vector2(100f, 90f));

        (rtRight,  imgRight)  = TaoNut("Btn_Right",  TaoSpriteMuiTen(Huong.Phai),
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(130f,  20f), new Vector2(100f, 90f));

        (rtJump,   imgJump)   = TaoNut("Btn_Jump",   TaoSpriteMuiTen(Huong.Len),
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-130f, 20f), new Vector2(100f, 90f));

        (rtCrouch, imgCrouch) = TaoNut("Btn_Crouch", TaoSpriteMuiTen(Huong.Xuong),
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-20f,  20f), new Vector2(100f, 90f));
    }

    private enum Huong { Len, Xuong, Trai, Phai }

    // ─── TẠO NÚT (không EventTrigger — polling xử lý input) ─────────
    (RectTransform rt, Image img) TaoNut(string name, Sprite sprite,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(overlayRoot.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchor; rt.anchorMax = anchor;
        rt.pivot     = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = COL_BTN;

        // Icon sprite ở giữa nút
        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(go.transform, false);
        var iconRT = iconGo.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.15f, 0.15f);
        iconRT.anchorMax = new Vector2(0.85f, 0.85f);
        iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
        var iconImg = iconGo.AddComponent<Image>();
        iconImg.sprite        = sprite;
        iconImg.color         = Color.white;
        iconImg.raycastTarget = false;

        return (rt, img);
    }

    // ─── VẼ SPRITE MŨI TÊN RUNTIME (4 hướng) ────────────────────────
    static Sprite TaoSpriteMuiTen(Huong huong)
    {
        const int W = 64, H = 64;
        const float PAD = 0.10f;

        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            name       = huong.ToString()
        };

        var pixels = new Color[W * H];
        for (int py = 0; py < H; py++)
        for (int px = 0; px < W; px++)
        {
            float fx = (px + 0.5f) / W;
            float fy = (py + 0.5f) / H;
            float t;
            bool inside;
            switch (huong)
            {
                case Huong.Len:
                    t = Mathf.Clamp01((fy - PAD) / (1f - 2f * PAD));
                    inside = fy >= PAD && fy <= 1f - PAD
                          && Mathf.Abs(fx - 0.5f) <= Mathf.Lerp(0.5f - PAD, 0f, t);
                    break;
                case Huong.Xuong:
                    t = Mathf.Clamp01((1f - PAD - fy) / (1f - 2f * PAD));
                    inside = fy >= PAD && fy <= 1f - PAD
                          && Mathf.Abs(fx - 0.5f) <= Mathf.Lerp(0.5f - PAD, 0f, t);
                    break;
                case Huong.Trai:
                    t = Mathf.Clamp01((1f - PAD - fx) / (1f - 2f * PAD));
                    inside = fx >= PAD && fx <= 1f - PAD
                          && Mathf.Abs(fy - 0.5f) <= Mathf.Lerp(0.5f - PAD, 0f, t);
                    break;
                default: // Phai
                    t = Mathf.Clamp01((fx - PAD) / (1f - 2f * PAD));
                    inside = fx >= PAD && fx <= 1f - PAD
                          && Mathf.Abs(fy - 0.5f) <= Mathf.Lerp(0.5f - PAD, 0f, t);
                    break;
            }
            pixels[py * W + px] = inside ? Color.white : Color.clear;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 1f);
    }
}
