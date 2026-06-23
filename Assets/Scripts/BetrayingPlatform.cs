using UnityEngine;
using System.Collections;

public class BetrayingPlatform : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  ENUMS
    // ─────────────────────────────────────────────────────────────

    public enum CheDoDidong
    {
        TuDong,          // Tự động A→B→A lặp mãi
        ChoTrigger,      // Đứng yên, chờ BetrayingTrigger kích hoạt
        TuDongMotChieu,  // Tự động A→B một lần rồi dừng
        Waypoint,        // Tự động chạy theo danh sách waypoint
        XoayBay          // Xoay đúng N độ, đẩy player xuống vực
    }

    public enum CheDoWaypoint
    {
        MotChieu,  // WP0→WP1→...→WPn rồi dừng
        LapLai,    // WP0→WP1→...→WPn→WP0 vòng lặp
        PingPong   // WP0→WPn→WP0→WPn qua lại
    }

    public enum HuongXoay
    {
        NguocKimDongHo,  // ngược chiều kim đồng hồ (+Z)
        XuoiKimDongHo    // xuôi chiều kim đồng hồ  (-Z)
    }

    public enum CheDoSauXoay
    {
        DungYen,     // giữ nguyên góc đã xoay, không làm gì thêm
        TroVeGoc,    // xoay ngược về góc ban đầu rồi dừng
        PhaHuy,      // xóa object sau khi xoay xong
        XoayVoHan    // sau khi xoay đến góc, tiếp tục xoay mãi cùng hướng
    }

    // ─────────────────────────────────────────────────────────────
    //  INSPECTOR — CHẾ ĐỘ
    // ─────────────────────────────────────────────────────────────

    [Header("=== CHẾ ĐỘ ===")]
    public CheDoDidong cheDo = CheDoDidong.ChoTrigger;

    // ─────────────────────────────────────────────────────────────
    //  INSPECTOR — WAYPOINTS  (chỉ dùng khi cheDo = Waypoint)
    // ─────────────────────────────────────────────────────────────

    [Header("=== WAYPOINTS ===")]
    public Vector2[] cacWaypoint = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(0, 3),
        new Vector2(5, 3)
    };
    public CheDoWaypoint cheDoWaypoint = CheDoWaypoint.LapLai;
    public float delayTaiMoiWaypoint = 0.3f;

    // ─────────────────────────────────────────────────────────────
    //  INSPECTOR — DI CHUYỂN THẲNG  (TuDong / ChoTrigger / TuDongMotChieu)
    // ─────────────────────────────────────────────────────────────

    [Header("=== DI CHUYỂN THẲNG ===")]
    [Tooltip("Điểm đến tính từ vị trí gốc (không dùng cho Waypoint / XoayBay)")]
    public Vector2 offsetDiemDen = new Vector2(3f, 0f);
    public float tocDo = 3f;
    [Tooltip("Tốc độ về (chỉ TuDong). 0 = dùng chung tocDo")]
    public float tocDoVe = 0f;

    // ─────────────────────────────────────────────────────────────
    //  INSPECTOR — DELAY  (dùng cho mọi chế độ)
    // ─────────────────────────────────────────────────────────────

    [Header("=== DELAY ===")]
    [Tooltip("Delay trước khi bắt đầu di chuyển / xoay")]
    public float delayTruocKhiDi = 0f;
    [Tooltip("Thời gian đứng yên tại điểm đến trước khi về (chỉ TuDong)")]
    public float delayTaiDiem = 0.5f;

    // ─────────────────────────────────────────────────────────────
    //  INSPECTOR — XOAY BAY  (chỉ dùng khi cheDo = XoayBay)
    // ─────────────────────────────────────────────────────────────

    [Header("=== XOAY BAY ===")]
    [Tooltip("Số độ cần xoay (luôn dương; hướng chọn ở Huong Xoay)")]
    public float gocXoayBay = 90f;
    public HuongXoay huongXoay = HuongXoay.NguocKimDongHo;
    [Tooltip("Tốc độ xoay đi (độ/giây)")]
    public float tocDoXoayBay = 120f;
    [Tooltip("Sau khi xoay tới góc: DungYen / TroVeGoc / PhaHuy / XoayVoHan")]
    public CheDoSauXoay cheDoSauXoay = CheDoSauXoay.DungYen;
    [Tooltip("Thời gian đứng yên trước khi xoay về (chỉ TroVeGoc)")]
    public float delayTruocKhiTroVe = 1f;
    [Tooltip("Tốc độ xoay về (chỉ TroVeGoc). 0 = dùng chung tocDoXoayBay")]
    public float tocDoXoayVe = 120f;

    // ─────────────────────────────────────────────────────────────
    //  INSPECTOR — PHÁ HỦY  (dùng cho mọi chế độ)
    // ─────────────────────────────────────────────────────────────

    [Header("=== PHÁ HỦY ===")]
    [Tooltip("Tự xóa object sau khi đến đích / xoay xong")]
    public bool phaHuySauKhiDen = false;
    public float delayPhaHuy = 0.5f;

    // ─────────────────────────────────────────────────────────────
    //  INTERNAL
    // ─────────────────────────────────────────────────────────────

    [HideInInspector] public Vector3 viTriGoc;
    [HideInInspector] public Vector3 viTriDen;

    private Transform playerTransform;
    private Vector3 viTriPlatformTruoc;
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private bool dangDiChuyen = false;

    // ─────────────────────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        viTriGoc = transform.position;
        viTriDen = viTriGoc + new Vector3(offsetDiemDen.x, offsetDiemDen.y, 0);
        viTriPlatformTruoc = transform.position;

        switch (cheDo)
        {
            case CheDoDidong.TuDong:         StartCoroutine(TuDongLapLai());    break;
            case CheDoDidong.TuDongMotChieu: StartCoroutine(TuDongMotChieu());  break;
            case CheDoDidong.Waypoint:       StartCoroutine(DiChuyenWaypoint()); break;
            case CheDoDidong.XoayBay:        StartCoroutine(XoayBayCoroutine()); break;
            // ChoTrigger: ngồi yên chờ BetrayingTrigger gọi
        }
    }

    void Update()
    {
        // Kéo player theo khi platform tịnh tiến
        Vector3 delta = transform.position - viTriPlatformTruoc;
        if (playerTransform != null && delta != Vector3.zero)
            playerTransform.position += delta;
        viTriPlatformTruoc = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerTransform = collision.transform;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerTransform = null;
    }

    // ─────────────────────────────────────────────────────────────
    //  CHẾ ĐỘ: TU DONG  (A→B→A lặp)
    // ─────────────────────────────────────────────────────────────

    IEnumerator TuDongLapLai()
    {
        if (delayTruocKhiDi > 0) yield return new WaitForSeconds(delayTruocKhiDi);

        while (true)
        {
            yield return StartCoroutine(DiToi(viTriDen, tocDo));
            yield return new WaitForSeconds(delayTaiDiem);

            float tocVe = tocDoVe > 0 ? tocDoVe : tocDo;
            yield return StartCoroutine(DiToi(viTriGoc, tocVe));
            yield return new WaitForSeconds(delayTaiDiem);

            if (phaHuySauKhiDen) { yield return StartCoroutine(PhaHuy()); yield break; }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  CHẾ ĐỘ: TU DONG MOT CHIEU  (A→B một lần)
    // ─────────────────────────────────────────────────────────────

    IEnumerator TuDongMotChieu()
    {
        if (delayTruocKhiDi > 0) yield return new WaitForSeconds(delayTruocKhiDi);
        yield return StartCoroutine(DiToi(viTriDen, tocDo));
        if (phaHuySauKhiDen) yield return StartCoroutine(PhaHuy());
    }

    // ─────────────────────────────────────────────────────────────
    //  CHẾ ĐỘ: CHO TRIGGER — kích hoạt từ BetrayingTrigger (offset)
    // ─────────────────────────────────────────────────────────────

    public void KichHoat(Vector2 offset, float tocDoOverride)
    {
        if (dangDiChuyen) return;
        Vector3 dich = viTriGoc + new Vector3(offset.x, offset.y, 0);
        float toc = tocDoOverride > 0 ? tocDoOverride : tocDo;
        StopAllCoroutines();
        StartCoroutine(DiTuTrigger(dich, toc));
    }

    IEnumerator DiTuTrigger(Vector3 dich, float toc)
    {
        if (delayTruocKhiDi > 0) yield return new WaitForSeconds(delayTruocKhiDi);
        yield return StartCoroutine(DiToi(dich, toc));
        if (phaHuySauKhiDen) yield return StartCoroutine(PhaHuy());
    }

    // ─────────────────────────────────────────────────────────────
    //  CHẾ ĐỘ: WAYPOINT — kích hoạt từ BetrayingTrigger
    // ─────────────────────────────────────────────────────────────

    public void KichHoatWaypoint(int batDauTu = 0)
    {
        if (cacWaypoint == null || cacWaypoint.Length == 0)
        {
            Debug.LogWarning("BetrayingPlatform: Không có waypoint!");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(DiChuyenWaypointTuIndex(batDauTu));
    }

    IEnumerator DiChuyenWaypoint()
    {
        if (cacWaypoint == null || cacWaypoint.Length == 0) yield break;
        if (delayTruocKhiDi > 0) yield return new WaitForSeconds(delayTruocKhiDi);
        yield return StartCoroutine(ChayWaypoint(0));
    }

    IEnumerator DiChuyenWaypointTuIndex(int indexBatDau)
    {
        if (delayTruocKhiDi > 0) yield return new WaitForSeconds(delayTruocKhiDi);
        yield return StartCoroutine(ChayWaypoint(indexBatDau));
    }

    IEnumerator ChayWaypoint(int indexBatDau)
    {
        Vector3[] danhSachDiem = new Vector3[cacWaypoint.Length];
        for (int i = 0; i < cacWaypoint.Length; i++)
            danhSachDiem[i] = viTriGoc + new Vector3(cacWaypoint[i].x, cacWaypoint[i].y, 0);

        int chiSoHienTai = Mathf.Clamp(indexBatDau, 0, danhSachDiem.Length - 1);
        int huong = 1;
        bool hoanThanhMotVong = false;

        if (chiSoHienTai > 0)
        {
            yield return StartCoroutine(DiToi(danhSachDiem[chiSoHienTai], tocDo));
            yield return new WaitForSeconds(delayTaiMoiWaypoint);
        }

        while (true)
        {
            int chiSoTiep = chiSoHienTai + huong;

            if (chiSoTiep >= danhSachDiem.Length)
            {
                if (cheDoWaypoint == CheDoWaypoint.MotChieu)
                {
                    if (phaHuySauKhiDen) yield return StartCoroutine(PhaHuy());
                    yield break;
                }
                else if (cheDoWaypoint == CheDoWaypoint.LapLai)
                {
                    chiSoTiep = 0;
                    hoanThanhMotVong = true;
                }
                else // PingPong
                {
                    huong = -1;
                    chiSoTiep = Mathf.Max(0, danhSachDiem.Length - 2);
                }
            }
            else if (chiSoTiep < 0)
            {
                huong = 1;
                chiSoTiep = Mathf.Min(1, danhSachDiem.Length - 1);
                hoanThanhMotVong = true;
            }

            yield return StartCoroutine(DiToi(danhSachDiem[chiSoTiep], tocDo));
            yield return new WaitForSeconds(delayTaiMoiWaypoint);
            chiSoHienTai = chiSoTiep;

            if (phaHuySauKhiDen && hoanThanhMotVong)
            {
                yield return StartCoroutine(PhaHuy());
                yield break;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  CHẾ ĐỘ: XOAY BAY
    // ─────────────────────────────────────────────────────────────

    /// <summary>Gọi từ BetrayingTrigger (cheDoTrigger = XoayBay)</summary>
    public void KichHoatXoayBay()
    {
        StopAllCoroutines();
        StartCoroutine(XoayBayCoroutine());
    }

    IEnumerator XoayBayCoroutine()
    {
        if (delayTruocKhiDi > 0) yield return new WaitForSeconds(delayTruocKhiDi);

        // Gắn player vào tường để xoay cùng
        if (playerTransform != null)
            playerTransform.SetParent(transform);

        // Xoay đến góc mục tiêu
        dangDiChuyen = true;
        yield return StartCoroutine(XoayMotLuot(gocXoayBay, tocDoXoayBay, huongXoay));
        dangDiChuyen = false;

        // Tách player — rơi tự do
        if (playerTransform != null)
        {
            playerTransform.SetParent(null);
            playerTransform = null;
        }

        // Xử lý sau khi đến góc
        switch (cheDoSauXoay)
        {
            case CheDoSauXoay.DungYen:
                // không làm gì thêm
                break;

            case CheDoSauXoay.TroVeGoc:
                if (delayTruocKhiTroVe > 0)
                    yield return new WaitForSeconds(delayTruocKhiTroVe);

                HuongXoay huongNguoc = huongXoay == HuongXoay.NguocKimDongHo
                    ? HuongXoay.XuoiKimDongHo
                    : HuongXoay.NguocKimDongHo;

                dangDiChuyen = true;
                float tocVe = tocDoXoayVe > 0 ? tocDoXoayVe : tocDoXoayBay;
                yield return StartCoroutine(XoayMotLuot(gocXoayBay, tocVe, huongNguoc));
                dangDiChuyen = false;
                break;

            case CheDoSauXoay.PhaHuy:
                yield return StartCoroutine(PhaHuy());
                break;

            case CheDoSauXoay.XoayVoHan:
                // tiếp tục xoay mãi cùng hướng, cùng tốc độ
                float daMoi = huongXoay == HuongXoay.NguocKimDongHo ? 1f : -1f;
                while (true)
                {
                    transform.Rotate(0, 0, daMoi * tocDoXoayBay * Time.deltaTime);
                    yield return null;
                }
        }
    }

    /// <summary>Xoay đúng |goc| độ theo hướng chỉ định, tốc độ (độ/giây)</summary>
    IEnumerator XoayMotLuot(float goc, float tocDoXoay, HuongXoay huong)
    {
        float mucTieu = Mathf.Abs(goc);
        float daMoi = huong == HuongXoay.NguocKimDongHo ? 1f : -1f;
        float daDi = 0f;

        while (daDi < mucTieu)
        {
            float buoc = Mathf.Min(tocDoXoay * Time.deltaTime, mucTieu - daDi);
            transform.Rotate(0, 0, daMoi * buoc);
            daDi += buoc;
            yield return null;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  CHUNG: DI CHUYỂN TỊNH TIẾN
    // ─────────────────────────────────────────────────────────────

    IEnumerator DiToi(Vector3 dich, float toc)
    {
        dangDiChuyen = true;
        while (Vector3.Distance(transform.position, dich) > 0.02f)
        {
            transform.position = Vector3.MoveTowards(transform.position, dich, toc * Time.deltaTime);
            yield return null;
        }
        transform.position = dich;
        dangDiChuyen = false;
    }

    // ─────────────────────────────────────────────────────────────
    //  CHUNG: PHÁ HỦY
    // ─────────────────────────────────────────────────────────────

    IEnumerator PhaHuy()
    {
        yield return new WaitForSeconds(delayPhaHuy);
        playerTransform = null;
        if (sr  != null) sr.enabled  = false;
        if (col != null) col.enabled = false;
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────────────────────────
    //  GIZMOS
    // ─────────────────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        Vector3 goc = Application.isPlaying ? viTriGoc : transform.position;

        if (cheDo == CheDoDidong.Waypoint && cacWaypoint != null)
        {
            for (int i = 0; i < cacWaypoint.Length; i++)
            {
                Vector3 diem = goc + new Vector3(cacWaypoint[i].x, cacWaypoint[i].y, 0);

                Gizmos.color = i == 0 ? Color.green
                             : i == cacWaypoint.Length - 1 ? Color.red
                             : Color.yellow;
                Gizmos.DrawWireSphere(diem, 0.18f);

                if (i < cacWaypoint.Length - 1)
                {
                    Vector3 diemTiep = goc + new Vector3(cacWaypoint[i + 1].x, cacWaypoint[i + 1].y, 0);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(diem, diemTiep);
                }

#if UNITY_EDITOR
                UnityEditor.Handles.Label(diem + Vector3.up * 0.3f, $"WP{i}");
#endif
            }

            if (cheDoWaypoint == CheDoWaypoint.LapLai && cacWaypoint.Length > 1)
            {
                Vector3 d0 = goc + new Vector3(cacWaypoint[0].x, cacWaypoint[0].y, 0);
                Vector3 dn = goc + new Vector3(cacWaypoint[^1].x, cacWaypoint[^1].y, 0);
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                Gizmos.DrawLine(dn, d0);
            }
        }
        else if (cheDo != CheDoDidong.XoayBay)
        {
            Vector3 den = goc + new Vector3(offsetDiemDen.x, offsetDiemDen.y, 0);
            Gizmos.color = Color.green;  Gizmos.DrawWireSphere(goc, 0.15f);
            Gizmos.color = Color.yellow; Gizmos.DrawLine(goc, den);
            Gizmos.color = Color.red;    Gizmos.DrawWireSphere(den, 0.15f);
        }
    }
}
