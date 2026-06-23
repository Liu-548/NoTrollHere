using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    [Header("=== NỘI DUNG ===")]
    [TextArea(2, 5)]
    public string[] cacDongThoai;
    public float thoiGianMoiDong = 3f;
    public float thoiGianFade = 0.5f;

    [Header("=== VỊ TRÍ TEXT ===")]
    public Vector3 offsetText = new Vector3(0f, 1.5f, 0f);

    [Header("=== STYLE ===")]
    public float coChu = 14f;
    public Color mauChu = new Color(0.91f, 0.91f, 0.82f, 1f);
    public Color mauNen = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    public float rongToiDa = 300f;

    [Header("=== CÀI ĐẶT ===")]
    public bool chiHienMotLan = true;

    private bool daHien = false;

    // ─── SHARED STATIC — dùng chung cho toàn bộ scene ───
    private static GameObject s_TextObject;
    private static TextMeshPro s_TmpText;
    private static GameObject s_NenObject;
    private static SpriteRenderer s_NenRenderer;
    private static DialogueTrigger s_DangChay;   // trigger đang hiển thị
    private static Coroutine s_Coroutine;
    private static Transform s_PlayerTransform;
    private static Vector3 s_OffsetHienTai;
    // ─────────────────────────────────────────────────────

    void Awake()
    {
        // Dọn object cũ nếu scene reload (static vẫn còn giá trị cũ)
        if (s_TextObject == null)
            KhoiTaoShared();
    }

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    static void OnSceneUnloaded(Scene scene)
    {
        // Reset shared state khi scene unload
        s_TextObject = null;
        s_TmpText = null;
        s_NenObject = null;
        s_NenRenderer = null;
        s_DangChay = null;
        s_Coroutine = null;
        s_PlayerTransform = null;
    }

    static void KhoiTaoShared()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) s_PlayerTransform = player.transform;

        s_TextObject = new GameObject("DialogueText_Shared");

        s_NenObject = new GameObject("Background");
        s_NenObject.transform.SetParent(s_TextObject.transform, false);
        s_NenRenderer = s_NenObject.AddComponent<SpriteRenderer>();
        s_NenRenderer.sprite = TaoSpriteNen();
        s_NenRenderer.sortingOrder = 10;

        GameObject tmpObj = new GameObject("Text");
        tmpObj.transform.SetParent(s_TextObject.transform, false);
        s_TmpText = tmpObj.AddComponent<TextMeshPro>();
        s_TmpText.fontSize = 14f;
        s_TmpText.alignment = TextAlignmentOptions.Center;
        s_TmpText.textWrappingMode = TextWrappingModes.Normal;
        s_TmpText.rectTransform.sizeDelta = new Vector2(3f, 10f);
        s_TmpText.sortingOrder = 11;

        s_TextObject.SetActive(false);
    }

    static Sprite TaoSpriteNen()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void Update()
    {
        // Chỉ trigger đang hiển thị mới cập nhật vị trí
        if (s_DangChay != this) return;
        if (s_TextObject != null && s_TextObject.activeSelf && s_PlayerTransform != null)
            s_TextObject.transform.position = s_PlayerTransform.position + s_OffsetHienTai;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (chiHienMotLan && daHien) return;

        // Đảm bảo shared object tồn tại (có thể null sau scene reload)
        if (s_TextObject == null)
        {
            KhoiTaoShared();
            // Cập nhật style từ trigger này
        }

        // Cập nhật player transform nếu cần
        if (s_PlayerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) s_PlayerTransform = p.transform;
        }

        daHien = true;

        // Dừng trigger đang chạy (nếu có) trước khi bắt đầu cái mới
        if (s_DangChay != null && s_Coroutine != null)
        {
            s_DangChay.StopCoroutine(s_Coroutine);
            s_Coroutine = null;
        }

        s_DangChay = this;
        s_OffsetHienTai = offsetText;

        // Áp style của trigger này
        s_TmpText.fontSize = coChu;
        s_TmpText.color = mauChu;
        s_TmpText.rectTransform.sizeDelta = new Vector2(rongToiDa / 100f, 10f);
        s_NenRenderer.color = mauNen;

        s_Coroutine = StartCoroutine(HienThiThoai());
    }

    IEnumerator HienThiThoai()
    {
        if (cacDongThoai == null || cacDongThoai.Length == 0)
            yield break;

        s_TextObject.SetActive(true);

        foreach (string dong in cacDongThoai)
        {
            // Nếu trigger này bị interrupt ở giữa, dừng lại
            if (s_DangChay != this) yield break;

            s_TmpText.text = dong;
            CapNhatKichThuocNen();

            yield return StartCoroutine(FadeText(0f, 1f, thoiGianFade));
            yield return new WaitForSeconds(thoiGianMoiDong);
            yield return StartCoroutine(FadeText(1f, 0f, thoiGianFade));
        }

        // Chỉ ẩn nếu vẫn là trigger đang active
        if (s_DangChay == this)
        {
            s_TextObject.SetActive(false);
            s_DangChay = null;
            s_Coroutine = null;
        }
    }

    void CapNhatKichThuocNen()
    {
        float chieuRong = Mathf.Min(
            s_TmpText.preferredWidth + 0.3f,
            rongToiDa / 100f + 0.3f);
        float chieuCao = s_TmpText.preferredHeight + 0.2f;

        s_NenObject.transform.localScale = new Vector3(chieuRong, chieuCao, 1f);
        s_NenObject.transform.localPosition = Vector3.zero;
        s_TmpText.transform.localPosition = new Vector3(0, 0, -0.1f);
    }

    IEnumerator FadeText(float tuAlpha, float denAlpha, float thoiGian)
    {
        float t = 0f;
        while (t < thoiGian)
        {
            if (s_DangChay != this) yield break;
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(tuAlpha, denAlpha, t / thoiGian);

            Color mauT = mauChu; mauT.a = alpha;
            s_TmpText.color = mauT;

            Color mauB = mauNen; mauB.a = mauNen.a * alpha;
            s_NenRenderer.color = mauB;

            yield return null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.96f, 0.78f, 0.26f, 0.3f);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);

#if UNITY_EDITOR
        if (cacDongThoai != null && cacDongThoai.Length > 0)
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                "[D] " + cacDongThoai[0]);
#endif
    }
}
