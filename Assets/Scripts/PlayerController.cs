using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("=== DI CHUYỂN ===")]
    public float tocDoChay = 7f;

    [Header("=== NHẢY ===")]
    public float lucNhay = 14f;
    public float trongLucRoi = 4f;
    public float trongLucNhay = 2f;
    public float thoiGianGiuNhayToiDa = 0.2f;

    public LayerMask layerMatDat;

    private Rigidbody2D rb;
    private bool dangDungTrenDat;
    private Animator animator;
    private SpriteRenderer sr;
    private float huongNgang;
    private bool muonNhay;
    private bool dangGiuNutNhay;
    private float thoiGianDaGiuNhay = 0f;
    private bool dangEpVaoTuong = false;
    private float footstepTimer = 0f;

    // Lò xo
    private float lucLoXo = 0f;
    private Vector2 huongLoXo = Vector2.up;
    private float thoiGianGiuLoXo = 0.5f; // Giữ lực lò xo bao lâu
    private float thoiGianDaGiuLoXo = 0f;
    private bool dangNayLoXo = false;

    private bool dangLeoDay = false;
    private float tocDoLeoDay = 4f;
    // Thêm biến
    private float thoiGianKhoaLeoDay = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Thêm vào đầu Update(), trước phần input
        if (thoiGianKhoaLeoDay > 0)
            thoiGianKhoaLeoDay -= Time.deltaTime;

        // === INPUT NGANG ===
        huongNgang = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            huongNgang -= 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            huongNgang += 1f;
        huongNgang = Mathf.Clamp(huongNgang, -1f, 1f);

        // === NHẢY ===
        if (Input.GetKeyDown(KeyCode.Space)
         || Input.GetKeyDown(KeyCode.W)
         || Input.GetKeyDown(KeyCode.UpArrow))
        {
            muonNhay = true;
            thoiGianDaGiuNhay = 0f;
            if (SoundManager.instance != null)
                SoundManager.instance.PlayNhay();
        }

        bool nutNhayDangGiu = Input.GetKey(KeyCode.Space)
                           || Input.GetKey(KeyCode.W)
                           || Input.GetKey(KeyCode.UpArrow);

        if (nutNhayDangGiu && !dangDungTrenDat && rb.linearVelocity.y > 0)
        {
            thoiGianDaGiuNhay += Time.deltaTime;
            dangGiuNutNhay = thoiGianDaGiuNhay <= thoiGianGiuNhayToiDa;
        }
        else
        {
            dangGiuNutNhay = false;
            if (dangDungTrenDat) thoiGianDaGiuNhay = 0f;
        }

        // === INPUT LEO DÂY ===
        if (dangLeoDay)
        {
            // Nhảy ra khỏi dây
            if (Input.GetKeyDown(KeyCode.Space)
             || Input.GetKeyDown(KeyCode.W)
             || Input.GetKeyDown(KeyCode.UpArrow))
            {
                KetThucLeoDay();
                muonNhay = true;
                thoiGianDaGiuNhay = 0f;
                if (SoundManager.instance != null)
                    SoundManager.instance.PlayNhay();
            }
        }

        // === SFX CHẠY ===
        if (dangDungTrenDat && Mathf.Abs(huongNgang) > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (SoundManager.instance != null)
                    SoundManager.instance.PlayChay();
                footstepTimer = 0.25f;
            }
        }
        else footstepTimer = 0f;

        // === FLIP SPRITE ===
        if (huongNgang > 0) sr.flipX = false;
        else if (huongNgang < 0) sr.flipX = true;

        CapNhatTrongLuc();
        CapNhatAnimation();
    }

    void FixedUpdate()
    {
        KiemTraMatDat();
        KiemTraEpTuong();

        // Đang nảy lò xo → bỏ qua leo dây hoàn toàn
        if (dangNayLoXo)
        {
            Nhay();
            return;
        }

        if (dangLeoDay)
            DiChuyenLeoDay();
        else
        {
            DiChuyen();
            Nhay();
        }
    }

    void DiChuyenLeoDay()
    {
        float huongDoc = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            huongDoc = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            huongDoc = -1f;

        // Di chuyển ngang + dọc trên dây
        rb.linearVelocity = new Vector2(
            huongNgang * tocDoChay * 0.5f, // ngang chậm hơn
            huongDoc * tocDoLeoDay);

        // Tắt gravity khi đang leo
        rb.gravityScale = 0f;
    }

    public void BatDauLeoDay(float tocDo)
    {
        // Không cho leo nếu đang trong thời gian khóa
        if (thoiGianKhoaLeoDay > 0) return;

        dangLeoDay = true;
        tocDoLeoDay = tocDo;
        rb.gravityScale = 0f;
    }

    public void KetThucLeoDay()
    {
        dangLeoDay = false;
        // Reset gravity ngay lập tức
        rb.gravityScale = trongLucRoi;
    }

    void KiemTraMatDat()
    {
        dangDungTrenDat = false;

        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int soContact = GetComponent<Collider2D>().GetContacts(contacts);

        for (int i = 0; i < soContact; i++)
        {
            if (contacts[i].normal.y > 0.5f)
            {
                dangDungTrenDat = true;
                break;
            }
        }
    }

    void KiemTraEpTuong()
    {
        float khoangKiemTra = 0.35f;
        bool tuongTrai = Physics2D.Raycast(
            transform.position, Vector2.left,
            khoangKiemTra, layerMatDat);
        bool tuongPhai = Physics2D.Raycast(
            transform.position, Vector2.right,
            khoangKiemTra, layerMatDat);
        dangEpVaoTuong = !dangDungTrenDat &&
            ((tuongTrai && huongNgang < 0) ||
             (tuongPhai && huongNgang > 0));
    }

    void DiChuyen()
    {
        // Lò xo ngang (x > 0.3) → khóa di chuyển trong thời gian giữ
        if (thoiGianDaGiuLoXo > 0 && Mathf.Abs(huongLoXo.x) > 0.3f)
        {
            thoiGianDaGiuLoXo -= Time.fixedDeltaTime;
            return;
        }

        // Lò xo thẳng lên → thoiGianDaGiuLoXo vẫn đếm nhưng không khóa
        if (thoiGianDaGiuLoXo > 0)
            thoiGianDaGiuLoXo -= Time.fixedDeltaTime;

        rb.linearVelocity = new Vector2(
            huongNgang * tocDoChay,
            rb.linearVelocity.y);
    }

    void Nhay()
    {
        if (muonNhay && dangDungTrenDat)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, lucNhay);
        muonNhay = false;

        if (dangNayLoXo)
        {
            if (Mathf.Abs(huongLoXo.x) < 0.3f)
                rb.linearVelocity = new Vector2(
                    huongNgang * tocDoChay,
                    huongLoXo.y * lucLoXo);
            else
                rb.linearVelocity = huongLoXo * lucLoXo;

            dangNayLoXo = false;
            lucLoXo = 0f;
            thoiGianDaGiuLoXo = thoiGianGiuLoXo; // Set sau khi nảy
        }
    }

    void CapNhatTrongLuc()
    {
        // Đang leo dây → gravity = 0, xử lý trong DiChuyenLeoDay()
        if (dangLeoDay) return;

        if (dangEpVaoTuong) { rb.gravityScale = trongLucRoi; return; }
        if (dangDungTrenDat) { rb.gravityScale = 1f; return; }

        if (rb.linearVelocity.y < 0)
            rb.gravityScale = trongLucRoi;
        else if (rb.linearVelocity.y > 0 && dangGiuNutNhay)
            rb.gravityScale = trongLucNhay;
        else
            rb.gravityScale = trongLucRoi;
    }

    void CapNhatAnimation()
    {
        if (animator == null) return;
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", dangDungTrenDat);
        animator.SetBool("IsFalling",
            rb.linearVelocity.y < -0.1f && !dangDungTrenDat);
    }

    // Gọi từ SpringTrap
    public void NayLoXo(float luc, Vector2 huong)
    {
        dangLeoDay = false;
        rb.gravityScale = trongLucRoi;
        rb.linearVelocity = Vector2.zero;

        lucLoXo = luc;
        huongLoXo = huong.normalized;
        dangNayLoXo = true;
        thoiGianDaGiuLoXo = thoiGianGiuLoXo;
        thoiGianKhoaLeoDay = 0.5f; // Khóa leo dây 0.5s
    }
}