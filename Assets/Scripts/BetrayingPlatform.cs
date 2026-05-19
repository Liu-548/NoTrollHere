using UnityEngine;
using System.Collections;

public class BetrayingPlatform : MonoBehaviour
{
    [Header("=== DI CHUYỂN ===")]
    public bool diChuyenKhiBatDau = false;
    // true  = tự đi ngay khi game bắt đầu, lặp lại mãi
    // false = đứng yên, chờ trigger

    public Vector2 offsetDiemDen = new Vector2(3f, 0f);
    // Điểm đến = vị trí hiện tại + offset này

    public float tocDo = 3f;

    [Header("=== PHÁ HỦY ===")]
    public bool phaHuySauKhiDen = false;
    // true = platform biến mất sau khi đến điểm đến

    public float delayPhaHuy = 0.5f;
    // Đợi bao lâu rồi mới phá hủy (giây)

    // Biến chia sẻ với BetrayingTrigger
    [HideInInspector] public Vector3 viTriGoc;

    // Biến nội bộ
    private Vector3 viTriDen;
    private Transform playerTransform;
    private Vector3 viTriPlatformTruoc;
    private SpriteRenderer sr;
    private BoxCollider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        viTriGoc = transform.position;
        viTriDen = viTriGoc + new Vector3(offsetDiemDen.x, offsetDiemDen.y, 0);
        viTriPlatformTruoc = transform.position;

        if (diChuyenKhiBatDau)
            StartCoroutine(LapLai());
    }

    void Update()
    {
        // Kéo player đi theo platform
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

    // =============================================================
    // CHẾ ĐỘ TỰ ĐI — lặp lại mãi
    // =============================================================
    IEnumerator LapLai()
    {
        while (true)
        {
            yield return StartCoroutine(DiToi(viTriDen, tocDo));
            yield return StartCoroutine(DiToi(viTriGoc, tocDo));
        }
    }

    // =============================================================
    // KÍCH HOẠT TỪ TRIGGER — gọi từ BetrayingTrigger
    // =============================================================
    public void KichHoat(Vector2 offset, float tocDoOverride)
    {
        Vector3 diemDen = viTriGoc + new Vector3(offset.x, offset.y, 0);
        float toc = tocDoOverride > 0 ? tocDoOverride : tocDo;

        StopAllCoroutines();
        StartCoroutine(DiToiRoiXuLy(diemDen, toc));
    }

    IEnumerator DiToiRoiXuLy(Vector3 dich, float toc)
    {
        // Di chuyển đến điểm đích
        while (Vector3.Distance(transform.position, dich) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, dich, toc * Time.deltaTime);
            yield return null;
        }
        transform.position = dich;

        // Nếu không phá hủy → dừng hẳn tại đây
        if (!phaHuySauKhiDen) yield break;

        // Đợi delay rồi phá hủy
        yield return new WaitForSeconds(delayPhaHuy);

        // Thả player ra trước khi biến mất
        playerTransform = null;

        // Ẩn platform
        sr.enabled = false;
        col.enabled = false;

        // Phá hủy object
        Destroy(gameObject);
    }

    IEnumerator DiToi(Vector3 dich, float toc)
    {
        while (Vector3.Distance(transform.position, dich) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, dich, toc * Time.deltaTime);
            yield return null;
        }
        transform.position = dich;
    }

    void OnDrawGizmos()
    {
        Vector3 den = transform.position +
            new Vector3(offsetDiemDen.x, offsetDiemDen.y, 0);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
        Gizmos.DrawLine(transform.position, den);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(den, 0.15f);
    }
}