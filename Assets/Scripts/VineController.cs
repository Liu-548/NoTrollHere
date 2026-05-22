using UnityEngine;

// Gắn lên Tilemap "Vine" — Layer Vine, Is Trigger ON
// Player chạm vào vine → có thể leo lên/xuống bằng W/S hoặc ↑/↓
public class VineController : MonoBehaviour
{
    [Header("=== TỐC ĐỘ ===")]
    public float tocDoLeo = 4f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null) pc.BatDauLeoDay(tocDoLeo);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null) pc.KetThucLeoDay();
    }
}