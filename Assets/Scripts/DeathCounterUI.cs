using UnityEngine;
using TMPro;

// Script này cập nhật text số lần chết lên màn hình
// Gắn lên object DeathCounterText
public class DeathCounterUI : MonoBehaviour
{
    // Tham chiếu tới component TextMeshPro
    // Kéo object DeathCounterText vào ô này trong Inspector
    public TextMeshProUGUI deathText;

    // Update chạy mỗi frame — liên tục cập nhật số chết
    void Update()
    {
        // Lấy số lần chết từ GameManager rồi hiển thị lên text
        // Dạng: "DEATHS: 7"
        deathText.text = "DEATHS: " + GameManager.instance.LaySoLanChet();
    }
}