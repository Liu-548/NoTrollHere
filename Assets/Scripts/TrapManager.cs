using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapManager : MonoBehaviour
{
    public enum CheDoSauKhiKichHoat
    {
        DichChuyen,
        PhaHuy,
        DichRoiPha
    }

    [Header("=== TRIGGER ===")]
    public bool tuDongTimTrigger = true;

    [Header("=== SAU KHI KÍCH HOẠT ===")]
    public CheDoSauKhiKichHoat cheDo = CheDoSauKhiKichHoat.DichChuyen;

    [Header("=== DỊCH CHUYỂN ===")]
    public Vector2 offsetDiemDen = new Vector2(0f, -10f);
    public float tocDo = 8f;
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

        Debug.Log($"TrapManager: tìm thấy {cacBayConScript.Count} bẫy con");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
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

        if (cheDo == CheDoSauKhiKichHoat.PhaHuy)
            yield return StartCoroutine(PhaHuyTatCa());
        else if (cheDo == CheDoSauKhiKichHoat.DichChuyen)
            yield return StartCoroutine(DichChuyenToanBo());
        else if (cheDo == CheDoSauKhiKichHoat.DichRoiPha)
        {
            yield return StartCoroutine(DichChuyenToanBo());
            yield return new WaitForSeconds(delayPhaHuy);
            yield return StartCoroutine(PhaHuyTatCa());
        }
    }

    void KichHoatTatCaBayCon()
    {
        foreach (MonoBehaviour script in cacBayConScript)
        {
            if (script == null) continue;

            if (script is HiddenSpike hs)
                hs.SendMessage("MocLen", SendMessageOptions.DontRequireReceiver);
            else if (script is WallSpike ws)
                ws.SendMessage("Ban", SendMessageOptions.DontRequireReceiver);
            else if (script is FallingBrick fb)
                fb.KichHoatRoi();
            else if (script is BetrayingPlatform bp)
                bp.KichHoat(offsetDiemDen, tocDo);
        }
    }

    IEnumerator DichChuyenToanBo()
    {
        Vector3 diemDen = viTriGoc +
            new Vector3(offsetDiemDen.x, offsetDiemDen.y, 0);

        while (Vector3.Distance(transform.position, diemDen) > 0.02f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, diemDen, tocDo * Time.deltaTime);
            yield return null;
        }
        transform.position = diemDen;
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
        Vector3 den = goc + new Vector3(offsetDiemDen.x, offsetDiemDen.y, 0);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(goc, new Vector3(1f, 1f, 0));
        Gizmos.DrawLine(goc, den);
        Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
        Gizmos.DrawWireCube(den, new Vector3(1f, 1f, 0));

#if UNITY_EDITOR
        UnityEditor.Handles.Label(den + Vector3.up * 0.4f,
            $"→ ({offsetDiemDen.x}, {offsetDiemDen.y})");
#endif
    }
}