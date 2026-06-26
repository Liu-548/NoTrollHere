using UnityEngine.InputSystem;

/// <summary>
/// Xử lý hold-to-repeat cho một tổ hợp phím (New Input System).
/// Gọi Update(dt) mỗi frame — trả về true khi nên kích hoạt:
///   - Ngay lập tức khi nhấn lần đầu
///   - Lặp lại sau mỗi RepeatInterval khi giữ phím (sau khoảng InitialDelay)
/// Dùng Time.unscaledDeltaTime ở scene có timeScale = 0 (PauseMenu).
/// Trả về false khi không có keyboard (Android/mobile — menu dùng touch tap).
/// </summary>
public class MenuKeyHold
{
    public float InitialDelay   = 0.35f;
    public float RepeatInterval = 0.08f;

    private readonly Key[] keys;
    private float timer   = 0f;
    private bool  holding = false;

    public MenuKeyHold(params Key[] keys) { this.keys = keys; }

    public bool Update(float dt)
    {
        var kb = Keyboard.current;
        if (kb == null) return false;   // Android không có keyboard → bỏ qua

        bool pressed = false;
        bool held    = false;
        foreach (var k in keys)
        {
            if (kb[k].wasPressedThisFrame) pressed = true;
            if (kb[k].isPressed)           held    = true;
        }

        if (pressed)
        {
            holding = true;
            timer   = InitialDelay;
            return true;
        }

        if (!held) { holding = false; timer = 0f; return false; }

        if (holding)
        {
            timer -= dt;
            if (timer <= 0f)
            {
                timer += RepeatInterval;
                return true;
            }
        }

        return false;
    }
}
