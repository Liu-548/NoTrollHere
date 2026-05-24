using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class AchievementPopup : MonoBehaviour
{
    public static AchievementPopup instance;

    [Header("=== UI ===")]
    public GameObject panelPopup;
    public TextMeshProUGUI txtTen;
    public TextMeshProUGUI txtMoTa;
    public TextMeshProUGUI txtNhanXet;

    [Header("=== CÀI ĐẶT ===")]
    public float thoiGianHien = 3f;
    public float thoiGianFade = 0.4f;

    private CanvasGroup canvasGroup;
    private bool dangHien = false;
    private struct PopupData
    {
        public string ten, moTa, nhanXet;
    }
    private Queue<PopupData> hangDoi = new Queue<PopupData>();

    void Awake()
    {
        instance = this;
        canvasGroup = panelPopup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panelPopup.AddComponent<CanvasGroup>();
        panelPopup.SetActive(false);
    }

    public void HienThiPopup(string ten, string moTa, string nhanXet)
    {
        hangDoi.Enqueue(new PopupData { ten = ten, moTa = moTa, nhanXet = nhanXet });
        if (!dangHien)
            StartCoroutine(ChayPopup());
    }

    IEnumerator ChayPopup()
    {
        dangHien = true;

        while (hangDoi.Count > 0)
        {
            PopupData data = hangDoi.Dequeue();

            txtTen.text = "🏆 " + data.ten;
            txtMoTa.text = data.moTa;
            txtNhanXet.text = data.nhanXet;

            panelPopup.SetActive(true);
            canvasGroup.alpha = 0f;

            // Fade in
            float t = 0f;
            while (t < thoiGianFade)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / thoiGianFade);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(thoiGianHien);

            // Fade out
            t = 0f;
            while (t < thoiGianFade)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / thoiGianFade);
                yield return null;
            }

            panelPopup.SetActive(false);
            yield return new WaitForSecondsRealtime(0.3f);
        }

        dangHien = false;
    }
}