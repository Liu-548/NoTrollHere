using UnityEngine;
using System.Collections;

// Spike ẩn dưới đất, mọc lên khi Player bước vào vùng trigger
// Gắn script này lên object HiddenSpike (object cha)
public class HiddenSpike : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianChoTruocKhiMoc = 0.2f;
    // Delay nhỏ trước khi mọc — đủ để player đi vào bẫy

    public float tocDoMoc = 3f;
    // Tốc độ trượt lên (đơn vị/giây)

    public float chieuCaoMocLen = 0.7f;
    // Mọc lên cao bao nhiêu so với vị trí ẩn

    // Biến nội bộ
    private Vector3 viTriAn;
    // Vị trí ban đầu (ẩn dưới đất)

    private Vector3 viTriMocLen;
    // Vị trí sau khi mọc lên hoàn toàn

    private bool dangMoc = false;
    // Tránh kích hoạt nhiều lần

    void Start()
    {
        // Lưu vị trí ẩn ban đầu
        viTriAn = transform.position;

        // Tính vị trí sau khi mọc lên
        viTriMocLen = viTriAn + new Vector3(0, chieuCaoMocLen, 0);
    }

    // Hàm này được gọi từ TriggerZone khi player bước vào
    public void KichHoat()
    {
        if (!dangMoc)
        {
            dangMoc = true;
            StartCoroutine(MocLen());
        }
    }

    IEnumerator MocLen()
    {
        // Đợi chút rồi mới mọc
        yield return new WaitForSeconds(thoiGianChoTruocKhiMoc);

        // Trượt lên từ từ
        while (transform.position.y < viTriMocLen.y)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                viTriMocLen,
                tocDoMoc * Time.deltaTime
            );
            yield return null;
        }

        // Đã mọc lên hoàn toàn — đợi 1.5 giây rồi thu về
        yield return new WaitForSeconds(1.5f);

        // Trượt xuống về vị trí ẩn
        while (transform.position.y > viTriAn.y)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                viTriAn,
                tocDoMoc * Time.deltaTime
            );
            yield return null;
        }

        // Reset để có thể kích hoạt lại
        dangMoc = false;
    }
}