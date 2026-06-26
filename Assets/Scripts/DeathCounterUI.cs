using UnityEngine;
using TMPro;

public class DeathCounterUI : MonoBehaviour
{
    public TextMeshProUGUI deathText;

    void Awake()
    {
        if (deathText == null)
            deathText = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        // Set text ngay frame đầu để không flicker với default của prefab
        CapNhat();
    }

    void Update()
    {
        CapNhat();
    }

    void CapNhat()
    {
        if (deathText == null) return;
        // Gọi static — không cần GameManager.instance, không bị ảnh hưởng bởi DDOL
        deathText.text = "DEATHS : " + GameManager.LaySoLanChet();
    }
}