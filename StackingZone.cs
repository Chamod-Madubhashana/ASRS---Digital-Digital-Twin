using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StackingZone : MonoBehaviour
{
    [Header("Stack Settings")]
    public Transform stackBase;
    public float boxHeight = 1f;
    public int maxStackSize = 10; // conveyor stops feeding if stack exceeds this

    [Header("References")]
    public ASRS asrs;

    // Only boxes currently IN the stack — placed boxes are removed completely
    private List<GameObject> stack = new List<GameObject>();

    // Track which boxes are already in rack — never touch these again
    private HashSet<int> placedBoxIDs = new HashSet<int>();

    public void AddBoxToStack(GameObject box)
    {
        // Already placed in rack — ignore completely
        if (placedBoxIDs.Contains(box.GetInstanceID())) return;

        // Destroy BoxMover permanently
        BoxMover mover = box.GetComponent<BoxMover>();
        if (mover != null) Destroy(mover);

        // Freeze physics
        Rigidbody rb = box.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Insert at bottom
        stack.Insert(0, box);

        // Rebuild only stack boxes — never touches rack boxes
        RebuildStackPositions();

        Debug.Log("[StackingZone] Box added. Stack size: " + stack.Count);

        if (asrs != null)
        {
            box.tag = "Box";
            asrs.EnqueueBox(stack[0]);
        }
    }

    void RebuildStackPositions()
    {
        for (int i = 0; i < stack.Count; i++)
        {
            if (stack[i] == null) continue;
            // Only move boxes that are NOT placed in rack
            if (placedBoxIDs.Contains(stack[i].GetInstanceID())) continue;
            stack[i].transform.SetParent(null);
            stack[i].transform.position = stackBase.position + Vector3.up * (i * boxHeight);
            stack[i].transform.rotation = Quaternion.identity;
        }
    }

    public GameObject PopBottomBox()
    {
        // Clean null entries first
        stack.RemoveAll(b => b == null);

        if (stack.Count == 0) return null;

        GameObject bottom = stack[0];
        stack.RemoveAt(0);
        bottom.transform.SetParent(null);

        RebuildStackPositions();

        Debug.Log("[StackingZone] Bottom popped. Remaining: " + stack.Count);
        return bottom;
    }

    // Call this from ASRS after placing in rack
    public void RegisterPlacedBox(GameObject box)
    {
        placedBoxIDs.Add(box.GetInstanceID());
        // Make sure it is not in stack list
        stack.Remove(box);
        Debug.Log("[StackingZone] Box registered as placed. ID: " + box.GetInstanceID());
    }

    public GameObject GetBottomBox()
    {
        stack.RemoveAll(b => b == null);
        if (stack.Count == 0) return null;
        return stack[0];
    }

    // Returns true if conveyor should pause spawning
    public bool IsStackFull() => stack.Count >= maxStackSize;

    public int GetStackCount() => stack.Count;

    void OnDrawGizmos()
    {
        if (stackBase == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(stackBase.position, Vector3.one * 0.5f);
    }
}