using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Tạo hiệu ứng hạt phấn trôi nổi trên nền Main Menu
// Gắn lên một GameObject rỗng trong Canvas
public class ChalkParticle : MonoBehaviour
{
    [Header("=== CÀI ĐẶT HẠT PHẤN ===")]
    public int soLuongHat = 25;          // Số hạt trên màn
    public float tocDoMin = 8f;          // Tốc độ trôi tối thiểu
    public float tocDoMax = 25f;         // Tốc độ trôi tối đa
    public float doMoMin = 0.03f;        // Độ mờ tối thiểu
    public float doMoMax = 0.12f;        // Độ mờ tối đa
    public float kichThuocMin = 2f;      // Kích thước hạt tối thiểu
    public float kichThuocMax = 6f;      // Kích thước hạt tối đa

    private RectTransform rectTransform;
    private struct Hat
    {
        public RectTransform rect;
        public Image image;
        public float tocDo;
        public float pha; // phase cho twinkle
    }
    private Hat[] cacHat;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cacHat = new Hat[soLuongHat];

        for (int i = 0; i < soLuongHat; i++)
        {
            // Tạo GameObject hạt
            GameObject go = new GameObject("Hat_" + i);
            go.transform.SetParent(transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();

            // Màu phấn trắng ngà
            float doMo = Random.Range(doMoMin, doMoMax);
            img.color = new Color(0.91f, 0.91f, 0.816f, doMo); // #E8E8D0

            // Kích thước ngẫu nhiên
            float size = Random.Range(kichThuocMin, kichThuocMax);
            rt.sizeDelta = new Vector2(size, size);

            // Vị trí ngẫu nhiên trong canvas
            rt.anchoredPosition = new Vector2(
                Random.Range(-960f, 960f),
                Random.Range(-540f, 540f)
            );

            cacHat[i] = new Hat
            {
                rect = rt,
                image = img,
                tocDo = Random.Range(tocDoMin, tocDoMax),
                pha = Random.Range(0f, Mathf.PI * 2f)
            };
        }
    }

    void Update()
    {
        float time = Time.time;
        for (int i = 0; i < cacHat.Length; i++)
        {
            var hat = cacHat[i];

            // Trôi lên trên
            Vector2 pos = hat.rect.anchoredPosition;
            pos.y += hat.tocDo * Time.deltaTime;

            // Lắc nhẹ ngang
            pos.x += Mathf.Sin(time * 0.5f + hat.pha) * 0.3f;

            // Reset xuống dưới khi ra khỏi màn
            if (pos.y > 560f)
            {
                pos.y = -560f;
                pos.x = Random.Range(-960f, 960f);
            }

            hat.rect.anchoredPosition = pos;

            // Twinkle: thay đổi độ mờ theo sin
            float doMo = Mathf.Lerp(doMoMin, doMoMax,
                (Mathf.Sin(time * 1.2f + hat.pha) + 1f) * 0.5f);
            Color c = hat.image.color;
            c.a = doMo;
            hat.image.color = c;

            cacHat[i] = hat;
        }
    }
}