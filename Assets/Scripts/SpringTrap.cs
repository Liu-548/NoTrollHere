using UnityEngine;

public class SpringTrap : MonoBehaviour
{
    [Header("=== LỰC BẮN ===")]
    public float lucBan = 25f;

    [Header("=== HƯỚNG BẮN ===")]
    // Tự động tính từ rotation của object
    // Xoay object 0°   = bắn lên
    // Xoay object 90°  = bắn sang phải
    // Xoay object -90° = bắn sang trái
    // Xoay object 180° = bắn xuống
    public bool tuDongTheoHuong = true;
    // Nếu tắt → dùng huongBan thủ công
    public Vector2 huongBan = Vector2.up;

    [Header("=== ANIMATION ===")]
    public float thoiGianNen = 0.05f;
    public float thoiGianGian = 0.15f;

    [Header("=== SFX ===")]
    public bool phatSFX = true;

    private Animator animator;
    private bool dangNhay = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Tính hướng bắn dựa theo rotation của object
    Vector2 LayHuongBan()
    {
        if (!tuDongTheoHuong)
            return huongBan.normalized;

        // Transform.up = hướng "lên" của object sau khi xoay
        // Object xoay 90° → up trỏ sang phải → bắn phải
        return (Vector2)transform.up;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (dangNhay) return;
        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Kiểm tra player đang đi vào lò xo
        // Dot product âm = player đang di chuyển ngược chiều bắn
        Vector2 huong = LayHuongBan();
        float dot = Vector2.Dot(rb.linearVelocity.normalized, huong);
        if (dot > 0.8f) return; // Player đang đi cùng chiều = bỏ qua

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null) pc.NayLoXo(lucBan, huong);

        if (phatSFX && SoundManager.instance != null)
            SoundManager.instance.PlayNhay();

        dangNhay = true;

        if (animator != null)
            animator.SetBool("IsPressed", true);

        StartCoroutine(AnimationLoXo());
    }

    System.Collections.IEnumerator AnimationLoXo()
    {
        yield return new WaitForSeconds(thoiGianNen);

        if (animator != null)
            animator.SetBool("IsPressed", false);

        yield return new WaitForSeconds(thoiGianGian + 0.3f);
        dangNhay = false;
    }

    void OnDrawGizmos()
    {
        Vector2 huong = tuDongTheoHuong
            ? (Vector2)transform.up
            : huongBan.normalized;

        // Vẽ mũi tên hướng bắn
        Gizmos.color = Color.yellow;
        Vector3 diemCuoi = transform.position +
            (Vector3)(huong * lucBan * 0.1f);
        Gizmos.DrawLine(transform.position, diemCuoi);
        Gizmos.DrawWireSphere(diemCuoi, 0.15f);
    }
}