using UnityEngine;
using TMPro;

public class TotalDeathCounterUI : MonoBehaviour
{
    public TextMeshProUGUI deathText;

    void Update()
    {
        if (GameManager.instance == null) return;
        deathText.text = "DEATHS THIS SESSION : "
            + GameManager.instance.LayTongSoLanChetTatCa();
    }
}