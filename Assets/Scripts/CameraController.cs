using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public enum CheDoCamera
    {
        CoDinh,      // Camera đứng yên — mặc định
        TheoDoi,     // Camera follow player mượt
        GioiHan      // Follow nhưng bị giới hạn trong vùng
    }

    [Header("=== CHẾ ĐỘ ===")]
    public CheDoCamera cheDo = CheDoCamera.CoDinh;

    [Header("=== THEO DÕI (khi cheDo = TheoDoi hoặc GioiHan) ===")]
    public Transform player;
    public float tocDoTheoDoi = 5f;
    public Vector2 offset = new Vector2(0f, 1f);

    [Header("=== GIỚI HẠN VÙNG (khi cheDo = GioiHan) ===")]
    public float gioiHanTrai = -10f;
    public float gioiHanPhai = 10f;
    public float gioiHanDuoi = -5f;
    public float gioiHanTren = 5f;

    [Header("=== DEAD ZONE (vùng player đi mà camera không di chuyển) ===")]
    public Vector2 deadZone = new Vector2(1f, 0.5f);

    private Vector3 viTriMucTieu;
    private float zGoc;

    void Start()
    {
        zGoc = transform.position.z;

        // Tự tìm player nếu chưa gán
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        viTriMucTieu = transform.position;
    }

    void LateUpdate()
    {
        if (cheDo == CheDoCamera.CoDinh) return;
        if (player == null) return;

        Vector3 viTriPlayer = player.position;
        Vector3 viTriCamHienTai = transform.position;

        // Tính vị trí mục tiêu với dead zone
        float mucTieuX = viTriMucTieu.x;
        float mucTieuY = viTriMucTieu.y;

        // Dead zone X
        float diffX = viTriPlayer.x + offset.x - viTriMucTieu.x;
        if (Mathf.Abs(diffX) > deadZone.x)
            mucTieuX = viTriPlayer.x + offset.x
                - Mathf.Sign(diffX) * deadZone.x;

        // Dead zone Y
        float diffY = viTriPlayer.y + offset.y - viTriMucTieu.y;
        if (Mathf.Abs(diffY) > deadZone.y)
            mucTieuY = viTriPlayer.y + offset.y
                - Mathf.Sign(diffY) * deadZone.y;

        viTriMucTieu = new Vector3(mucTieuX, mucTieuY, zGoc);

        // Giới hạn vùng
        if (cheDo == CheDoCamera.GioiHan)
        {
            float halfH = Camera.main.orthographicSize;
            float halfW = halfH * Camera.main.aspect;

            viTriMucTieu.x = Mathf.Clamp(viTriMucTieu.x,
                gioiHanTrai + halfW, gioiHanPhai - halfW);
            viTriMucTieu.y = Mathf.Clamp(viTriMucTieu.y,
                gioiHanDuoi + halfH, gioiHanTren - halfH);
        }

        // Smooth follow
        transform.position = Vector3.Lerp(
            viTriCamHienTai, viTriMucTieu,
            tocDoTheoDoi * Time.deltaTime);
    }

    // =============================================================
    // CAMERA SHAKE — tập trung tại đây, tránh xung đột
    // =============================================================
    private bool dangRung = false;

    public static void RungManHinhStatic(float thoiGian, float doDung)
    {
        CameraController cc = Camera.main?.GetComponent<CameraController>();
        if (cc != null)
            cc.StartCoroutine(cc.CoroutineRung(thoiGian, doDung));
        else
        {
            // Fallback khi không có CameraController — rung trực tiếp
            Camera cam = Camera.main;
            if (cam != null)
            {
                // Không thể dùng coroutine static, skip
                Debug.LogWarning("CameraController: không tìm thấy để rung màn hình");
            }
        }
    }

    public IEnumerator CoroutineRung(float thoiGian, float doDung)
    {
        if (dangRung) yield break; // Tránh chồng chất nhiều shake
        dangRung = true;

        float daRung = 0f;
        while (daRung < thoiGian)
        {
            // Thêm offset vào viTriMucTieu thay vì thao tác transform trực tiếp
            viTriMucTieu += new Vector3(
                Random.Range(-doDung, doDung),
                Random.Range(-doDung, doDung),
                0f);
            daRung += Time.unscaledDeltaTime;
            yield return null;
        }

        dangRung = false;
    }

    void OnDrawGizmos()
    {
        if (cheDo != CheDoCamera.GioiHan) return;

        Gizmos.color = new Color(0.96f, 0.78f, 0.26f, 0.3f);
        Vector3 tam = new Vector3(
            (gioiHanTrai + gioiHanPhai) * 0.5f,
            (gioiHanDuoi + gioiHanTren) * 0.5f, 0);
        Vector3 size = new Vector3(
            gioiHanPhai - gioiHanTrai,
            gioiHanTren - gioiHanDuoi, 0);
        Gizmos.DrawWireCube(tam, size);
    }
}