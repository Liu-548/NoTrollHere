using UnityEngine;

/// <summary>
/// Xử lý hold-to-repeat cho một tổ hợp phím.
/// Gọi Update(dt) mỗi frame — trả về true khi nên kích hoạt:
///   - Ngay lập tức khi nhấn lần đầu
///   - Lặp lại sau mỗi RepeatInterval khi giữ phím (sau khoảng InitialDelay)
/// Dùng Time.unscaledDeltaTime ở scene có timeScale = 0 (PauseMenu).
/// </summary>
public class MenuKeyHold
{
    public float InitialDelay   = 0.35f; // giây trước khi bắt đầu repeat
    public float RepeatInterval = 0.08f; // giây giữa mỗi lần repeat

    private readonly KeyCode[] keys;
    private float timer     = 0f;
    private bool  holding   = false;

    public MenuKeyHold(params KeyCode[] keys) { this.keys = keys; }

    public bool Update(float dt)
    {
        bool pressed = false;
        bool held    = false;
        foreach (var k in keys)
        {
            if (Input.GetKeyDown(k)) pressed = true;
            if (Input.GetKey(k))     held    = true;
        }

        if (pressed)
        {
            holding = true;
            timer   = InitialDelay;
            return true;          // kích hoạt ngay lần nhấn đầu
        }

        if (!held) { holding = false; timer = 0f; return false; }

        if (holding)
        {
            timer -= dt;
            if (timer <= 0f)
            {
                timer += RepeatInterval;
                return true;      // kích hoạt repeat
            }
        }

        return false;
    }
}
