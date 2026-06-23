using UnityEngine;
using System.Collections;

public class BetrayingPlatform : MonoBehaviour
{
    public enum CheDoDidong
    {
        TuDong,
        ChoTrigger,
        TuDongMotChieu,
        Waypoint,
        XoayBay       // xoay đúng N độ rồi dừng → đẩy player xuống vực
    }

    public enum CheDoWaypoint
    {
        MotChieu,
        LapLai,
        PingPong
    }

    public enum HuongXoay
    {
        NguocKimDongHo,  // Z dương (+)
        XuoiKimDongHo    // Z âm  (-)
    }

    public enum CheDoSauXoay
    {
        OYenTaiCho,  // giữ nguyên góc sau khi xoay
        TroVeGoc     // xoay ngược về vị trí ban đầu
    }

    [Header("=== CHẾ ĐỘ ===")]
    public CheDoDidong cheDo = CheDoDidong.ChoTrigger;

    [Header("=== WAYPOINTS ===")]
    public Vector2[] cacWaypoint = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(0, 3),
        new Vector2(5, 3)
    };
    public CheDoWaypoint cheDoWaypoint = CheDoWaypoint.LapLai;
    public float delayTaiMoiWaypoint = 0.3f;

    [Header("=== ĐIỂM ĐẾN (không dùng Waypoint) ===")]
    public Vector2 offsetDiemDen = new Vector2(3f, 0f);

    [Header("=== TỐC ĐỘ ===")]
    public float tocDo = 3f;
    public float tocDoVe = 0f;

    [Header("=== DELAY ===")]
    public float delayTruocKhiDi = 0f;
    public float delayTaiDiem = 0.5f;

    [Header("=== XOAY (di chuyển) ===")]
    public bool coXoay = false;
    public float tocDoXoay = 90f;

    [Header("=== XOAY BAY (chế độ XoayBay) ===")]
    [Tooltip("Số độ cần xoay")]
    public float gocXoayBay = 90f;
    [Tooltip("Hướng xoay")]
    public HuongXoay huongXoay = HuongXoay.NguocKimDongHo;
    [Tooltip("Tốc độ xoay đi (độ/giây)")]
    public float tocDoXoayBay = 120f;
    [Tooltip("Delay trước khi bắt đầu xoay")]
    public float delayXoayBay = 0f;
    [Tooltip("Sau khi xoay xong thì làm gì")]
    public CheDoSauXoay cheDoSauXoay = CheDoSauXoay.OYenTaiCho;
    [Tooltip("Delay trước khi xoay về (chỉ dùng khi TroVeGoc)")]
    public float delayTruocKhiTroVe = 1f;
    [Tooltip("Tốc độ xoay về (chỉ dùng khi TroVeGoc)")]
    public float tocDoXoayVe = 120f;

    [Header("=== PHÁ HỦY ===")]
    public bool phaHuySauKhiDen = false;
    public float delayPhaHuy = 0.5f;

    [Header("=== VISUAL ===")]
    public bool nhayMayKhiSapDi = false;
    public float thoiGianNhayMay = 0.3f;
    public bool rungManHinhTruocKhiDi = false;
    public float thoiGianRung = 0.3f;
    public float doDungManHinh = 0.1f;

    [HideInInspector] public Vector3 viTriGoc;
    [HideInInspector] public Vector3 viTriDen;

    private Transform playerTransform;
    private Vector3 viTriPlatformTruoc;
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private bool dangDiChuyen = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        viTriGoc = transform.position;
        viTriDen = viTriGoc + new Vector3(offsetDiemDen.x, offsetDiemDen.y, 0);
        viTriPlatformTruoc = transform.position;

        if (cheDo == CheDoDidong.TuDong)
            StartCoroutine(TuDongLapLai());
        else if (cheDo == CheDoDidong.TuDongMotChieu)
            StartCoroutine(TuDongMotChieu());
        else if (cheDo == CheDoDidong.Waypoint)
            StartCoroutine(DiChuyenWaypoint());
        else if (cheDo == CheDoDidong.XoayBay)
            StartCoroutine(XoayBayTuDong());
    }

    void Update()
    {
        Vector3 delta = transform.position - viTriPlatformTruoc;
        if (playerTransform != null && delta != Vector3.zero)
            playerTransform.position += delta;
        viTriPlatformTruoc = transform.position;

        if (coXoay && dangDiChuyen && cheDo != CheDoDidong.XoayBay)
            transform.Rotate(0, 0, tocDoXoay * Time.deltaTime);
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

    // =============================================================
    // HELPER — chạy visual trước khi di chuyển
    // =============================================================
    IEnumerator ChayHieuUngTruocKhiDi()
    {
        if (nhayMayKhiSapDi)
            yield return StartCoroutine(HieuUngNhayMay());
        if (rungManHinhTruocKhiDi)
            yield return StartCoroutine(RungManHinh());
    }

    // =============================================================
    // TỰ ĐỘNG A→B→A
    // =============================================================
    IEnumerator TuDongLapLai()
    {
        if (delayTruocKhiDi > 0)
            yield return new WaitForSeconds(delayTruocKhiDi);

        while (true)
        {
            yield return StartCoroutine(ChayHieuUngTruocKhiDi());
            yield return StartCoroutine(DiToi(viTriDen, tocDo));
            yield return new WaitForSeconds(delayTaiDiem);

            float tocVe = tocDoVe > 0 ? tocDoVe : tocDo;
            yield return StartCoroutine(DiToi(viTriGoc, tocVe));
            yield return new WaitForSeconds(delayTaiDiem);

            if (phaHuySauKhiDen)
            {
                yield return StartCoroutine(PhaHuy());
                yield break;
            }
        }
    }

    // =============================================================
    // TỰ ĐỘNG MỘT CHIỀU
    // =============================================================
    IEnumerator TuDongMotChieu()
    {
        if (delayTruocKhiDi > 0)
            yield return new WaitForSeconds(delayTruocKhiDi);

        yield return StartCoroutine(ChayHieuUngTruocKhiDi());
        yield return StartCoroutine(DiToi(viTriDen, tocDo));

        if (phaHuySauKhiDen)
            yield return StartCoroutine(PhaHuy());
    }

    // =============================================================
    // KÍCH HOẠT TỪ TRIGGER — offset
    // =============================================================
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
        if (delayTruocKhiDi > 0)
            yield return new WaitForSeconds(delayTruocKhiDi);

        yield return StartCoroutine(ChayHieuUngTruocKhiDi());
        yield return StartCoroutine(DiToi(dich, toc));

        if (phaHuySauKhiDen)
            yield return StartCoroutine(PhaHuy());
    }

    // =============================================================
    // KÍCH HOẠT WAYPOINT TỪ TRIGGER
    // =============================================================
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

    // =============================================================
    // WAYPOINT — tự động
    // =============================================================
    IEnumerator DiChuyenWaypoint()
    {
        if (cacWaypoint == null || cacWaypoint.Length == 0) yield break;
        if (delayTruocKhiDi > 0)
            yield return new WaitForSeconds(delayTruocKhiDi);

        yield return StartCoroutine(ChayHieuUngTruocKhiDi());
        yield return StartCoroutine(ChayWaypoint(0));
    }

    // =============================================================
    // WAYPOINT — từ trigger
    // =============================================================
    IEnumerator DiChuyenWaypointTuIndex(int indexBatDau)
    {
        if (delayTruocKhiDi > 0)
            yield return new WaitForSeconds(delayTruocKhiDi);

        yield return StartCoroutine(ChayHieuUngTruocKhiDi());
        yield return StartCoroutine(ChayWaypoint(indexBatDau));
    }

    // =============================================================
    // LOGIC WAYPOINT CHUNG
    // =============================================================
    IEnumerator ChayWaypoint(int indexBatDau)
    {
        Vector3[] danhSachDiem = new Vector3[cacWaypoint.Length];
        for (int i = 0; i < cacWaypoint.Length; i++)
            danhSachDiem[i] = viTriGoc +
                new Vector3(cacWaypoint[i].x, cacWaypoint[i].y, 0);

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
                    if (phaHuySauKhiDen)
                        yield return StartCoroutine(PhaHuy());
                    yield break;
                }
                else if (cheDoWaypoint == CheDoWaypoint.LapLai)
                {
                    chiSoTiep = 0;
                    hoanThanhMotVong = true;
                }
                else if (cheDoWaypoint == CheDoWaypoint.PingPong)
                {
                    huong = -1;
                    chiSoTiep = danhSachDiem.Length - 2;
                    if (chiSoTiep < 0) chiSoTiep = 0;
                }
            }
            else if (chiSoTiep < 0)
            {
                huong = 1;
                chiSoTiep = 1;
                if (chiSoTiep >= danhSachDiem.Length)
                    chiSoTiep = 0;
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

    // =============================================================
    // DI CHUYỂN
    // =============================================================
    IEnumerator DiToi(Vector3 dich, float toc)
    {
        dangDiChuyen = true;
        while (Vector3.Distance(transform.position, dich) > 0.02f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, dich, toc * Time.deltaTime);
            yield return null;
        }
        transform.position = dich;
        dangDiChuyen = false;
    }

    // =============================================================
    // PHÁ HỦY
    // =============================================================
    IEnumerator PhaHuy()
    {
        yield return new WaitForSeconds(delayPhaHuy);
        playerTransform = null;
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;
        Destroy(gameObject);
    }

    // =============================================================
    // NHẤP NHÁY
    // =============================================================
    IEnumerator HieuUngNhayMay()
    {
        if (sr == null) yield break;
        Color mauGoc = sr.color;
        float moiLan = thoiGianNhayMay / 6f;
        for (int i = 0; i < 3; i++)
        {
            sr.color = new Color(mauGoc.r, mauGoc.g, mauGoc.b, 0.3f);
            yield return new WaitForSeconds(moiLan);
            sr.color = mauGoc;
            yield return new WaitForSeconds(moiLan);
        }
    }

    // =============================================================
    // RUNG MÀN HÌNH — route qua CameraController để tránh xung đột
    // =============================================================
    IEnumerator RungManHinh()
    {
        CameraController cc = Camera.main?.GetComponent<CameraController>();
        if (cc != null)
            yield return StartCoroutine(cc.CoroutineRung(thoiGianRung, doDungManHinh));
        else
        {
            // Fallback khi không có CameraController
            Camera cam = Camera.main;
            if (cam == null) yield break;
            Vector3 viTriGocCam = cam.transform.position;
            float daRung = 0f;
            while (daRung < thoiGianRung)
            {
                cam.transform.position = new Vector3(
                    viTriGocCam.x + Random.Range(-doDungManHinh, doDungManHinh),
                    viTriGocCam.y + Random.Range(-doDungManHinh, doDungManHinh),
                    viTriGocCam.z);
                daRung += Time.deltaTime;
                yield return null;
            }
            cam.transform.position = viTriGocCam;
        }
    }

    // =============================================================
    // XOAY BAY — tự động (khi cheDo = XoayBay)
    // =============================================================
    IEnumerator XoayBayTuDong()
    {
        if (delayXoayBay > 0)
            yield return new WaitForSeconds(delayXoayBay);
        yield return StartCoroutine(ThucHienXoayBay());
    }

    // =============================================================
    // XOAY BAY — kích hoạt từ trigger
    // =============================================================
    public void KichHoatXoayBay()
    {
        StopAllCoroutines();
        StartCoroutine(XoayBayTuDong());
    }

    // =============================================================
    // XOAY BAY — helper xoay một lượt góc
    // =============================================================
    IEnumerator XoayMotLuot(float goc, float tocDo)
    {
        float daMoi = huongXoay == HuongXoay.NguocKimDongHo ? 1f : -1f;
        float daDi = 0f;
        while (daDi < goc)
        {
            float buoc = Mathf.Min(tocDo * Time.deltaTime, goc - daDi);
            transform.Rotate(0, 0, daMoi * buoc);
            daDi += buoc;
            yield return null;
        }
    }

    // =============================================================
    // XOAY BAY — logic chính: xoay N độ, player đi theo, rồi ở yên / trở về
    // =============================================================
    IEnumerator ThucHienXoayBay()
    {
        yield return StartCoroutine(ChayHieuUngTruocKhiDi());

        // Gắn player vào tường để xoay cùng
        if (playerTransform != null)
            playerTransform.SetParent(transform);

        dangDiChuyen = true;
        yield return StartCoroutine(XoayMotLuot(gocXoayBay, tocDoXoayBay));
        dangDiChuyen = false;

        // Tách player — nếu ở đây player còn trên tường thì sẽ rơi theo vật lý
        if (playerTransform != null)
        {
            playerTransform.SetParent(null);
            playerTransform = null;
        }

        // --- Sau khi xoay xong ---
        if (cheDoSauXoay == CheDoSauXoay.TroVeGoc)
        {
            // Đợi rồi xoay ngược về
            if (delayTruocKhiTroVe > 0)
                yield return new WaitForSeconds(delayTruocKhiTroVe);

            // Đảo hướng để xoay về
            HuongXoay huongCu = huongXoay;
            huongXoay = huongXoay == HuongXoay.NguocKimDongHo
                ? HuongXoay.XuoiKimDongHo
                : HuongXoay.NguocKimDongHo;

            dangDiChuyen = true;
            yield return StartCoroutine(XoayMotLuot(gocXoayBay, tocDoXoayVe > 0 ? tocDoXoayVe : tocDoXoayBay));
            dangDiChuyen = false;

            huongXoay = huongCu; // khôi phục để có thể kích hoạt lại
        }

        if (phaHuySauKhiDen)
            yield return StartCoroutine(PhaHuy());
    }

    // =============================================================
    // GIZMOS
    // =============================================================
    void OnDrawGizmos()
    {
        Vector3 goc = Application.isPlaying ? viTriGoc : transform.position;

        if (cheDo == CheDoDidong.Waypoint && cacWaypoint != null)
        {
            for (int i = 0; i < cacWaypoint.Length; i++)
            {
                Vector3 diem = goc +
                    new Vector3(cacWaypoint[i].x, cacWaypoint[i].y, 0);

                Gizmos.color = i == 0 ? Color.green :
                               i == cacWaypoint.Length - 1 ? Color.red :
                               Color.yellow;
                Gizmos.DrawWireSphere(diem, 0.18f);

                if (i < cacWaypoint.Length - 1)
                {
                    Vector3 diemTiep = goc +
                        new Vector3(cacWaypoint[i + 1].x, cacWaypoint[i + 1].y, 0);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(diem, diemTiep);
                }

#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    diem + Vector3.up * 0.3f, $"WP{i}");
#endif
            }

            if (cheDoWaypoint == CheDoWaypoint.LapLai && cacWaypoint.Length > 1)
            {
                Vector3 d0 = goc + new Vector3(cacWaypoint[0].x, cacWaypoint[0].y, 0);
                Vector3 dn = goc + new Vector3(
                    cacWaypoint[cacWaypoint.Length - 1].x,
                    cacWaypoint[cacWaypoint.Length - 1].y, 0);
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                Gizmos.DrawLine(dn, d0);
            }
        }
        else
        {
            Vector3 den = goc + new Vector3(offsetDiemDen.x, offsetDiemDen.y, 0);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(goc, 0.15f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(goc, den);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(den, 0.15f);
        }
    }
}