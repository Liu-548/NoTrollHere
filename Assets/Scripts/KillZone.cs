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

        // Báo cho AchievementManager biết loại bẫy
        if (AchievementManager.instance != null)
            AchievementManager.instance.KiemTraSauKhiChet(loaiBay);

        DeathEffect effect = vatTheChamVao.GetComponent<DeathEffect>();
        if (effect != null)
            effect.KichHoatHieuUng();
        else
            GameManager.instance.PlayerChet();
    }
}