using UnityEngine;

/// <summary>
/// Đảo chiều trọng lực khi được kích hoạt.
/// Gán vào cùng GameObject với PlayerController.
/// Gắn KichHoat() vào onWPressed / onSpacePressed (bật wGanScript / spaceGanScript = true).
/// </summary>
public class GravityFlipper : MonoBehaviour
{
    [Header("=== ĐẢO TRỌNG LỰC ===")]
    [Tooltip("Trạng thái hiện tại — xem trực tiếp trên Inspector khi chạy game.")]
    public bool dangDaoChieu = false;

    [Tooltip("Bật = chỉ đảo chiều khi nhân vật đang đứng trên mặt đất.")]
    public bool yeuCauMatDat = false;

    [Header("=== THAM CHIẾU ===")]
    [Tooltip("Kéo GameObject Player vào đây. Để trống sẽ tự tìm theo tag 'Player'.")]
    public GameObject player;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerController playerController;
    private Collider2D col;

    void Start()
    {
        // Ưu tiên reference thủ công; nếu không có thì tìm theo tag
        if (player == null)
            player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            rb               = player.GetComponent<Rigidbody2D>();
            sr               = player.GetComponent<SpriteRenderer>();
            playerController = player.GetComponent<PlayerController>();
            col              = player.GetComponent<Collider2D>();
        }

        if (rb == null) Debug.LogError("[GravityFlipper] Không tìm thấy Rigidbody2D! Kéo Player vào field 'player' hoặc đặt tag 'Player'.", this);
        if (sr == null) Debug.LogError("[GravityFlipper] Không tìm thấy SpriteRenderer! Kéo Player vào field 'player' hoặc đặt tag 'Player'.", this);
    }

    /// <summary>
    /// Gọi từ UnityEvent (onWPressed / onSpacePressed).
    /// Mỗi lần gọi đảo ngược chiều trọng lực + lật sprite.
    /// </summary>
    public void KichHoat()
    {
        if (rb == null || sr == null) return;

        // Nếu yêu cầu mặt đất:
        // - Trọng lực bình thường → phải đứng trên sàn (normal.y > 0)
        // - Đang đảo chiều       → phải đứng trên trần (normal.y < 0)
        if (yeuCauMatDat)
        {
            bool hopLe = dangDaoChieu ? DangDungTrenTran() : playerController != null && playerController.dangDungTrenDat;
            if (!hopLe) return;
        }

        dangDaoChieu = !dangDaoChieu;

        // Lật sprite theo trục Y để nhân vật úp ngược đúng chiều rơi
        sr.flipY = dangDaoChieu;
    }

    // Kiểm tra nhân vật đang chạm trần (contact normal hướng xuống)
    bool DangDungTrenTran()
    {
        if (col == null) return false;
        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int count = col.GetContacts(contacts);
        for (int i = 0; i < count; i++)
            if (contacts[i].normal.y < -0.5f) return true;
        return false;
    }

    void LateUpdate()
    {
        if (rb == null) return;

        // PlayerController đặt gravityScale (luôn dương) trong Update().
        // LateUpdate chạy sau đó → đảm bảo gravityScale đúng chiều mỗi frame.
        if (dangDaoChieu && rb.gravityScale > 0)
            rb.gravityScale = -rb.gravityScale;
        else if (!dangDaoChieu && rb.gravityScale < 0)
            rb.gravityScale = -rb.gravityScale;
    }
}
