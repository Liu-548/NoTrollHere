using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChalkParticle : MonoBehaviour
{
    [Header("=== CÀI ĐẶT HẠT PHẤN ===")]
    public int soLuongHat = 25;
    public float tocDoMin = 8f;
    public float tocDoMax = 25f;
    public float doMoMin = 0.03f;
    public float doMoMax = 0.12f;
    public float kichThuocMin = 2f;
    public float kichThuocMax = 6f;

    // Màu mặc định — có thể đổi từ ngoài
    [Header("=== MÀU HẠT ===")]
    public Color mauHat = new Color(0.91f, 0.91f, 0.816f, 1f); // #E8E8D0

    private RectTransform rectTransform;

    private struct Hat
    {
        public RectTransform rect;
        public Image image;
        public float tocDo;
        public float pha;
    }

    private Hat[] cacHat;

    // Màu các chương — index khớp với LevelSelectManager
    // 0=Special, 1=Ch1, 2=Ch2, 3=Ch3, 4=Ch4
    private static readonly Color[] mauTheoChuong =
    {
        new Color(0.91f, 0.78f, 0.94f, 1f), // Special: tím nhạt
        new Color(0.91f, 0.91f, 0.82f, 1f), // Ch1: trắng ngà
        new Color(0.78f, 0.91f, 0.82f, 1f), // Ch2: xanh ngà
        new Color(0.91f, 0.82f, 0.78f, 1f), // Ch3: đỏ ngà
        new Color(0.78f, 0.85f, 0.91f, 1f), // Ch4: xanh dương ngà
    };

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cacHat = new Hat[soLuongHat];

        for (int i = 0; i < soLuongHat; i++)
        {
            GameObject go = new GameObject("Hat_" + i);
            go.transform.SetParent(transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();

            float doMo = Random.Range(doMoMin, doMoMax);
            img.color = new Color(mauHat.r, mauHat.g, mauHat.b, doMo);

            float size = Random.Range(kichThuocMin, kichThuocMax);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(
                Random.Range(-960f, 960f),
                Random.Range(-540f, 540f));

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
        if (cacHat == null) return; // chưa Start() xong
        float time = Time.time;
        for (int i = 0; i < cacHat.Length; i++)
        {
            var hat = cacHat[i];

            Vector2 pos = hat.rect.anchoredPosition;
            pos.y += hat.tocDo * Time.deltaTime;
            pos.x += Mathf.Sin(time * 0.5f + hat.pha) * 0.3f;

            if (pos.y > 560f)
            {
                pos.y = -560f;
                pos.x = Random.Range(-960f, 960f);
            }

            hat.rect.anchoredPosition = pos;

            float doMo = Mathf.Lerp(doMoMin, doMoMax,
                (Mathf.Sin(time * 1.2f + hat.pha) + 1f) * 0.5f);
            Color c = hat.image.color;
            c.r = mauHat.r;
            c.g = mauHat.g;
            c.b = mauHat.b;
            c.a = doMo;
            hat.image.color = c;

            cacHat[i] = hat;
        }
    }

    // =============================================================
    // ĐỔI MÀU THEO CHƯƠNG — gọi từ LevelSelectManager
    // =============================================================
    public void DoiMauTheoChuong(int indexChuong)
    {
        if (indexChuong >= 0 && indexChuong < mauTheoChuong.Length)
            mauHat = mauTheoChuong[indexChuong];
    }

    public void DoiMau(Color mauMoi)
    {
        mauHat = mauMoi;
    }
}