using UnityEngine;
using Debug = UnityEngine.Debug;

public class BoxArrivalHandler : MonoBehaviour
{
    [Header("References")]
    public StackingZone stackingZone;
    public float detectionRadius = 1f;

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Box")) continue;

            GameObject box = hit.gameObject;
            BoxMover mover = box.GetComponent<BoxMover>();

            if (mover == null || !mover.enabled) continue;

            // ── Stop the mover IMMEDIATELY before anything else ──
            mover.enabled = false;

            // ── Kill all physics movement ──
            Rigidbody rb = box.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // ── Untag so this box is never processed twice ──
            box.tag = "Untagged";

            Debug.Log("[ArrivalHandler] Box arrived, sending to Place2: " + box.name);

            if (stackingZone != null)
                stackingZone.AddBoxToStack(box);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}