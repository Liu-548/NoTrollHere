using UnityEngine;

public class KillZone : MonoBehaviour
{
    [Header("=== LOẠI BẪY ===")]
    // Điền tên bẫy để AchievementManager theo dõi
    // Các giá trị hợp lệ:
    // "Spike" "HiddenSpike" "FakePlatform" "FallingBrick"
    // "SpringTrap" "WallSpike" "BetrayingPlatform" ""
    public string loaiBay = "";

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (!vatTheChamVao.CompareTag("Player")) return;

        DeathEffect effect = vatTheChamVao.GetComponent<DeathEffect>();
        if (effect != null)
        {
            // Nếu đang chết rồi (dangChet=true) → bỏ qua hoàn toàn
            if (!effect.KichHoatHieuUng()) return;
        }
        else
        {
            // Không có DeathEffect → dùng guard của GameManager
            if (!GameManager.CoTheXuLyChet()) return;
            GameManager.instance.PlayerChet();
        }

        // Chỉ báo Achievement khi đây là cú chết thực sự (không bị block)
        if (AchievementManager.instance != null)
            AchievementManager.instance.KiemTraSauKhiChet(loaiBay);
    }
}