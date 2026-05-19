using UnityEngine;
using System.Collections;

public class FallingBrick : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianChoTruocKhiRoi = 0.3f;
    // Delay trước khi rơi

    public float thoiGianHoiSinh = 3f;
    // Bao lâu thì xuất hiện lại

    public bool dangRoi = false;
    // Public để BrickKillZone đọc được

    private Rigidbody2D rb;
    private Vector3 viTriGoc;
    private BoxCollider2D col;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        viTriGoc = transform.position;
    }

    public void KichHoatRoi()
    {
        if (!dangRoi)
        {
            dangRoi = true;
            StartCoroutine(DemGioRoi());
        }
    }

    IEnumerator DemGioRoi()
    {
        // Đợi chút rồi rơi
        yield return new WaitForSeconds(thoiGianChoTruocKhiRoi);

        // Rung nhẹ báo hiệu sắp rơi
        float thoiGianRung = 0.2f;
        float daRung = 0f;
        while (daRung < thoiGianRung)
        {
            float offsetX = Random.Range(-0.05f, 0.05f);
            transform.position = viTriGoc + new Vector3(offsetX, 0, 0);
            daRung += Time.deltaTime;
            yield return null;
        }
        transform.position = viTriGoc;

        // Tắt collider ngay — không cần rơi vật lý
        // Gạch sẽ "rơi" bằng cách di chuyển thủ công xuống dưới
        col.enabled = false;

        // Di chuyển gạch xuống nhanh (giả lập rơi)
        float tocDoRoi = 20f;
        while (transform.position.y > viTriGoc.y - 10f)
        {
            transform.position += Vector3.down * tocDoRoi * Time.deltaTime;
            yield return null;
        }

        // Ẩn sprite
        sr.enabled = false;

        // Đợi rồi hồi sinh
        yield return new WaitForSeconds(thoiGianHoiSinh);

        // Khôi phục
        transform.position = viTriGoc;
        col.enabled = true;
        sr.enabled = true;
        dangRoi = false;
    }
}