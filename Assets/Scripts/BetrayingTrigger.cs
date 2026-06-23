using UnityEngine;

public class BetrayingTrigger : MonoBehaviour
{
    public enum CheDoTrigger
    {
        DiDenOffset,   // Trigger đẩy platform đến 1 điểm offset
        BatDauWaypoint, // Trigger kích hoạt chạy waypoint
        XoayBay        // Trigger kích hoạt xoay N độ rồi đẩy player xuống vực
    }

    [Header("=== CHẾ ĐỘ TRIGGER ===")]
    public CheDoTrigger cheDoTrigger = CheDoTrigger.DiDenOffset;

    [Header("=== DI ĐẾN OFFSET (khi cheDoTrigger = DiDenOffset) ===")]
    public Vector2 offset;
    public float tocDo = 0f;

    [Header("=== WAYPOINT (khi cheDoTrigger = BatDauWaypoint) ===")]
    // Bắt đầu từ waypoint nào (mặc định 0)
    public int batDauTuWaypoint = 0;

    [Header("=== CÀI ĐẶT CHUNG ===")]
    // Kích hoạt lại được không hay chỉ 1 lần
    public bool chiKichHoatMotLan = true;

    private BetrayingPlatform platform;
    private bool daKichHoat = false;

    void Start()
    {
        platform = GetComponentInParent<BetrayingPlatform>();
        if (platform == null)
            Debug.LogError(gameObject.name +
                ": Không tìm thấy BetrayingPlatform ở object cha!");
    }

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (!vatTheChamVao.CompareTag("Player")) return;
        if (chiKichHoatMotLan && daKichHoat) return;

        daKichHoat = true;

        if (cheDoTrigger == CheDoTrigger.DiDenOffset)
            platform.KichHoat(offset, tocDo);
        else if (cheDoTrigger == CheDoTrigger.BatDauWaypoint)
            platform.KichHoatWaypoint(batDauTuWaypoint);
        else if (cheDoTrigger == CheDoTrigger.XoayBay)
            platform.KichHoatXoayBay();
    }

    void OnDrawGizmos()
    {
        if (platform == null)
            platform = GetComponentInParent<BetrayingPlatform>();
        if (platform == null) return;

        if (cheDoTrigger == CheDoTrigger.DiDenOffset)
        {
            Vector3 diemDen = platform.transform.position +
                new Vector3(offset.x, offset.y, 0);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(diemDen, 0.12f);
            Gizmos.DrawLine(transform.position, diemDen);
        }
        else if (cheDoTrigger == CheDoTrigger.BatDauWaypoint)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.12f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.3f,
                $"→ WP{batDauTuWaypoint}");
#endif
        }
        else if (cheDoTrigger == CheDoTrigger.XoayBay)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.12f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.3f, "⟳ XoayBay");
#endif
        }
    }
}