using UnityEngine;

public class BetrayingTrigger : MonoBehaviour
{
    [Header("=== ĐIỂM ĐẾN (offset so với vị trí gốc platform) ===")]
    public Vector2 offset;
    // Ví dụ: (0, -10) = lao xuống 10 đơn vị
    // Ví dụ: (5, 0)   = lao sang phải 5 đơn vị

    [Header("=== TỐC ĐỘ (0 = dùng mặc định của platform) ===")]
    public float tocDo = 0f;

    private BetrayingPlatform platform;
    private bool daKichHoat = false;

    void Start()
    {
        // Tự tìm platform từ object cha
        platform = GetComponentInParent<BetrayingPlatform>();

        if (platform == null)
            Debug.LogError(gameObject.name +
                ": Không tìm thấy BetrayingPlatform ở object cha!");
    }

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (!vatTheChamVao.CompareTag("Player")) return;
        if (daKichHoat) return;

        daKichHoat = true;
        platform.KichHoat(offset, tocDo);
    }

    void OnDrawGizmos()
    {
        if (platform == null)
            platform = GetComponentInParent<BetrayingPlatform>();
        if (platform == null) return;

        Vector3 diemDen = platform.transform.position +
            new Vector3(offset.x, offset.y, 0);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(diemDen, 0.12f);
        Gizmos.DrawLine(transform.position, diemDen);
    }
}