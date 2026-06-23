using UnityEngine;
using System.Collections;

public class DeathEffect : MonoBehaviour
{
    [Header("=== SCREEN SHAKE ===")]
    public float doDungCamera = 0.15f;
    public float thoiGianRung = 0.2f;

    [Header("=== FLASH ĐỎ ===")]
    public float thoiGianFlash = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color mauGoc;
    private PlayerController playerController;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mauGoc = spriteRenderer.color;
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void KichHoatHieuUng()
    {
        StartCoroutine(ChayHieuUng());
    }

    IEnumerator ChayHieuUng()
    {
        // Dừng nhân vật hoàn toàn ngay lập tức
        if (playerController != null)
            playerController.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Dừng velocity
            rb.bodyType = RigidbodyType2D.Static; // Freeze hoàn toàn
        }

        // SFX chết
        if (SoundManager.instance != null)
            SoundManager.instance.PlayChet();

        // Flash đỏ
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(thoiGianFlash);
        spriteRenderer.color = mauGoc;

        // Screen shake — route qua CameraController để tránh xung đột
        CameraController cc = Camera.main?.GetComponent<CameraController>();
        if (cc != null)
            yield return StartCoroutine(cc.CoroutineRung(thoiGianRung, doDungCamera));
        else
        {
            // Fallback khi không có CameraController
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 viTriGoc = cam.transform.position;
                float daRung = 0f;
                while (daRung < thoiGianRung)
                {
                    cam.transform.position = new Vector3(
                        viTriGoc.x + Random.Range(-doDungCamera, doDungCamera),
                        viTriGoc.y + Random.Range(-doDungCamera, doDungCamera),
                        viTriGoc.z);
                    daRung += Time.deltaTime;
                    yield return null;
                }
                cam.transform.position = viTriGoc;
            }
            else
                yield return new WaitForSeconds(thoiGianRung);
        }

        if (rb != null)
            rb.bodyType = RigidbodyType2D.Dynamic;

        GameManager.instance.PlayerChet();
    }
}