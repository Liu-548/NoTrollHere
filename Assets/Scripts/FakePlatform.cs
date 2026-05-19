using UnityEngine;
using System.Collections;

// Platform biến mất sau khi Player đứng lên
// Gắn script này lên object FakePlatform
public class FakePlatform : MonoBehaviour
{
    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianTruocKhiBienMat = 0.5f;
    // Đứng lên bao lâu thì bắt đầu rung (giây)

    public float thoiGianRung = 0.3f;
    // Rung bao lâu trước khi biến mất

    public float thoiGianHoiSinh = 2f;
    // Bao lâu sau khi biến mất thì xuất hiện lại

    // Biến nội bộ
    private bool dangDemGio = false;
    // Tránh kích hoạt nhiều lần cùng lúc

    private Vector3 viTriGoc;
    // Lưu vị trí gốc để rung xung quanh đó

    private SpriteRenderer sr;
    private BoxCollider2D col;

    void Start()
    {
        viTriGoc = transform.position;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    // Gọi khi có vật thể đứng lên platform
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Chỉ kích hoạt khi Player đứng lên
        // và chưa đang trong quá trình biến mất
        if (collision.gameObject.CompareTag("Player") && !dangDemGio)
        {
            dangDemGio = true;
            StartCoroutine(DemGioBienMat());
        }
    }

    IEnumerator DemGioBienMat()
    {
        // --- Đợi một chút trước khi rung ---
        yield return new WaitForSeconds(thoiGianTruocKhiBienMat);

        // --- Rung platform để báo hiệu sắp biến mất ---
        float thoiGianDaRung = 0f;
        while (thoiGianDaRung < thoiGianRung)
        {
            // Rung nhẹ sang trái phải
            float offsetX = Random.Range(-0.05f, 0.05f);
            transform.position = viTriGoc + new Vector3(offsetX, 0, 0);
            thoiGianDaRung += Time.deltaTime;
            yield return null;
        }

        // --- Biến mất ---
        // Tắt collider để player rơi xuyên qua
        col.enabled = false;
        // Ẩn sprite
        sr.enabled = false;

        // --- Đợi rồi xuất hiện lại ---
        yield return new WaitForSeconds(thoiGianHoiSinh);

        // Khôi phục vị trí gốc
        transform.position = viTriGoc;
        // Bật lại collider và sprite
        col.enabled = true;
        sr.enabled = true;

        // Reset để có thể kích hoạt lại
        dangDemGio = false;
    }
}