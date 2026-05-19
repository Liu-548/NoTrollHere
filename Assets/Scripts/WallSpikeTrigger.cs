using UnityEngine;

// Gắn lên TriggerZone (con của WallSpike)
public class WallSpikeTrigger : MonoBehaviour
{
    private WallSpike wallSpike;

    void Start()
    {
        wallSpike = GetComponentInParent<WallSpike>();
    }

    void OnTriggerEnter2D(Collider2D vatTheChamVao)
    {
        if (vatTheChamVao.CompareTag("Player"))
        {
            wallSpike.KichHoatBan();
        }
    }
}