using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("=== DI CHUYỂN ===")]
    public float tocDoChay = 7f;
    [Range(0.1f, 1f)]
    public float heSoTocDoNgoi = 0.3f;

    [Header("=== NHẢY ===")]
    public float lucNhay = 14f;
    public float trongLucRoi = 4f;
    public float trongLucNhay = 2f;
    public float thoiGianGiuNhayToiDa = 0.2f;

    [Header("=== PHÍM DI CHUYỂN ===")]
    public KeyCode nutTrai      = KeyCode.A;
    public KeyCode nutPhai      = KeyCode.D;
    public KeyCode nutNhayLeo   = KeyCode.W;   // nhảy + leo dây lên
    public KeyCode nutXuongNgoi = KeyCode.S;   // leo dây xuống + ngồi

    [Header("=== GÁN SCRIPT CHO CÁC NÚT ===")]
    [Tooltip("Bật = nút W gọi onWPressed thay vì nhảy/leo dây lên.")]
    public bool wGanScript = false;
    public UnityEvent onWPressed;

    [Tooltip("Bật = nút S gọi onSPressed thay vì ngồi/leo dây xuống.")]
    public bool sGanScript = false;
    public UnityEvent onSPressed;

    [Tooltip("Bật = nút A gọi onAPressed thay vì đi trái.")]
    public bool aGanScript = false;
    public UnityEvent onAPressed;

    [Tooltip("Bật = nút D gọi onDPressed thay vì đi phải.")]
    public bool dGanScript = false;
    public UnityEvent onDPressed;

    [Header("=== PHÍM SPACE ===")]
    [Tooltip("Tắt = Space hoạt động giống W (nhảy + leo dây). Bật = gọi onSpacePressed.")]
    public bool spaceGanScript = false;
    public UnityEvent onSpacePressed;

    public LayerMask layerMatDat;

    private Rigidbody2D rb;
    [HideInInspector] public bool dangDungTrenDat;
    private Animator animator;
    private SpriteRenderer sr;
    private BoxCollider2D col;

    private float huongNgang;
    private bool muonNhay;
    private bool dangGiuNutNhay;
    private float thoiGianDaGiuNhay = 0f;
    private bool dangEpVaoTuong = false;
    private float footstepTimer = 0f;

    // Tham chiếu GravityFlipper (nếu có) — dùng để xác định chiều "mặt đất"
    private GravityFlipper gravityFlipper;

    // Lò xo
    private float lucLoXo = 0f;
    private Vector2 huongLoXo = Vector2.up;
    private float thoiGianGiuLoXo = 0.5f;
    private float thoiGianDaGiuLoXo = 0f;
    private bool dangNayLoXo = false;

    // Leo dây
    private bool dangLeoDay = false;
    private float tocDoLeoDay = 4f;
    private float thoiGianKhoaLeoDay = 0f;

    // Nhảy animation
    private bool dangNhayAnim = false;   // true từ lúc bấm nhảy đến khi chạm đất

    // Ngồi
    private bool dangNgoi = false;
    [HideInInspector] public Vector2 kichThuocColGoc;
    [HideInInspector] public Vector2 offsetColGoc;

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        animator        = GetComponent<Animator>();
        sr              = GetComponent<SpriteRenderer>();
        col             = GetComponent<BoxCollider2D>();
        gravityFlipper  = GetComponent<GravityFlipper>();

        // Lưu giá trị collider gốc trong Awake để SkinApplier.Start() có thể ghi đè đúng thứ tự
        if (col != null)
        {
            kichThuocColGoc = col.size;
            offsetColGoc    = col.offset;
        }
    }

    void Start() { }

    void Update()
    {
        if (thoiGianKhoaLeoDay > 0)
            thoiGianKhoaLeoDay -= Time.deltaTime;

        var gi = GameInput.instance;
        if (gi == null) return; // GameInput chưa khởi tạo (an toàn trên mọi nền tảng)

        // === INPUT NGANG ===
        // Nút A/D: nếu ganScript → gọi event khi bấm, không di chuyển
        if (gi.ADown && aGanScript) onAPressed?.Invoke();
        if (gi.DDown && dGanScript) onDPressed?.Invoke();

        huongNgang = 0f;
        if (!aGanScript && !dGanScript)
        {
            huongNgang = gi.HuongNgang;
        }
        else
        {
            if (!aGanScript) huongNgang -= gi.AHeld ? 1f : 0f;
            if (!dGanScript) huongNgang += gi.DHeld ? 1f : 0f;
            huongNgang = Mathf.Clamp(huongNgang, -1f, 1f);
        }

        // === NHẢY ===
        bool spaceXuong = gi.SpaceDown;
        bool spaceGiu   = gi.SpaceHeld;
        bool wXuong     = gi.WDown;
        bool wGiu       = gi.WHeld;

        // Nút có script riêng → kích hoạt event rồi bỏ qua chức năng mặc định
        if (spaceXuong && spaceGanScript) onSpacePressed?.Invoke();
        if (wXuong     && wGanScript)     onWPressed?.Invoke();

        // Virtual jump (mobile): chỉ tính khi không bị keyboard chặn
        bool virtualJump      = gi.JumpPressed && !wXuong && !spaceXuong;
        bool virtualJumpHeld  = gi.JumpHeld    && !wGiu   && !spaceGiu;

        // Nút nhảy ảo (mobile) cũng kích hoạt script nếu được cấu hình
        if (virtualJump && wGanScript)     onWPressed?.Invoke();
        if (virtualJump && spaceGanScript) onSpacePressed?.Invoke();

        bool nutNhayXuong = (!wGanScript && wXuong)
                         || (!spaceGanScript && spaceXuong)
                         || (virtualJump && !wGanScript && !spaceGanScript);

        bool nutNhayGiu   = (!wGanScript && wGiu)
                         || (!spaceGanScript && spaceGiu)
                         || (virtualJumpHeld && !wGanScript && !spaceGanScript);

        if (nutNhayXuong && !dangNgoi)
        {
            muonNhay = true;
            dangNhayAnim = true;        // Bật ngay khi bấm nhảy
            thoiGianDaGiuNhay = 0f;
            if (SoundManager.instance != null)
                SoundManager.instance.PlayNhay();
        }
        // Giữ nút nhảy → tự động nhảy lại khi vừa chạm đất (auto-jump khi hold)
        else if (nutNhayGiu && dangDungTrenDat && !dangNgoi && !muonNhay)
        {
            muonNhay = true;
            dangNhayAnim = true;
            thoiGianDaGiuNhay = 0f;
            if (SoundManager.instance != null)
                SoundManager.instance.PlayNhay();
        }

        if (nutNhayGiu && !dangDungTrenDat && rb.linearVelocity.y > 0)
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
            bool thoatLeoDay = (!wGanScript && wXuong)
                            || (!spaceGanScript && spaceXuong);
                            // virtualJump (nút UI) không thoát dây — dùng để leo lên qua virtualMove.y
            if (thoatLeoDay)
            {
                KetThucLeoDay();
                muonNhay = true;
                thoiGianDaGiuNhay = 0f;
                if (SoundManager.instance != null)
                    SoundManager.instance.PlayNhay();
            }
        }

        // Nút S: gọi event nếu ganScript
        if (gi.SDown && sGanScript) onSPressed?.Invoke();

        // === NGỒI ===
        XuLyNgoi();

        // === SFX CHẠY ===
        if (dangDungTrenDat && Mathf.Abs(huongNgang) > 0.1f && !dangNgoi)
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

    // ─────────────────────────────────────────────
    //  NGỒI
    // ─────────────────────────────────────────────
    void XuLyNgoi()
    {
        if (dangLeoDay) return; // không ngồi khi leo dây

        var gi2 = GameInput.instance;
        bool nutXuongGiu = !sGanScript && gi2 != null && gi2.CrouchHeld;

        // Bắt đầu ngồi: chỉ khi đứng trên đất VÀ giữ S
        if (nutXuongGiu && dangDungTrenDat && !dangNgoi)
        {
            dangNgoi = true;
            if (col != null)
            {
                col.size   = new Vector2(kichThuocColGoc.x, kichThuocColGoc.y * 0.5f);
                col.offset = new Vector2(offsetColGoc.x, offsetColGoc.y - kichThuocColGoc.y * 0.25f);
                Physics2D.SyncTransforms(); // đồng bộ ngay để tránh mất contact 1 frame
            }
        }
        // Đứng dậy: CHỈ khi THẢ nút S — không phụ thuộc dangDungTrenDat
        // (tránh flickering do collider nhỏ lại khiến contact bị mất tạm thời)
        else if (!nutXuongGiu && dangNgoi)
        {
            if (CoTheDungDay())
            {
                dangNgoi = false;
                if (col != null)
                {
                    col.size   = kichThuocColGoc;
                    col.offset = offsetColGoc;
                }
            }
        }
    }

    bool CoTheDungDay()
    {
        if (col == null) return true;

        // OverlapBox ở vị trí đứng đầy đủ chiều cao, trừ phần dưới chân
        Vector2 center = (Vector2)transform.position
                       + new Vector2(offsetColGoc.x, offsetColGoc.y + kichThuocColGoc.y * 0.25f);
        Vector2 size   = new Vector2(kichThuocColGoc.x * 0.9f, kichThuocColGoc.y * 0.5f);

        return Physics2D.OverlapBox(center, size, 0f, layerMatDat) == null;
    }

    // ─────────────────────────────────────────────
    //  FIXED UPDATE
    // ─────────────────────────────────────────────
    void FixedUpdate()
    {
        KiemTraMatDat();
        KiemTraEpTuong();

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

    // ─────────────────────────────────────────────
    //  DI CHUYỂN
    // ─────────────────────────────────────────────
    void DiChuyenLeoDay()
    {
        float huongDoc = GameInput.instance != null ? GameInput.instance.HuongDoc : 0f;
        if (wGanScript)  huongDoc = Mathf.Min(huongDoc, 0f); // W bị ganScript → không leo lên
        if (sGanScript)  huongDoc = Mathf.Max(huongDoc, 0f); // S bị ganScript → không leo xuống

        rb.linearVelocity = new Vector2(
            huongNgang * tocDoChay * 0.5f,
            huongDoc * tocDoLeoDay);

        rb.gravityScale = 0f;
    }

    void DiChuyen()
    {
        if (thoiGianDaGiuLoXo > 0 && Mathf.Abs(huongLoXo.x) > 0.3f)
        {
            thoiGianDaGiuLoXo -= Time.fixedDeltaTime;
            return;
        }
        if (thoiGianDaGiuLoXo > 0)
            thoiGianDaGiuLoXo -= Time.fixedDeltaTime;

        float tocDoHienTai = dangNgoi ? tocDoChay * heSoTocDoNgoi : tocDoChay;

        rb.linearVelocity = new Vector2(
            huongNgang * tocDoHienTai,
            rb.linearVelocity.y);
    }

    void Nhay()
    {
        if (muonNhay && dangDungTrenDat && !dangNgoi)
        {
            // Khi trọng lực đảo ngược, nhảy phải đẩy theo hướng ngược (xuống)
            bool daoNguoc = gravityFlipper != null && gravityFlipper.dangDaoChieu;
            float huongNhay = daoNguoc ? -lucNhay : lucNhay;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, huongNhay);
        }
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
            thoiGianDaGiuLoXo = thoiGianGiuLoXo;
        }
    }

    // ─────────────────────────────────────────────
    //  KIỂM TRA MẶT ĐẤT / TƯỜNG
    // ─────────────────────────────────────────────
    void KiemTraMatDat()
    {
        dangDungTrenDat = false;

        ContactPoint2D[] contacts = new ContactPoint2D[10];
        int soContact = (col != null)
            ? col.GetContacts(contacts)
            : GetComponent<Collider2D>().GetContacts(contacts);

        // FixedUpdate chạy sau LateUpdate của frame trước → gravityScale đã được
        // GravityFlipper đảo dấu đúng: dương = trọng lực bình thường, âm = đảo ngược.
        // Trọng lực bình thường  → mặt đất có normal.y > 0 (hướng lên)
        // Trọng lực đảo ngược   → "mặt đất" là trần, normal.y < 0 (hướng xuống)
        float gravSign = rb.gravityScale >= 0 ? 1f : -1f;

        for (int i = 0; i < soContact; i++)
        {
            if (contacts[i].normal.y * gravSign > 0.5f)
            {
                dangDungTrenDat = true;
                break;
            }
        }
    }

    void KiemTraEpTuong()
    {
        float khoangKiemTra = 0.35f;
        bool tuongTrai = Physics2D.Raycast(transform.position, Vector2.left,  khoangKiemTra, layerMatDat);
        bool tuongPhai = Physics2D.Raycast(transform.position, Vector2.right, khoangKiemTra, layerMatDat);
        dangEpVaoTuong = !dangDungTrenDat &&
            ((tuongTrai && huongNgang < 0) || (tuongPhai && huongNgang > 0));
    }

    // ─────────────────────────────────────────────
    //  TRỌNG LỰC
    // ─────────────────────────────────────────────
    void CapNhatTrongLuc()
    {
        if (dangLeoDay) return;
        // Kiểm tra đất TRƯỚC tường — nếu vừa đứng đất vừa sát tường → vẫn dùng gravity bình thường
        if (dangDungTrenDat) { rb.gravityScale = 1f; return; }
        if (dangEpVaoTuong) { rb.gravityScale = trongLucRoi; return; }

        if (rb.linearVelocity.y < 0)
            rb.gravityScale = trongLucRoi;
        else if (rb.linearVelocity.y > 0 && dangGiuNutNhay)
            rb.gravityScale = trongLucNhay;
        else
            rb.gravityScale = trongLucRoi;
    }

    // ─────────────────────────────────────────────
    //  ANIMATION
    // ─────────────────────────────────────────────
    private const string STATE_IDLE   = "PlayerIdle";
    private const string STATE_RUN    = "PlayerRun";
    private const string STATE_JUMP   = "PlayerJump";
    private const string STATE_FALL   = "PlayerFall";
    private const string STATE_CROUCH = "PlayerCrouch";

    private string stateTruoc = "";

    void CapNhatAnimation()
    {
        if (animator == null) return;

        // GravityFlipper.LateUpdate chưa chạy ở thời điểm này (Update đang chạy),
        // nên dùng dangDaoChieu thay vì rb.gravityScale để biết chiều trọng lực.
        bool daoNguoc = gravityFlipper != null && gravityFlipper.dangDaoChieu;

        // Đặt lại cờ nhảy khi đã chạm "mặt đất" và không còn đà thoát khỏi nó.
        // Bình thường: không đà lên (velY ≤ 0). Đảo ngược: không đà xuống (velY ≥ 0).
        bool daHetDaNhay = daoNguoc ? rb.linearVelocity.y >= 0f : rb.linearVelocity.y <= 0f;
        if (dangDungTrenDat && daHetDaNhay)
            dangNhayAnim = false;

        float velY      = rb.linearVelocity.y;
        bool trongKhong = !dangDungTrenDat;
        // Đang rơi = di chuyển theo chiều trọng lực (xuống bình thường, lên khi đảo)
        bool dangRoi    = trongKhong && (daoNguoc ? velY > 0.1f : velY < -0.1f);

        animator.SetFloat("Speed",      Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded",  dangDungTrenDat);
        animator.SetBool("IsCrouching", dangNgoi);
        animator.SetBool("IsFalling",   dangRoi);

        string stateCanPhat;
        if (dangNgoi)
            // Đang ngồi
            stateCanPhat = STATE_CROUCH;
        else if (dangNhayAnim || (trongKhong && !dangRoi))
            // Vừa bấm nhảy (kể cả frame trước khi velocity được áp dụng)
            // HOẶC đang trên không nhưng chưa rơi (đang lên / đỉnh quỹ đạo)
            stateCanPhat = STATE_JUMP;
        else if (dangRoi)
            // Đang rơi xuống
            stateCanPhat = STATE_FALL;
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            stateCanPhat = STATE_RUN;
        else
            stateCanPhat = STATE_IDLE;

        // Force-play nếu state thay đổi HOẶC Animator bị AnyState kéo sang state sai
        bool saiState = !animator.GetCurrentAnimatorStateInfo(0).IsName(stateCanPhat);
        if (stateCanPhat != stateTruoc || saiState)
        {
            animator.Play(stateCanPhat, 0, 0f);
            stateTruoc = stateCanPhat;
        }
    }

    // ─────────────────────────────────────────────
    //  API CÔNG KHAI
    // ─────────────────────────────────────────────
    public void BatDauLeoDay(float tocDo)
    {
        if (thoiGianKhoaLeoDay > 0) return;
        dangLeoDay = true;
        tocDoLeoDay = tocDo;
        rb.gravityScale = 0f;
    }

    public void KetThucLeoDay()
    {
        dangLeoDay = false;
        rb.gravityScale = trongLucRoi;
    }

    public void NayLoXo(float luc, Vector2 huong)
    {
        dangLeoDay = false;
        rb.gravityScale = trongLucRoi;
        rb.linearVelocity = Vector2.zero;

        lucLoXo = luc;
        huongLoXo = huong.normalized;
        dangNayLoXo = true;
        thoiGianDaGiuLoXo = thoiGianGiuLoXo;
        thoiGianKhoaLeoDay = 0.5f;
    }
}
