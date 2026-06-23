# BetrayingPlatform — Tóm tắt sử dụng

> Cập nhật lần cuối: kiểm tra toàn bộ 10 scene đang dùng script này.

---

## Cách hoạt động chung

`BetrayingPlatform` gắn lên một object di chuyển/xoay.  
`BetrayingTrigger` gắn lên **child collider** của cùng object đó — khi player chạm vào trigger, nó gọi hàm tương ứng trên BetrayingPlatform.

---

## 5 chế độ (Che Do)

### 0 · TuDong — Tự động A→B→A lặp
Platform tự đi từ vị trí gốc → điểm đến → về, lặp mãi. Không cần trigger.

**Dùng khi:** platform di chuyển liên tục không cần player kích hoạt (bẫy luôn chạy, thang máy...).

**Trường quan trọng:** `offsetDiemDen`, `tocDo`, `tocDoVe` (0 = dùng cùng tocDo), `delayTaiDiem`.

**Scene đang dùng:** Level_2_2, Level_2_5, Level_2_6, Level_S_1

---

### 1 · ChoTrigger — Đứng yên, chờ trigger
Platform không làm gì cho đến khi `BetrayingTrigger` (chế độ `DiDenOffset`) gọi.  
Sau đó di chuyển đến `viTriGoc + offset` rồi dừng.

**Dùng khi:** bẫy chỉ kích hoạt khi player đạp lên / chạm vào (sàn sập, cầu bật...).

**Trường quan trọng:** `offsetDiemDen` (dùng làm default offset), `tocDo`.  
Offset thực tế do `BetrayingTrigger.offset` ghi đè.

**Scene đang dùng:** Level_1_1, Level_1_3, Level_1_4, Level_1_6, Level_2_4

---

### 2 · TuDongMotChieu — Tự động A→B một lần
Giống TuDong nhưng chỉ đi một chiều rồi dừng hẳn (hoặc tự xóa).

**Dùng khi:** sàn sập một lần không quay lại, platform "phóng" player rồi biến mất.

**Trường quan trọng:** `offsetDiemDen`, `tocDo`, `phaHuySauKhiDen`.

**Scene đang dùng:** Level_2_6

---

### 3 · Waypoint — Chạy theo danh sách điểm
Platform đi qua các điểm trong `cacWaypoint[]` (offset so với vị trí gốc).  
Có thể tự chạy ngay hoặc chờ `BetrayingTrigger` (chế độ `BatDauWaypoint`) kích hoạt.

**Dùng khi:** đường đi phức tạp (L-shape, zigzag, thang máy nhiều tầng).

**Trường quan trọng:** `cacWaypoint[]`, `cheDoWaypoint`, `delayTaiMoiWaypoint`.

| cheDoWaypoint | Hành vi |
|---|---|
| MotChieu | WP0→WP1→...→WPn rồi dừng |
| LapLai | WP0→WPn→WP0→WPn vòng kín |
| PingPong | WP0→WPn→WP0→WPn qua lại |

**Lưu ý:** WP0 thường đặt `(0,0)` = vị trí gốc của object.

**Scene đang dùng:** Level_1_1, Level_1_3, Level_1_4, Level_2_5, Level_2_6, Level_S_1 (qua trigger BatDauWaypoint)

---

### 4 · XoayBay — Xoay đúng N độ, đẩy player xuống vực
Platform xoay quanh pivot của chính nó. Player đang đứng trên sẽ bị kéo theo góc xoay rồi văng ra.

**Dùng khi:** tường/sàn xoay đẩy player xuống vực — cần đặt **pivot object ở mép** tường (không phải giữa).

**Trường quan trọng:**

| Trường | Ý nghĩa |
|---|---|
| `gocXoayBay` | Số độ (luôn dương, VD: 90) |
| `huongXoay` | NguocKimDongHo / XuoiKimDongHo |
| `tocDoXoayBay` | Tốc độ xoay đi (độ/giây) |
| `cheDoSauXoay` | OYenTaiCho (giữ góc) / TroVeGoc (xoay về) |
| `delayTruocKhiTroVe` | Thời gian đứng yên trước khi về (chỉ TroVeGoc) |
| `tocDoXoayVe` | Tốc độ xoay về. 0 = dùng tocDoXoayBay |

**Kích hoạt bằng trigger:** dùng `BetrayingTrigger` chế độ `XoayBay`.  
**Tự chạy ngay:** chọn `cheDo = XoayBay` (không cần trigger).

**Scene đang dùng:** Level_2_7

---

## Trường dùng chung (mọi chế độ)

| Trường | Ý nghĩa | Default |
|---|---|---|
| `delayTruocKhiDi` | Delay trước khi bắt đầu | 0 |
| `phaHuySauKhiDen` | Tự xóa object sau khi xong | false |
| `delayPhaHuy` | Delay trước khi xóa | 0.5 |

---

## BetrayingTrigger — 3 chế độ

| cheDoTrigger | Gọi hàm nào | Dùng khi |
|---|---|---|
| `DiDenOffset` | `KichHoat(offset, tocDo)` | Kích hoạt di chuyển thẳng (ChoTrigger) |
| `BatDauWaypoint` | `KichHoatWaypoint(batDauTuWaypoint)` | Kích hoạt chạy waypoint |
| `XoayBay` | `KichHoatXoayBay()` | Kích hoạt xoay bay |

**`chiKichHoatMotLan`:** bật = trigger chỉ fire một lần dù player ra vào nhiều lần.

---

## Những gì đã xóa (và tại sao)

| Tính năng xóa | Lý do |
|---|---|
| `coXoay` / `tocDoXoay` (xoay liên tục khi di chuyển) | 0 scene sử dụng |
| `nhayMayKhiSapDi` / `thoiGianNhayMay` | 0 scene sử dụng |
| `rungManHinhTruocKhiDi` / `thoiGianRung` / `doDungManHinh` | 0 scene sử dụng |

---

## Ghi chú Level_2_7 (XoayBay)

Trường `gocXoayBay` trong scene này được lưu là `-90` (số âm từ lần chỉnh cũ).  
Script hiện tại dùng `Mathf.Abs(gocXoayBay)` nên vẫn hoạt động đúng — xoay 90° theo `huongXoay = XuoiKimDongHo`.  
Có thể sửa lại thành `gocXoayBay = 90` trong Inspector cho nhất quán.
