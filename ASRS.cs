using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ASRS : MonoBehaviour
{
    [Header("ASRS Parts")]
    public Transform part1;
    public Transform part2;
    public Transform part3;
    public Transform grabPoint;

    [Header("Speeds")]
    public float speed1 = 5f;
    public float speed2 = 5f;
    public float speed3 = 5f;

    [Header("Clamp Limits")]
    [NonSerialized] public float minX = -12f, maxX = 10f;
    [NonSerialized] public float minY = -8.5f, maxY = 3.2f;
    [NonSerialized] public float minZ = -1f, maxZ = 3f;
    public float zOffset = 0f;



    [Header("Grab Settings")]
    public float grabDistance = 0.6f;
    [NonSerialized] public float retractZ = 1.5f;

    [NonSerialized] public float xAdjust = -5f; // shift all X targets by this amount

    [Header("References")]
    public RackManager rackManager;
    public StackingZone stackingZone;

    private float offsetX2, offsetX3;
    private float relativeOffsetY2, relativeOffsetY3;

    public enum ASRSMode { Manual, Auto }
    [HideInInspector] public ASRSMode mode = ASRSMode.Manual;

    private GameObject heldBox = null;
    private Queue<GameObject> boxQueue = new Queue<GameObject>();
    private bool autoRunning = false;

    void Start()
    {
        if (part1 != null)
        {
            if (part2 != null) offsetX2 = part2.position.x - part1.position.x;
            if (part3 != null) offsetX3 = part3.position.x - part1.position.x;
        }
        if (part2 != null && part3 != null)
        {
            float cy = (part2.position.y + part3.position.y) / 2f;
            relativeOffsetY2 = part2.position.y - cy;
            relativeOffsetY3 = part3.position.y - cy;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            mode = (mode == ASRSMode.Manual) ? ASRSMode.Auto : ASRSMode.Manual;
            Debug.Log("[ASRS] Mode: " + mode);
        }

        if (mode == ASRSMode.Manual)
        {
            HandleManualInput();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (heldBox == null) TryManualGrab();
                else ManualRelease();
            }
        }
        else
        {
            if (!autoRunning && boxQueue.Count > 0)
                StartCoroutine(AutoCycle(boxQueue.Dequeue()));
        }

        // ── Box follows grabPoint every frame while held ──
        if (heldBox != null)
        {
            Transform gp = grabPoint != null ? grabPoint : part3;
            heldBox.transform.position = gp.position;
        }
    }

    public void EnqueueBox(GameObject box)
    {
        boxQueue.Enqueue(box);
        Debug.Log("[ASRS] Box queued. Queue size: " + boxQueue.Count);
    }

    void TryManualGrab()
    {
        Transform gp = grabPoint != null ? grabPoint : part3;
        Collider[] hits = Physics.OverlapSphere(gp.position, grabDistance);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Box"))
            {
                AttachBox(hit.gameObject);
                return;
            }
        }
        Debug.Log("[ASRS] No box within grab range.");
    }

    void ManualRelease()
    {
        if (heldBox == null) return;
        if (rackManager != null)
        {
            Transform nearest = rackManager.GetNearestFreeSlot(
                grabPoint != null ? grabPoint.position : part3.position, 1.0f);
            if (nearest != null)
            {
                PlaceBoxInSlot(nearest);
                return;
            }
        }
        heldBox = null;
    }

    IEnumerator AutoCycle(GameObject box)
    {
        autoRunning = true;

        // ── Get bottom box from stack ──
        if (stackingZone != null)
        {
            GameObject bottom = stackingZone.GetBottomBox();
            if (bottom != null)
                box = bottom;
            else
            {
                Debug.LogWarning("[ASRS] Stack empty, skipping.");
                autoRunning = false;
                yield break;
            }
        }

        // ── Move to box ──
        yield return StartCoroutine(MoveXTo(5f));
        yield return StartCoroutine(MoveYTo(box.transform.position.y - relativeOffsetY3));
        yield return StartCoroutine(MoveZTo(box.transform.position.z));

        // ── Pop and grab ──
        if (stackingZone != null)
            box = stackingZone.PopBottomBox();

        if (box == null)
        {
            autoRunning = false;
            yield break;
        }

        AttachBox(box);
        yield return new WaitForSeconds(0.3f);

        // ── Retract ──
        yield return StartCoroutine(MoveZTo(retractZ + zOffset));

        // ── Find slot ──
        Transform slot = rackManager != null ? rackManager.GetRandomFreeSlot() : null;
        if (slot == null)
        {
            Debug.LogWarning("[ASRS] No free rack slot!");
            autoRunning = false;
            yield break;
        }

        // ── Move to slot ──
        yield return StartCoroutine(MoveXTo(slot.position.x - offsetX3));
        yield return StartCoroutine(MoveYTo(slot.position.y - relativeOffsetY3));
        yield return StartCoroutine(MoveZTo(slot.position.z));

        yield return new WaitForSeconds(0.3f);

        // ── Place box ──
        PlaceBoxInSlot(slot);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(MoveZTo(retractZ + zOffset));

        Debug.Log("[ASRS] Cycle complete → " + slot.name);
        autoRunning = false;
    }

    void AttachBox(GameObject box)
    {
        heldBox = box;

        // Destroy mover permanently
        BoxMover mover = box.GetComponent<BoxMover>();
        if (mover != null) Destroy(mover);

        // Freeze physics, no parenting
        Rigidbody rb = box.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        box.tag = "Untagged";
        Debug.Log("[ASRS] Attached: " + box.name);
    }

    void PlaceBoxInSlot(Transform slot)
    {
        if (heldBox == null) return;

        GameObject box = heldBox;
        heldBox = null;

        // Tell StackingZone this box is done — never touch it again
        if (stackingZone != null)
            stackingZone.RegisterPlacedBox(box);

        // Destroy mover
        BoxMover mover = box.GetComponent<BoxMover>();
        if (mover != null) Destroy(mover);

        // Freeze completely
        Rigidbody rb = box.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Disable colliders while snapping
        Collider[] cols = box.GetComponents<Collider>();
        foreach (var col in cols) col.enabled = false;

        // Snap to slot
        box.transform.SetParent(null);
        box.transform.position = slot.position;
        box.transform.rotation = Quaternion.identity;

        // Re-enable colliders
        foreach (var col in cols) col.enabled = true;

        // Ensure renderer on
        Renderer rend = box.GetComponent<Renderer>();
        if (rend != null) rend.enabled = true;

        if (rackManager != null)
            rackManager.OccupySlot(slot, box);

        StartCoroutine(VerifyBox(box, slot));

        Debug.Log("[ASRS] Box placed at slot: " + slot.name + " pos: " + slot.position);
    }

    IEnumerator VerifyBox(GameObject box, Transform slot)
    {
        yield return new WaitForSeconds(0.5f);

        if (box == null)
        {
            Debug.LogError("[ASRS] BOX WAS DESTROYED after placing! Check for Destroy() in other scripts.");
            yield break;
        }

        // Force position again in case something moved it
        box.transform.position = slot.position;

        Renderer rend = box.GetComponent<Renderer>();
        if (rend != null && !rend.enabled)
        {
            Debug.LogError("[ASRS] BOX RENDERER WAS DISABLED — re-enabling.");
            rend.enabled = true;
        }

        // Make absolutely sure it is frozen
        Rigidbody rb = box.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Debug.Log("[ASRS] Verified box at: " + box.transform.position +
                  " | Active: " + box.activeInHierarchy +
                  " | Renderer: " + (rend != null ? rend.enabled.ToString() : "none"));
    }

    void DetachBox()
    {
        if (heldBox == null) return;

        BoxMover mover = heldBox.GetComponent<BoxMover>();
        if (mover != null) Destroy(mover);

        Rigidbody rb = heldBox.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        heldBox = null;
    }

    IEnumerator MoveXTo(float targetX)
    {
        targetX = Mathf.Clamp(targetX, minX, maxX);
        while (Mathf.Abs(part1.position.x - targetX) > 0.04f)
        {
            SetX(Mathf.MoveTowards(part1.position.x, targetX, speed1 * Time.deltaTime));
            yield return null;
        }
        SetX(targetX);
    }

    IEnumerator MoveYTo(float targetCenterY)
    {
        targetCenterY = Mathf.Clamp(targetCenterY, minY, maxY);
        while (true)
        {
            float cy = (part2.position.y + part3.position.y) / 2f;
            if (Mathf.Abs(cy - targetCenterY) < 0.04f) break;
            SetCenterY(Mathf.MoveTowards(cy, targetCenterY, speed2 * Time.deltaTime));
            yield return null;
        }
        SetCenterY(targetCenterY);
    }

    IEnumerator MoveZTo(float targetZ)
    {
        targetZ = Mathf.Clamp(targetZ, minZ + zOffset, maxZ + zOffset);

        while (Mathf.Abs(part3.position.z - targetZ) > 0.04f)
        {
            float newZ = Mathf.MoveTowards(
                part3.position.z,
                targetZ,
                speed3 * Time.deltaTime
            );

            part3.position = new Vector3(
                part3.position.x,
                part3.position.y,
                newZ
            );

            yield return null;
        }

        part3.position = new Vector3(
            part3.position.x,
            part3.position.y,
            targetZ
        );
    }

    void SetX(float x)
    {
        part1.position = new Vector3(x, part1.position.y, part1.position.z);
        if (part2 != null) part2.position = new Vector3(x + offsetX2, part2.position.y, part2.position.z);
        if (part3 != null) part3.position = new Vector3(x + offsetX3, part3.position.y, part3.position.z);
    }

    void SetCenterY(float cy)
    {
        part2.position = new Vector3(part2.position.x, cy + relativeOffsetY2, part2.position.z);
        part3.position = new Vector3(part3.position.x, cy + relativeOffsetY3, part3.position.z);
    }

    void HandleManualInput()
    {
        if (part1 != null)
        {
            float move1 = Input.GetAxis("Horizontal");
            float newX = Mathf.Clamp(part1.position.x + move1 * speed1 * Time.deltaTime, minX, maxX);
            SetX(newX);
        }

        if (part2 != null && part3 != null)
        {
            float move2 = Input.GetAxis("Vertical");
            float cy = (part2.position.y + part3.position.y) / 2f;
            cy = Mathf.Clamp(cy + move2 * speed2 * Time.deltaTime, minY, maxY);
            SetCenterY(cy);
        }

        if (part3 != null)
        {
            float zDelta = 0f;
            if (Input.GetKey(KeyCode.Q)) zDelta = -1f;
            if (Input.GetKey(KeyCode.E)) zDelta = 1f;
            float newZ = Mathf.Clamp(part3.position.z + zDelta * speed3 * Time.deltaTime, minZ, maxZ);
            part3.position = new Vector3(part3.position.x, part3.position.y, newZ);
        }
    }
}