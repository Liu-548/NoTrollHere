using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Singleton quản lý tất cả input — keyboard/mouse (PC) và touch (Android).
/// PlayerController và các script khác chỉ đọc từ đây, không dùng Input.GetKey trực tiếp.
/// Execution order -100 để chạy trước mọi script khác.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameInput : MonoBehaviour
{
    public static GameInput instance;

    // ─── INDIVIDUAL KEY STATES (PC — null-safe khi không có keyboard) ───
    public bool WDown     => Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame;
    public bool WHeld     => Keyboard.current != null && Keyboard.current.wKey.isPressed;
    public bool SDown     => Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
    public bool SHeld     => Keyboard.current != null && Keyboard.current.sKey.isPressed;
    public bool ADown     => Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame;
    public bool AHeld     => Keyboard.current != null && Keyboard.current.aKey.isPressed;
    public bool DDown     => Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame;
    public bool DHeld     => Keyboard.current != null && Keyboard.current.dKey.isPressed;
    public bool SpaceDown => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
    public bool SpaceHeld => Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

    // ─── VIRTUAL INPUT (Android — set bởi MobileUIOverlay) ──────────────
    private Vector2 virtualMove;
    private bool    virtualJumpPressed;
    private bool    virtualJumpHeld;
    private bool    virtualCrouchHeld;
    private bool    virtualPausePressed;

    // ─── PUBLIC COMBINED API ─────────────────────────────────────────────

    /// Hướng ngang [-1, 1]: A/D keyboard + left joystick + arrow keys
    public float HuongNgang
    {
        get
        {
            float val = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  val -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) val += 1f;
            }
            val += virtualMove.x;
            return Mathf.Clamp(val, -1f, 1f);
        }
    }

    /// Hướng dọc [-1, 1]: W/S keyboard + left joystick (dùng cho leo dây)
    public float HuongDoc
    {
        get
        {
            float val = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    val += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  val -= 1f;
            }
            val += virtualMove.y;
            return Mathf.Clamp(val, -1f, 1f);
        }
    }

    /// Jump pressed this frame (W + Space + virtual button)
    public bool JumpPressed => WDown || SpaceDown || virtualJumpPressed;

    /// Jump held (W + Space + virtual button)
    public bool JumpHeld    => WHeld || SpaceHeld || virtualJumpHeld;

    /// Crouch held (S + virtual button)
    public bool CrouchHeld  => SHeld || virtualCrouchHeld;

    /// Pause pressed this frame (Esc + P + RMB + virtual button)
    public bool PausePressed
    {
        get
        {
            if (virtualPausePressed) return true;
            if (Keyboard.current == null) return false;
            return Keyboard.current.escapeKey.wasPressedThisFrame
                || Keyboard.current.pKey.wasPressedThisFrame
                || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);
        }
    }

    // ─── HELPER: kiểm tra bất kỳ phím xác nhận ─────────────────────────
    public bool ConfirmDown
        => (Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame
            || Keyboard.current.numpadEnterKey.wasPressedThisFrame
            || Keyboard.current.spaceKey.wasPressedThisFrame));

    public bool EscapeDown
        => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

    // ─── MOBILE INPUT METHODS (gọi từ MobileUIOverlay) ──────────────────
    public void SetVirtualMove(Vector2 dir) => virtualMove = dir;

    // Gọi từ MobileUIOverlay.Update() để cập nhật trạng thái giữ nút
    public void SetVirtualJump(bool held) => virtualJumpHeld = held;

    // Gọi từ MobileUIOverlay trực tiếp khi PointerDown — không phụ thuộc vào virtualJumpHeld
    public void FireVirtualJumpPress() => virtualJumpPressed = true;

    public void SetVirtualCrouch(bool held) => virtualCrouchHeld = held;

    public void FireVirtualPause() => virtualPausePressed = true;

    // ─── LIFECYCLE ───────────────────────────────────────────────────────
    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID
        // Khóa màn hình ngang
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait          = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft     = true;
        Screen.autorotateToLandscapeRight    = true;
#endif
    }

    void LateUpdate()
    {
        // Reset one-shot virtual inputs sau mỗi frame
        virtualJumpPressed  = false;
        virtualPausePressed = false;
    }
}
