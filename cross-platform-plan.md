# Kế hoạch phát triển đa nền tảng — NoTrollHere

## Nền tảng mục tiêu
Windows · Android

---

## Bối cảnh hiện tại
- Engine: Unity 6 (6.3 LTS), C#
- Input hiện tại: `Input.GetKey` (legacy) — keyboard + mouse only
- UI: Screen Space Overlay Canvas, CanvasScaler 1280×720, Scale With Screen Size
- Keyboard nav: WASD/Esc/Enter toàn bộ menu đã hoàn chỉnh
- Pause: Esc / P / chuột phải

---

## Vấn đề cốt lõi khi port Android
Toàn bộ input đang bind cứng vào keyboard/mouse. Android không có những thứ này. Cần tách input ra khỏi logic game trước khi làm bất cứ thứ gì khác.

---

## Giải pháp: Input Abstraction Layer

### Bước 1 — Migrate sang Unity New Input System
Cài package `com.unity.inputsystem` qua Package Manager.

Tạo `InputActions` asset với 2 action maps:

```
GameplayActions:
  Move    → WASD (PC) | Virtual Joystick (Mobile) | Left Stick (Gamepad)
  Jump    → Space (PC) | Jump Button (Mobile) | A (Gamepad)
  Crouch  → S/Down (PC) | Crouch Button (Mobile) | B (Gamepad)
  Pause   → Esc/P/RMB (PC) | Pause Button (Mobile) | Start (Gamepad)

UIActions:
  Navigate → WASD/Arrow | Swipe/Tap | D-pad
  Confirm  → Enter/Space | Tap | A
  Cancel   → Esc | Back | B
```

### Bước 2 — GameInput singleton
```csharp
public class GameInput : MonoBehaviour
{
    public static GameInput instance;
    private PlayerInputActions actions;

    public Vector2 MoveInput   => actions.Gameplay.Move.ReadValue<Vector2>();
    public bool    JumpPressed  => actions.Gameplay.Jump.WasPressedThisFrame();
    public bool    PausePressed => actions.Gameplay.Pause.WasPressedThisFrame();
}
```
`PlayerController` và tất cả script chỉ đọc từ `GameInput.instance`, không dùng `Input.GetKey` trực tiếp nữa.

### Bước 3 — Menu navigation
Thay keyboard nav thủ công (WASD tracking) bằng Unity **EventSystem + Navigation built-in**.  
EventSystem tự hỗ trợ cả touch tap lẫn keyboard — không cần code riêng cho từng nền tảng.  
Khung vàng (`MenuSelectionFrame`) hook vào `ISelectHandler` thay vì track key press.

### Bước 4 — Mobile UI overlay
```csharp
// Chỉ bật trên Android
#if UNITY_ANDROID
    mobileUIOverlay.SetActive(true);
#else
    mobileUIOverlay.SetActive(false);
#endif
```
Thêm virtual joystick + on-screen buttons (Jump, Crouch, Pause) — chỉ hiện khi chạy trên Android.

### Bước 5 — Safe Area (notch/cutout Android)
```csharp
var safe = Screen.safeArea;
var rt = GetComponent<RectTransform>();
rt.anchorMin = new Vector2(safe.x / Screen.width, safe.y / Screen.height);
rt.anchorMax = new Vector2((safe.x + safe.width) / Screen.width,
                           (safe.y + safe.height) / Screen.height);
```
Áp cho root Canvas panel để tránh bị notch/cutout Android che.

---

## Chi phí theo nền tảng

| Nền tảng | Chi phí | Ghi chú |
|----------|---------|---------|
| Windows  | Miễn phí | Build trực tiếp từ Unity trên máy hiện có |
| Android  | $25 một lần | Google Play Developer account |

**Unity**: Personal tier miễn phí nếu doanh thu < $200k/năm, hỗ trợ build cả Windows lẫn Android từ cùng một máy Windows.

---

## Thứ tự phát triển khuyến nghị (song song)

Sau khi migrate New Input System xong, Windows và Android có thể phát triển **song song** vì:
- Cùng codebase, cùng Unity project
- Conditional compilation (`#if UNITY_ANDROID`) cô lập hoàn toàn phần mobile-only
- Build Windows: File → Build Settings → Windows → Build
- Build Android: File → Build Settings → Android → Build (cần Android Build Support module)

```
Bước 1: Migrate New Input System  ←  làm trước, một lần duy nhất
              ↓
      ┌───────┴────────┐
      ↓                ↓
   Windows          Android
  (feature dev)   (port + mobile UI)
  (luôn playable)  (test trên device)
      ↓                ↓
      └───────┬────────┘
              ↓
         Release cả hai
```

**Workflow song song thực tế:**
- Mọi gameplay feature viết platform-agnostic qua `GameInput.instance`
- Android-specific code bọc trong `#if UNITY_ANDROID`
- Test Windows: Play Mode trong Editor hoặc standalone build
- Test Android: Build APK → cài thẳng lên device (USB debugging) hoặc dùng Android emulator

---

## Files cần thay đổi chính khi migrate

| File | Thay đổi | Ảnh hưởng nền tảng |
|------|----------|--------------------|
| `PlayerController.cs` | Thay `Input.GetKey` → `GameInput.instance.*` | Windows + Android |
| `PauseMenu.cs` | Thay `Input.GetKeyDown` → New Input System | Windows + Android |
| `MainMenuManager.cs` | Dùng EventSystem navigation thay WASD manual | Windows + Android |
| `LevelSelectManager.cs` | Tương tự | Windows + Android |
| `SkinSelectManager.cs` | Tương tự | Windows + Android |
| `AchievementSceneManager.cs` | Tương tự | Windows + Android |
| `MenuKeyHold.cs` | Rewrite dùng `InputAction.IsPressed()` | Windows + Android |
| *(mới)* `GameInput.cs` | Singleton quản lý tất cả input | Windows + Android |
| *(mới)* `MobileUIOverlay.cs` | Virtual joystick + on-screen buttons (`#if UNITY_ANDROID`) | Android only |
| *(mới)* `SafeAreaHandler.cs` | Xử lý notch/cutout Android (`#if UNITY_ANDROID`) | Android only |

**Nguyên tắc song song:** Files không có tag `Android only` phải luôn hoạt động đúng trên cả hai nền tảng. Không được dùng `Input.GetKey` trực tiếp ở bất kỳ đâu ngoài `GameInput.cs`.
