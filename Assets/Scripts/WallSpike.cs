using UnityEngine;
using System.Collections;

// Spike gắn trên tường, bắn ra ngang khi player đi vào vùng trigger
// Gắn script này lên object WallSpike (object cha)
public class WallSpike : MonoBehaviour
{
    [Header("=== HƯỚNG BẮN ===")]
    public Vector2 huongBan = Vector2.left;
    // Vector2.left = bắn sang trái
    // Vector2.right = bắn sang phải
    // Chỉnh trong Inspector tùy vị trí tường

    [Header("=== TỐC ĐỘ ===")]
    public float tocDoBan = 8f;
    // Tốc độ trượt ra (đơn vị/giây)

    public float quangDuongBan = 2f;
    // Bắn ra bao xa thì dừng lại

    public float thoiGianChoTruocKhiBan = 0.15f;
    // Delay nhỏ trước khi bắn

    public float thoiGianRutVe = 1.5f;
    // Đứng ở ngoài bao lâu thì rút về

    // Biến nội bộ
    private Vector3 viTriGoc;
    private Vector3 viTriDaDen;
    private bool dangBan = false;

    void Start()
    {
        viTriGoc = transform.position;
        // Tính điểm đến = vị trí gốc + hướng x quãng đường
        viTriDaDen = viTriGoc + (Vector3)(huongBan.normalized * quangDuongBan);
    }

    // Gọi từ WallSpikeTrigger khi player bước vào
    public void KichHoatBan()
    {
        if (!dangBan)
        {
            dangBan = true;
            StartCoroutine(BanRa());
        }
    }

    IEnumerator BanRa()
    {
        // Delay nhỏ
        yield return new WaitForSeconds(thoiGianChoTruocKhiBan);

        // Trượt ra nhanh
        while (Vector3.Distance(transform.position, viTriDaDen) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                viTriDaDen,
                tocDoBan * Time.deltaTime
            );
            yield return null;
        }
        transform.position = viTriDaDen;

        // Đứng ở ngoài một lúc
        yield return new WaitForSeconds(thoiGianRutVe);

        // Rút về chậm hơn
        while (Vector3.Distance(transform.position, viTriGoc) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                viTriGoc,
                (tocDoBan * 0.5f) * Time.deltaTime
            );
            yield return null;
        }
        transform.position = viTriGoc;

        dangBan = false;
    }
}