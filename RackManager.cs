using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RackManager : MonoBehaviour
{
    [System.Serializable]
    public class RackSlot
    {
        public Transform slotTransform;
        [HideInInspector] public bool isOccupied = false;
        [HideInInspector] public GameObject storedBox = null;
    }

    [Header("Rack Slots")]
    public List<RackSlot> slots = new List<RackSlot>();

    public Transform GetNextFreeSlot()
    {
        foreach (var slot in slots)
            if (!slot.isOccupied && slot.slotTransform != null)
                return slot.slotTransform;
        return null;
    }

    public Transform GetNearestFreeSlot(Vector3 worldPos, float maxDist = Mathf.Infinity)
    {
        Transform best = null;
        float bestDist = maxDist;

        foreach (var slot in slots)
        {
            if (slot.isOccupied || slot.slotTransform == null) continue;
            float d = Vector3.Distance(worldPos, slot.slotTransform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = slot.slotTransform;
            }
        }
        return best;
    }

    public void OccupySlot(Transform slotTransform, GameObject box)
    {
        foreach (var slot in slots)
        {
            if (slot.slotTransform == slotTransform)
            {
                slot.isOccupied = true;
                slot.storedBox = box;
                Debug.Log("[RackManager] Slot occupied: " + slotTransform.name);
                return;
            }
        }
    }

    public void FreeSlot(Transform slotTransform)
    {
        foreach (var slot in slots)
        {
            if (slot.slotTransform == slotTransform)
            {
                slot.isOccupied = false;
                slot.storedBox = null;
                return;
            }
        }
    }

    void OnDrawGizmos()
    {
        foreach (var slot in slots)
        {
            if (slot.slotTransform == null) continue;
            Gizmos.color = slot.isOccupied ? Color.red : Color.green;
            Gizmos.DrawWireCube(slot.slotTransform.position, Vector3.one * 0.4f);
        }
    }

    public Transform GetRandomFreeSlot()
    {
        List<RackSlot> freeSlots = new List<RackSlot>();

        foreach (var slot in slots)
            if (!slot.isOccupied && slot.slotTransform != null)
                freeSlots.Add(slot);

        if (freeSlots.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, freeSlots.Count);
        Debug.Log("[RackManager] Random slot chosen: " + freeSlots[randomIndex].slotTransform.name);
        return freeSlots[randomIndex].slotTransform;
    }
}