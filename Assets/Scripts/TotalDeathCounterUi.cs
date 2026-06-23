using UnityEngine;
using TMPro;

public class TotalDeathCounterUI : MonoBehaviour
{
    public TextMeshProUGUI deathText;

    void Update()
    {
        if (deathText == null) return;

        // Đọc thẳng từ PlayerPrefs — không phụ thuộc GameManager.instance
        int tongChet = PlayerPrefs.GetInt("Deaths_Total", 0);
        deathText.text = "TOTAL DEATHS : " + tongChet;
    }
}
