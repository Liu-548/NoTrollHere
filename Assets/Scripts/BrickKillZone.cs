using UnityEngine;

// Gắn lên object con BrickKillZone của FallingBrick
// Chỉ giết player khi gạch đang rơi
public class BrickKillZone : MonoBehaviour
{
    private FallingBrick fallingBrick;

    void Start()
    {
        // Lấy FallingBrick từ object cha
        fallingBrick = GetComponentInParent<FallingBrick>();
    }

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (vatTheChamVao.CompareTag("Player"))
        {
            // Chỉ giết khi gạch ĐANG RƠI
            // Nếu gạch chưa rơi thì bỏ qua hoàn toàn
            if (fallingBrick != null && fallingBrick.dangRoi)
            {
                DeathEffect effect = vatTheChamVao.GetComponent<DeathEffect>();

                if (effect != null)
                {
                    effect.KichHoatHieuUng();
                }
                else if (GameManager.CoTheXuLyChet())
                {
                    GameManager.instance.PlayerChet();
                }
            }
        }
    }
}