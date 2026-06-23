using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Teleporter : MonoBehaviour
{
    [Header("=== ĐIỂM ĐẾN ===")]
    public Transform diemDen;

    [Header("=== TỐC ĐỘ FADE ===")]
    public float tocDoFade = 0.1f;

    private bool dangDichChuyen = false;
    private static Image fadeImage;

    void Start()
    {
        if (fadeImage == null)
            TaoFadePanel();
    }

    void TaoFadePanel()
    {
        // Tạo Canvas riêng có sorting order cao — tránh đặt trong Canvas game
        // để không chặn raycast của UI game
        GameObject canvasObj = new GameObject("FadeCanvas_Teleport");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        // KHÔNG thêm GraphicRaycaster → panel này không nhận và không chặn input
        DontDestroyOnLoad(canvasObj);

        GameObject panel = new GameObject("FadePanel");
        panel.transform.SetParent(canvas.transform, false);
        fadeImage = panel.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false; // KHÔNG chặn click khi trong suốt
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (!vatTheChamVao.CompareTag("Player")) return;
        if (dangDichChuyen) return;
        if (diemDen == null)
        {
            Debug.LogWarning("Teleporter: Chưa gán Diem Den!");
            return;
        }

        // SFX teleport
        if (SoundManager.instance != null)
            SoundManager.instance.PlayTeleport();

        dangDichChuyen = true;
        StartCoroutine(DichChuyen(vatTheChamVao.gameObject));
    }

    IEnumerator DichChuyen(GameObject player)
    {
        // Tắt input player
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        // Fade to black
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha = Mathf.MoveTowards(alpha, 1f, tocDoFade);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Teleport
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        player.transform.position = diemDen.position;
        yield return new WaitForFixedUpdate();

        // Fade back in
        while (alpha > 0f)
        {
            alpha = Mathf.MoveTowards(alpha, 0f, tocDoFade);
            if (fadeImage != null)
                fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Bật lại input
        if (pc != null) pc.enabled = true;
        dangDichChuyen = false;
    }

    void OnDrawGizmos()
    {
        if (diemDen == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, diemDen.position);
        Gizmos.DrawWireSphere(diemDen.position, 0.2f);
    }
}