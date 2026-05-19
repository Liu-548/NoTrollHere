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

    // Footstep timer
    private float footstepTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
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

            // SFX nhảy
            if (SoundManager.instance != null)
                SoundManager.instance.PlayNhay();
        }

        // Giữ nút nhảy — có giới hạn thời gian
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

        // === SFX CHẠY (footstep) ===
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
        else
        {
            footstepTimer = 0f;
        }

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
        DiChuyen();
        Nhay();
    }

    void KiemTraMatDat()
    {
        dangDungTrenDat = Physics2D.OverlapCircle(
            (Vector2)transform.position + Vector2.down * 0.6f,
            0.15f,
            layerMatDat
        );
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
        rb.linearVelocity = new Vector2(
            huongNgang * tocDoChay,
            rb.linearVelocity.y
        );
    }

    void Nhay()
    {
        if (muonNhay && dangDungTrenDat)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, lucNhay);
        muonNhay = false;
    }

    void CapNhatTrongLuc()
    {
        if (dangEpVaoTuong)
        {
            rb.gravityScale = trongLucRoi;
            return;
        }

        if (dangDungTrenDat)
        {
            rb.gravityScale = 1f;
            return;
        }

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
}