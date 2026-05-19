using UnityEngine;
using System.Collections;

// Script tạo hiệu ứng khi Player chết
// Gắn lên GameObject "Player"
public class DeathEffect : MonoBehaviour
{
    [Header("=== SCREEN SHAKE ===")]
    public float doDungCamera = 0.15f;
    // Biên độ rung — số càng lớn rung càng mạnh

    public float thoiGianRung = 0.2f;
    // Rung trong bao lâu (giây)

    [Header("=== FLASH ĐỎ ===")]
    public float thoiGianFlash = 0.1f;
    // Flash đỏ hiện trong bao lâu (giây)

    // Tham chiếu nội bộ
    private SpriteRenderer spriteRenderer;
    // Dùng để đổi màu Player khi chết

    private Color mauGoc;
    // Lưu màu gốc để khôi phục lại sau khi flash

    void Start()
    {
        // Lấy SpriteRenderer gắn trên Player
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Lưu màu gốc của Player (cam đỏ E85D04)
        mauGoc = spriteRenderer.color;
    }

    // =============================================================
    // HÀM NÀY GỌI TỪ KILLZONE KHI PLAYER CHẾT
    // Thay vì gọi GameManager.PlayerChet() trực tiếp,
    // KillZone sẽ gọi hàm này trước để chạy effect, rồi mới chết
    // =============================================================
    public void KichHoatHieuUng()
    {
        StartCoroutine(ChayHieuUng());
    }

    IEnumerator ChayHieuUng()
    {
        // SFX chết ngay lập tức — nghe đồng thời với flash
        if (SoundManager.instance != null)
            SoundManager.instance.PlayChet();

        // --- FLASH ĐỎ ---
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(thoiGianFlash);
        spriteRenderer.color = mauGoc;

        // --- SCREEN SHAKE ---
        StartCoroutine(RungCamera());
        yield return new WaitForSeconds(thoiGianRung);

        GameManager.instance.PlayerChet();
    }

    IEnumerator RungCamera()
    {
        // Lấy camera chính trong scene
        Camera cam = Camera.main;

        // Lưu vị trí gốc của camera
        Vector3 viTriGoc = cam.transform.position;

        // Biến đếm thời gian đã rung
        float thoiGianDaRung = 0f;

        // Lặp liên tục cho đến khi hết thời gian rung
        while (thoiGianDaRung < thoiGianRung)
        {
            // Tạo vị trí ngẫu nhiên xung quanh vị trí gốc
            float x = viTriGoc.x + Random.Range(-doDungCamera, doDungCamera);
            float y = viTriGoc.y + Random.Range(-doDungCamera, doDungCamera);

            // Di chuyển camera tới vị trí ngẫu nhiên đó
            cam.transform.position = new Vector3(x, y, viTriGoc.z);

            // Tăng thời gian đếm
            thoiGianDaRung += Time.deltaTime;

            // Đợi sang frame tiếp theo
            yield return null;
        }

        // Rung xong → khôi phục camera về vị trí gốc
        cam.transform.position = viTriGoc;
    }
}