using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Khung vàng di chuyển tới nút đang được chọn bằng bàn phím trong các scene menu.
/// ChonNut(btnRT)              → chữ nhật thẳng (MainMenu, LevelSelect, Achievements)
/// ChonNut(btnRT, tronGoc:true) → chữ nhật bo tròn  (PauseMenu)
/// </summary>
public static class MenuSelectionFrame
{
    private static GameObject      khung;
    private static RectTransform   khungRT;
    private static Transform       chaHienTai;
    private static bool            modeTronGoc = false;

    // Cache texture bo tròn — tái tạo khi kích thước nút thay đổi
    private static Texture2D texTronGoc;
    private static Sprite    spriteTronGoc;
    private static int       texW = -1, texH = -1;

    private static readonly Color MAU_VANG  = new Color(0.96f, 0.78f, 0.26f, 1f); // #F5C842
    private const float           DO_DAY_PX = 2f;   // độ dày viền (canvas pixel)
    private const int             CORNER_R  = 5;    // bán kính góc tròn (canvas pixel)

    // =========================================================
    // PUBLIC API
    // =========================================================

    /// <param name="tronGoc">true = bo tròn góc (PauseMenu), false = chữ nhật thẳng</param>
    public static void ChonNut(RectTransform btnRT, bool tronGoc = false)
    {
        if (btnRT == null) { An(); return; }

        Transform cha = btnRT.parent;
        if (khung == null || chaHienTai != cha || modeTronGoc != tronGoc)
        {
            DamBaoTao(cha, tronGoc);
            modeTronGoc = tronGoc;
        }
        if (khung == null) return;

        // Copy RectTransform từ button (cùng parent → cùng hệ tọa độ)
        khungRT.anchorMin        = btnRT.anchorMin;
        khungRT.anchorMax        = btnRT.anchorMax;
        khungRT.pivot            = btnRT.pivot;
        khungRT.anchoredPosition = btnRT.anchoredPosition;
        khungRT.sizeDelta        = btnRT.sizeDelta;

        // Chế độ bo tròn: cập nhật texture nếu kích thước nút thay đổi
        if (tronGoc)
            CapNhatTextureTronGoc(btnRT);

        khung.SetActive(true);
        khung.transform.SetAsLastSibling();
    }

    public static void An() { if (khung != null) khung.SetActive(false); }

    // =========================================================
    // INTERNAL – khởi tạo object khung
    // =========================================================

    static void DamBaoTao(Transform cha, bool tronGoc)
    {
        if (khung != null) Object.Destroy(khung);
        chaHienTai = cha;

        khung   = new GameObject("KeyboardSelectionFrame");
        khung.transform.SetParent(cha, false);
        khungRT = khung.AddComponent<RectTransform>();

        // LayoutElement: bỏ qua LayoutGroup để không bị dời chỗ
        var le = khung.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        var img = khung.AddComponent<Image>();
        img.raycastTarget = false;

        if (tronGoc)
        {
            // Chế độ bo tròn: texture sẽ được tạo/cập nhật trong CapNhatTextureTronGoc()
            img.color = MAU_VANG;
            img.type  = Image.Type.Simple;
        }
        else
        {
            // Chế độ chữ nhật thẳng: 4 dải màu
            img.color = Color.clear;
            float d = DO_DAY_PX;
            TaoVien("Top",    new Vector2(0,1), new Vector2(1,1), new Vector2(0,-d), Vector2.zero);
            TaoVien("Bottom", Vector2.zero,     new Vector2(1,0), Vector2.zero,      new Vector2(0, d));
            TaoVien("Left",   Vector2.zero,     new Vector2(0,1), Vector2.zero,      new Vector2(d, 0));
            TaoVien("Right",  new Vector2(1,0), Vector2.one,      new Vector2(-d,0), Vector2.zero);
        }

        khung.SetActive(false);
    }

