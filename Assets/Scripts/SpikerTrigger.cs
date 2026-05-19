using UnityEngine;

// Gắn lên TriggerZone (object con của HiddenSpike)
// Phát hiện Player bước vào rồi kích hoạt spike mọc lên
public class SpikeTrigger : MonoBehaviour
{
    // Tham chiếu tới HiddenSpike cha
    // Tự động lấy từ object cha — không cần kéo thả
    private HiddenSpike hiddenSpike;

    void Start()
    {
        // Lấy script HiddenSpike từ object cha
        hiddenSpike = GetComponentInParent<HiddenSpike>();
    }

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (vatTheChamVao.CompareTag("Player"))
        {
            // Báo HiddenSpike mọc lên
            hiddenSpike.KichHoat();
        }
    }
}