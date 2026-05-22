using UnityEngine;

// Gắn lên child GameObject của TrapManager
// Child cần có BoxCollider2D Is Trigger = ON
public class TrapManagerTrigger : MonoBehaviour
{
    private TrapManager trapManager;

    void Start()
    {
        trapManager = GetComponentInParent<TrapManager>();
        if (trapManager == null)
            Debug.LogError(name + ": Không tìm thấy TrapManager ở object cha!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (trapManager != null)
            trapManager.KichHoatTuTrigger();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
            Gizmos.DrawWireCube(
                transform.position + (Vector3)box.offset,
                box.size);
    }
}