    static void TaoVien(string ten, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
    {
        var go = new GameObject(ten);
        go.transform.SetParent(khung.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = oMin; rt.offsetMax = oMax;
        var im = go.AddComponent<Image>();
        im.color = MAU_VANG;
        im.raycastTarget = false;
    }

    // =========================================================
    // INTERNAL – texture bo tròn khớp kích thước nút
    // =========================================================

    /// <summary>
    /// Tạo (hoặc tái dùng) texture vẽ viền bo tròn đúng với kích thước nút.
    /// Dùng Image.Type.Simple — không stretch, không oval.
    /// </summary>
    static void CapNhatTextureTronGoc(RectTransform btnRT)
    {
        // Lấy kích thước thực của nút (canvas pixel)
        Vector2 sd = btnRT.sizeDelta;
        int w = Mathf.RoundToInt(Mathf.Abs(sd.x > 1 ? sd.x : btnRT.rect.width));
        int h = Mathf.RoundToInt(Mathf.Abs(sd.y > 1 ? sd.y : btnRT.rect.height));
        if (w <= 0 || h <= 0)
        {
            // Layout chưa sẵn sàng: ẩn tạm để không hiện hình vàng đặc
            var imgFail = khung?.GetComponent<Image>();
            if (imgFail != null) imgFail.color = Color.clear;
            return;
        }

        // Tái tạo texture chỉ khi kích thước thay đổi
        if (w != texW || h != texH || texTronGoc == null)
        {
            texW = w; texH = h;

            if (texTronGoc    != null) Object.Destroy(texTronGoc);
            if (spriteTronGoc != null) Object.Destroy(spriteTronGoc);

            texTronGoc = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                name       = "RoundedFrameTex"
            };

            int bw   = Mathf.Max(1, Mathf.RoundToInt(DO_DAY_PX));
            int r    = CORNER_R;
            float fw = w, fh = h;

            var pixels = new Color[w * h];
            for (int py = 0; py < h; py++)
            for (int px = 0; px < w; px++)
            {
                float fx = px + 0.5f, fy = py + 0.5f;
                bool inOuter = TrongRR(fx, fy, 0,  0,  fw,    fh,    r + bw);
                bool inInner = TrongRR(fx, fy, bw, bw, fw-bw, fh-bw, r);
                pixels[py * w + px] = (inOuter && !inInner) ? Color.white : Color.clear;
            }

            texTronGoc.SetPixels(pixels);
            texTronGoc.Apply();

            // PPU = 1 → 1 texel = 1 canvas pixel → Image.Simple hiển thị đúng 1:1
            spriteTronGoc = Sprite.Create(
                texTronGoc,
                new Rect(0, 0, w, h),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 1f
            );
            spriteTronGoc.name = "RoundedFrameSprite";
        }

        // LUÔN gán sprite vào Image — khung có thể vừa được tạo mới (scene reload)
        var img = khung?.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = spriteTronGoc;
            img.type   = Image.Type.Simple;
            img.color  = MAU_VANG;
        }
    }

    // =========================================================
    // HELPERS
    // =========================================================

    // Kiểm tra (px,py) có nằm trong rounded rect [x0,x1)×[y0,y1) với góc bo radius r
    static bool TrongRR(float px, float py, float x0, float y0, float x1, float y1, float r)
    {
        if (px < x0 || px >= x1 || py < y0 || py >= y1) return false;
        r = Mathf.Min(r, (x1 - x0) * 0.5f, (y1 - y0) * 0.5f);
        if (px < x0+r && py < y0+r) return Dist2(px, py, x0+r, y0+r) <= r*r;
        if (px >= x1-r && py < y0+r) return Dist2(px, py, x1-r, y0+r) <= r*r;
        if (px < x0+r && py >= y1-r) return Dist2(px, py, x0+r, y1-r) <= r*r;
        if (px >= x1-r && py >= y1-r) return Dist2(px, py, x1-r, y1-r) <= r*r;
        return true;
    }

    static float Dist2(float ax, float ay, float bx, float by)
        => (ax-bx)*(ax-bx) + (ay-by)*(ay-by);
}
