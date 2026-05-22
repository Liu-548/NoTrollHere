using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapManager : MonoBehaviour
{
    public enum CheDoSauKhiKichHoat
    {
        DiChuyen,      // Di chuyển dần đến điểm đến
        Teleport,      // Teleport ngay lập tức
        PhaHuy,        // Phá hủy toàn bộ ngay
        DiRoiPha,      // Di chuyển xong rồi phá hủy
        TeleportRoiPha // Teleport xong rồi phá hủy
    }

    [Header("=== SAU KHI KÍCH HOẠT ===")]
    public CheDoSauKhiKichHoat cheDo = CheDoSauKhiKichHoat.Teleport;

    [Header("=== DI CHUYỂN / TELEPORT ===")]
    // Hướng và quãng đường di chuyển
    public Vector2 huongDiChuyen = new Vector2(0f, -1f);
    // Ví dụ: (0,-1) = xuống, (1,0) = phải, (-1,0) = trái
    public float quangDuong = 10f;
    // Khoảng cách di chuyển (units)
    public float tocDoDiChuyen = 8f;
    // Tốc độ (chỉ dùng khi cheDo = DiChuyen)
    public float delayTruocKhiDi = 0f;

    [Header("=== PHÁ HỦY ===")]
    public float delayPhaHuy = 0.3f;

    [Header("=== VISUAL ===")]
    public bool rungManHinhKhiKichHoat = false;
    public float thoiGianRung = 0.2f;
    public float doDung = 0.1f;

    [Header("=== CÀI ĐẶT ===")]
    public bool chiKichHoatMotLan = true;

    private List<MonoBehaviour> cacBayConScript = new List<MonoBehaviour>();
    private bool daKichHoat = false;
    private Vector3 viTriGoc;

    // Tính điểm đến từ hướng + quãng đường
    Vector3 DiemDen => viTriGoc +
        new Vector3(
            huongDiChuyen.normalized.x * quangDuong,
            huongDiChuyen.normalized.y * quangDuong,
            0);

    void Start()
    {
        viTriGoc = transform.position;
        TimCacBayCon();
    }

    void TimCacBayCon()
    {
        cacBayConScript.Clear();

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child == transform) continue;

            HiddenSpike hs = child.GetComponent<HiddenSpike>();
            if (hs != null) cacBayConScript.Add(hs);

            WallSpike ws = child.GetComponent<WallSpike>();
            if (ws != null) cacBayConScript.Add(ws);

            FallingBrick fb = child.GetComponent<FallingBrick>();
            if (fb != null) cacBayConScript.Add(fb);

            SpringTrap st = child.GetComponent<SpringTrap>();
            if (st != null) cacBayConScript.Add(st);

            BetrayingPlatform bp = child.GetComponent<BetrayingPlatform>();
            if (bp != null) cacBayConScript.Add(bp);
        }

        Debug.Log($"TrapManager: {cacBayConScript.Count} bẫy con");
    }

    // Gọi từ TrapManagerTrigger
    public void KichHoatTuTrigger()
    {
        if (chiKichHoatMotLan && daKichHoat) return;
        daKichHoat = true;
        StartCoroutine(KichHoat());
    }

    IEnumerator KichHoat()
    {
        if (rungManHinhKhiKichHoat)
            yield return StartCoroutine(RungManHinh());

        KichHoatTatCaBayCon();

        if (delayTruocKhiDi > 0)
            yield return new WaitForSeconds(delayTruocKhiDi);

        switch (cheDo)
        {
            case CheDoSauKhiKichHoat.DiChuyen:
                yield return StartCoroutine(DiChuyenDan());
                break;

            case CheDoSauKhiKichHoat.Teleport:
                TeleportNgay();
                break;

            case CheDoSauKhiKichHoat.PhaHuy:
                yield return StartCoroutine(PhaHuyTatCa());
                break;

            case CheDoSauKhiKichHoat.DiRoiPha:
                yield return StartCoroutine(DiChuyenDan());
                yield return new WaitForSeconds(delayPhaHuy);
                yield return StartCoroutine(PhaHuyTatCa());
                break;

            case CheDoSauKhiKichHoat.TeleportRoiPha:
                TeleportNgay();
                yield return new WaitForSeconds(delayPhaHuy);
                yield return StartCoroutine(PhaHuyTatCa());
                break;
        }
    }

    void KichHoatTatCaBayCon()
    {
        foreach (MonoBehaviour script in cacBayConScript)
        {
            if (script == null) continue;

            if (script is HiddenSpike hs)
                hs.SendMessage("MocLen",
                    SendMessageOptions.DontRequireReceiver);
            else if (script is WallSpike ws)
                ws.SendMessage("Ban",
                    SendMessageOptions.DontRequireReceiver);
            else if (script is FallingBrick fb)
                fb.KichHoatRoi();
            else if (script is BetrayingPlatform bp)
            {
                // Dùng offset từ hướng + quãng đường
                Vector2 offset = huongDiChuyen.normalized * quangDuong;
                bp.KichHoat(offset, tocDoDiChuyen);
            }
        }
    }

    // Di chuyển dần — cha kéo con theo
    IEnumerator DiChuyenDan()
    {
        Vector3 diemDen = DiemDen;

        while (Vector3.Distance(transform.position, diemDen) > 0.02f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, diemDen,
                tocDoDiChuyen * Time.deltaTime);
            yield return null;
        }
        transform.position = diemDen;
    }

    // Teleport ngay lập tức
    void TeleportNgay()
    {
        transform.position = DiemDen;
    }

    IEnumerator PhaHuyTatCa()
    {
        foreach (SpriteRenderer sr in
            GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;

        foreach (Collider2D col in
            GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    IEnumerator RungManHinh()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 goc = cam.transform.position;
        float daRung = 0f;

        while (daRung < thoiGianRung)
        {
            cam.transform.position = new Vector3(
                goc.x + Random.Range(-doDung, doDung),
                goc.y + Random.Range(-doDung, doDung),
                goc.z);
            daRung += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = goc;
    }

    void OnDrawGizmos()
    {
        Vector3 goc = Application.isPlaying ? viTriGoc : transform.position;
        Vector3 den = goc + new Vector3(
            huongDiChuyen.normalized.x * quangDuong,
            huongDiChuyen.normalized.y * quangDuong, 0);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(goc, new Vector3(1f, 1f, 0));
        Gizmos.DrawLine(goc, den);
        Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
        Gizmos.DrawWireCube(den, new Vector3(1f, 1f, 0));

#if UNITY_EDITOR
        string loai = cheDo == CheDoSauKhiKichHoat.Teleport ||
                      cheDo == CheDoSauKhiKichHoat.TeleportRoiPha
                      ? "⚡ teleport" : "→ di chuyển";
        UnityEditor.Handles.Label(den + Vector3.up * 0.4f,
            $"{loai} ({huongDiChuyen.normalized.x * quangDuong:F1}, " +
            $"{huongDiChuyen.normalized.y * quangDuong:F1})");
#endif
    }
}