using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// On-screen controls cho Android:
///   Trái/Phải: 2 nút bấm giữ (◀ ▶)
///   Nhảy: nút hình mũi tên lên (vẽ runtime)
///   Ngồi: nút hình mũi tên xuống (vẽ runtime)
///   Pause: ⏸
/// Tự ẩn trên PC. Feed input vào GameInput.instance.
/// </summary>
public class MobileUIOverlay : MonoBehaviour
{
    [Header("=== THAM CHIẾU ===")]
    [Tooltip("Panel root chứa overlay. Để trống = tự tạo runtime.")]
    public GameObject overlayRoot;

    // Trạng thái nút đang giữ
    private bool leftHeld, rightHeld, jumpHeld, crouchHeld;

    // Màu nút
    private static readonly Color COL_BTN       = new Color(1f, 1f, 1f, 0.15f);
    private static readonly Color COL_BTN_PRESS = new Color(1f, 1f, 1f, 0.42f);

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
#if UNITY_ANDROID
        if (overlayRoot == null) TaoPanelRoot();
        if (overlayRoot != null) TaoControls();
        // PC/Editor: overlayRoot (Panel_SafeArea) phải luôn active vì chứa cả HUD
        // → không SetActive(false) trên non-Android nữa
#endif
    }

    void Update()
    {
#if UNITY_ANDROID
        float x = (rightHeld ? 1f : 0f) - (leftHeld  ? 1f : 0f);
        float y = (jumpHeld  ? 1f : 0f) - (crouchHeld ? 1f : 0f); // leo dây lên/xuống
        GameInput.instance?.SetVirtualMove(new Vector2(x, y));
        GameInput.instance?.SetVirtualJump(jumpHeld);
        GameInput.instance?.SetVirtualCrouch(crouchHeld);
#endif
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
        // ── Di chuyển: sprite mũi tên trái/phải, góc dưới trái ──
        TaoNutSprite("Btn_Left",
            sprite: TaoSpriteMuiTen(huong: Huong.Trai),
            anchor: new Vector2(0f, 0f), pivot: new Vector2(0f, 0f),
            pos: new Vector2(20f, 20f), size: new Vector2(100f, 90f),
            onDown: _ => leftHeld  = true,
            onUp:   _ => leftHeld  = false);

        TaoNutSprite("Btn_Right",
            sprite: TaoSpriteMuiTen(huong: Huong.Phai),
            anchor: new Vector2(0f, 0f), pivot: new Vector2(0f, 0f),
            pos: new Vector2(130f, 20f), size: new Vector2(100f, 90f),
            onDown: _ => rightHeld = true,
            onUp:   _ => rightHeld = false);

        // ── Action: Nhảy + Ngồi, góc dưới phải ──
        TaoNutSprite("Btn_Jump",
            sprite: TaoSpriteMuiTen(huong: Huong.Len),
            anchor: new Vector2(1f, 0f), pivot: new Vector2(1f, 0f),
            pos: new Vector2(-130f, 20f), size: new Vector2(100f, 90f),
            onDown: _ => jumpHeld   = true,
            onUp:   _ => jumpHeld   = false);

        TaoNutSprite("Btn_Crouch",
            sprite: TaoSpriteMuiTen(huong: Huong.Xuong),
            anchor: new Vector2(1f, 0f), pivot: new Vector2(1f, 0f),
            pos: new Vector2(-20f, 20f), size: new Vector2(100f, 90f),
            onDown: _ => crouchHeld = true,
            onUp:   _ => crouchHeld = false);

        // Pause đã có Btn_Pause trong HUD → không tạo thêm
    }

    private enum Huong { Len, Xuong, Trai, Phai }

    // ─── NÚT SPRITE (mũi tên vẽ runtime) ────────────────────────────
    void TaoNutSprite(string name, Sprite sprite,
        Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size,
        UnityEngine.Events.UnityAction<BaseEventData> onDown,
        UnityEngine.Events.UnityAction<BaseEventData> onUp)
    {
        var go  = TaoNutBase(name, anchor, pivot, pos, size);
        var img = go.GetComponent<Image>();

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

        var trigger = go.AddComponent<EventTrigger>();
        if (onDown != null)
            AddTrigger(trigger, EventTriggerType.PointerDown, d =>
            {
                img.color = COL_BTN_PRESS;
                onDown(d);
            });
        if (onUp != null)
            AddTrigger(trigger, EventTriggerType.PointerUp, d =>
            {
                img.color = COL_BTN;
                onUp(d);
            });
        AddTrigger(trigger, EventTriggerType.PointerExit, d =>
        {
            img.color = COL_BTN;
            onUp?.Invoke(d);
        });
    }

    // ─── HELPER: tạo base GameObject nút ─────────────────────────────
    GameObject TaoNutBase(string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
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
        return go;
    }

    static void AddTrigger(EventTrigger et, EventTriggerType type,
        UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(action);
        et.triggers.Add(entry);
    }

    // ─── VẼ SPRITE MŨI TÊN RUNTIME (4 hướng) ────────────────────────
    static Sprite TaoSpriteMuiTen(Huong huong)
    {
        const int W = 64, H = 64;
        const float PAD = 0.10f; // lề tương đối [0..1]

        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            name       = huong.ToString()
        };

        var pixels = new Color[W * H];
        for (int py = 0; py < H; py++)
        for (int px = 0; px < W; px++)
        {
            float fx = (px + 0.5f) / W; // [0,1]
            float fy = (py + 0.5f) / H;

            // Tam giác theo hướng: t = khoảng cách từ đáy đến đỉnh [0,1]
            // halfSpan = nửa chiều rộng tại vị trí t (thu về 0 ở đỉnh)
            float t;
            bool inside;
            switch (huong)
            {
                case Huong.Len:
                    // đỉnh ở trên (fy=1-PAD), đáy ở dưới (fy=PAD)
                    t = Mathf.Clamp01((fy - PAD) / (1f - 2f * PAD));
                    inside = fy >= PAD && fy <= 1f - PAD
                          && Mathf.Abs(fx - 0.5f) <= Mathf.Lerp(0.5f - PAD, 0f, t);
                    break;
                case Huong.Xuong:
                    // đỉnh ở dưới (fy=PAD), đáy ở trên (fy=1-PAD)
                    t = Mathf.Clamp01((1f - PAD - fy) / (1f - 2f * PAD));
                    inside = fy >= PAD && fy <= 1f - PAD
                          && Mathf.Abs(fx - 0.5f) <= Mathf.Lerp(0.5f - PAD, 0f, t);
                    break;
                case Huong.Trai:
                    // đỉnh ở trái (fx=PAD), đáy ở phải (fx=1-PAD)
                    t = Mathf.Clamp01((1f - PAD - fx) / (1f - 2f * PAD));
                    inside = fx >= PAD && fx <= 1f - PAD
                          && Mathf.Abs(fy - 0.5f) <= Mathf.Lerp(0.5f - PAD, 0f, t);
                    break;
                default: // Phai
                    // đỉnh ở phải (fx=1-PAD), đáy ở trái (fx=PAD)
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

    // ─── CLEANUP ─────────────────────────────────────────────────────
    void OnDisable()
    {
        leftHeld = rightHeld = jumpHeld = crouchHeld = false;
        GameInput.instance?.SetVirtualMove(Vector2.zero);
        GameInput.instance?.SetVirtualJump(false);
        GameInput.instance?.SetVirtualCrouch(false);
    }
}
