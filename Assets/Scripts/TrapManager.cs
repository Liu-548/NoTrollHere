using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrapManager : MonoBehaviour
{
    public enum CheDoSauKhiKichHoat
    {
        DiChuyen,
        Teleport,
        PhaHuy,
        DiRoiPha,
        TeleportRoiPha
    }

    [Header("=== SAU KHI KÍCH HOẠT ===")]
    public CheDoSauKhiKichHoat cheDo = CheDoSauKhiKichHoat.Teleport;

    [Header("=== DI CHUYỂN / TELEPORT ===")]
    public Vector2 huongDiChuyen = new Vector2(0f, -1f);
    public float quangDuong = 10f;
    public float tocDoDiChuyen = 8f;
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

    // Lưu vị trí gốc của từng con
    private List<Transform> cacConTransform = new List<Transform>();
    private List<Vector3> cacViTriGocCon = new List<Vector3>();

    private bool daKichHoat = false;
    private Vector3 viTriGocCha;

    // Điểm đến của CHA — dùng để di chuyển cha
    Vector3 DiemDenCha => viTriGocCha + new Vector3(
        huongDiChuyen.normalized.x * quangDuong,
        huongDiChuyen.normalized.y * quangDuong,
        0);

    // Điểm đến của CON theo index — vị trí gốc con + offset
    Vector3 DiemDenCon(int index) => cacViTriGocCon[index] + new Vector3(
        huongDiChuyen.normalized.x * quangDuong,
        huongDiChuyen.normalized.y * quangDuong,
        0);

    void Start()
    {
        viTriGocCha = transform.position;
        TimCacBayCon();
    }

    void TimCacBayCon()
    {
        cacBayConScript.Clear();
        cacConTransform.Clear();
        cacViTriGocCon.Clear();

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child == transform) continue;

            // Lưu transform và vị trí gốc của mọi child
            if (!cacConTransform.Contains(child))
            {
                cacConTransform.Add(child);
                cacViTriGocCon.Add(child.position); // World position
            }

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

        Debug.Log($"TrapManager: {cacBayConScript.Count} bẫy con," +
                  $" {cacConTransform.Count} transform con");
    }

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

        if (delayTruocKhiDi > 0)
            yield return new WaitForSeconds(delayTruocKhiDi);

        switch (cheDo)
        {
            case CheDoSauKhiKichHoat.DiChuyen:
                // Kích hoạt hành vi bẫy con (ví dụ HiddenSpike mọc lên trong lúc di chuyển)
                KichHoatTatCaBayCon();
                yield return StartCoroutine(DiChuyenDan());
                break;

            case CheDoSauKhiKichHoat.Teleport:
                // Chỉ dịch chuyển position — bẫy con tự kích hoạt bằng trigger riêng khi player chạm
                TeleportNgay();
                break;

            case CheDoSauKhiKichHoat.PhaHuy:
                KichHoatTatCaBayCon();
                yield return StartCoroutine(PhaHuyTatCa());
                break;

            case CheDoSauKhiKichHoat.DiRoiPha:
                KichHoatTatCaBayCon();
                yield return StartCoroutine(DiChuyenDan());
                yield return new WaitForSeconds(delayPhaHuy);
                yield return StartCoroutine(PhaHuyTatCa());
                break;

            case CheDoSauKhiKichHoat.TeleportRoiPha:
                // Teleport trước, sau đó phá hủy — cũng không kích hoạt hành vi con
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
                hs.KichHoat();
            else if (script is WallSpike ws)
                ws.KichHoatBan();
            else if (script is FallingBrick fb)
                fb.KichHoatRoi();
            else if (script is BetrayingPlatform bp)
            {
                Vector2 offset = huongDiChuyen.normalized * quangDuong;
                bp.KichHoat(offset, tocDoDiChuyen);
            }
        }
    }

    // Di chuyển dần — tính từ vị trí gốc của TỪNG CON
    IEnumerator DiChuyenDan()
    {
        // Tách con ra khỏi cha để di chuyển độc lập
        List<Transform> cacConHopLe = new List<Transform>();
        for (int i = 0; i < cacConTransform.Count; i++)
        {
            if (cacConTransform[i] == null) continue;
            cacConTransform[i].SetParent(null); // Tách khỏi cha
            cacConHopLe.Add(cacConTransform[i]);
        }

        // Di chuyển từng con đến đúng điểm đến của nó
        bool chuaXong = true;
        while (chuaXong)
        {
            chuaXong = false;
            for (int i = 0; i < cacConHopLe.Count; i++)
            {
                if (cacConHopLe[i] == null) continue;

                Vector3 diemDen = cacViTriGocCon[
                    cacConTransform.IndexOf(cacConHopLe[i])] +
                    new Vector3(
                        huongDiChuyen.normalized.x * quangDuong,
                        huongDiChuyen.normalized.y * quangDuong,
                        0);

                if (Vector3.Distance(
                    cacConHopLe[i].position, diemDen) > 0.02f)
                {
                    cacConHopLe[i].position = Vector3.MoveTowards(
                        cacConHopLe[i].position,
                        diemDen,
                        tocDoDiChuyen * Time.deltaTime);
                    chuaXong = true;
                }
                else
                {
                    cacConHopLe[i].position = diemDen;
                }
            }
            yield return null;
        }

        // Gắn lại vào cha sau khi di chuyển xong
        foreach (Transform con in cacConHopLe)
            if (con != null) con.SetParent(transform);
    }

    // Teleport từng con đến đúng vị trí của nó
    void TeleportNgay()
    {
        for (int i = 0; i < cacConTransform.Count; i++)
        {
            if (cacConTransform[i] == null) continue;
            cacConTransform[i].position = DiemDenCon(i);
        }

        // Di chuyển cha đến vị trí tương ứng
        transform.position = DiemDenCha;
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
        CameraController cc = Camera.main?.GetComponent<CameraController>();
        if (cc != null)
            yield return StartCoroutine(cc.CoroutineRung(thoiGianRung, doDung));
        else
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
    }

    void OnDrawGizmos()
    {
        // Vẽ gizmos cho từng con
        if (Application.isPlaying)
        {
            for (int i = 0; i < cacConTransform.Count; i++)
            {
                if (cacConTransform[i] == null) continue;
                Vector3 goc = cacViTriGocCon[i];
                Vector3 den = DiemDenCon(i);

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(goc, 0.2f);
                Gizmos.DrawLine(goc, den);
                Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
                Gizmos.DrawWireSphere(den, 0.2f);
            }
        }
        else
        {
            // Trong Editor — vẽ từ vị trí con hiện tại
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child == transform) continue;
                Vector3 goc = child.position;
                Vector3 den = goc + new Vector3(
                    huongDiChuyen.normalized.x * quangDuong,
                    huongDiChuyen.normalized.y * quangDuong,
                    0);

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(goc, 0.15f);
                Gizmos.DrawLine(goc, den);
                Gizmos.color = new Color(1f, 0f, 1f, 0.4f);
                Gizmos.DrawWireSphere(den, 0.15f);

#if UNITY_EDITOR
                string loai = cheDo == CheDoSauKhiKichHoat.Teleport ||
                              cheDo == CheDoSauKhiKichHoat.TeleportRoiPha
                              ? "⚡" : "→";
                UnityEditor.Handles.Label(den + Vector3.up * 0.3f,
                    $"{loai} {child.name}");
#endif
            }
        }
    }
}