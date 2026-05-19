using UnityEngine;

// Script này gắn lên bất kỳ vật thể nào có thể giết Player
// Ví dụ: spike, hố, killzone dưới đất
// KHÔNG gắn lên FallingBrick — cái đó dùng BrickKillZone riêng
public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (vatTheChamVao.CompareTag("Player"))
        {
            DeathEffect effect = vatTheChamVao.GetComponent<DeathEffect>();

            if (effect != null)
            {
                // Có DeathEffect → chạy flash đỏ + shake trước
                effect.KichHoatHieuUng();
            }
            else
            {
                // Không có → chết thẳng
                GameManager.instance.PlayerChet();
            }
        }
    }
}