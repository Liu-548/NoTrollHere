using UnityEngine;

/// <summary>
/// Điều chỉnh RectTransform của Canvas root panel theo Screen.safeArea
/// để tránh bị notch/cutout Android che khuất UI.
/// Attach vào root panel của Canvas (không phải Canvas chính, mà child đầu tiên của nó).
/// </summary>
[ExecuteAlways]
public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform rt;
    private Rect          lastSafeArea = Rect.zero;
    private Vector2       lastScreenSize;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        Rect safe = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // Chỉ apply khi safe area hoặc screen size thay đổi (tránh set mỗi frame)
        if (safe == lastSafeArea && screenSize == lastScreenSize) return;

        lastSafeArea   = safe;
        lastScreenSize = screenSize;
        Apply(safe, screenSize);
    }

    void Apply(Rect safe, Vector2 screen)
    {
        if (rt == null) return;

        // Tính anchor theo tỉ lệ màn hình
        Vector2 anchorMin = new Vector2(safe.x / screen.x, safe.y / screen.y);
        Vector2 anchorMax = new Vector2(
            (safe.x + safe.width)  / screen.x,
            (safe.y + safe.height) / screen.y);

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

#if UNITY_EDITOR
    // Hiển thị safe area trong Editor để preview
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        Rect s = Screen.safeArea;
        if (s == new Rect(0, 0, Screen.width, Screen.height)) return; // không có notch

        GUI.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        // Vẽ vùng bị che phía trên
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height - s.y - s.height),
            Texture2D.whiteTexture);
    }
#endif
}
