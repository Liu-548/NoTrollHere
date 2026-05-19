using UnityEngine;

// Gắn lên TriggerZone (object con của FallingBrick)
// Phát hiện Player đi bên dưới rồi kích hoạt gạch rơi
public class BrickTrigger : MonoBehaviour
{
    private FallingBrick fallingBrick;

    void Start()
    {
        // Lấy script FallingBrick từ object cha
        fallingBrick = GetComponentInParent<FallingBrick>();
    }

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (vatTheChamVao.CompareTag("Player"))
        {
            fallingBrick.KichHoatRoi();
        }
    }
}