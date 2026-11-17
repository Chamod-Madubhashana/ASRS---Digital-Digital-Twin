using System;
using UnityEngine;

public class ConveyorSystem : MonoBehaviour
{
    public GameObject boxPrefab;
    public Transform startPoint1;
    public Transform startPoint2;
    public Transform tJunction1;
    public Transform tJunction2;
    public Transform lJunction;
    public Transform endPoint1;
    public Transform endPoint2;
    public StackingZone stackingZone; // ← ADD THIS
    public float speed = 2f;
    [NonSerialized] public float spawnDelay1 = 3f;
    [NonSerialized] public float spawnDelay2 = 3f;
    public float minDistance1 = 6f;
    public float minDistance2 = 3f;

    private GameObject lastBox1;
    private GameObject lastBox2;

    void Start()
    {
        InvokeRepeating("SpawnBox1", 0.5f, spawnDelay1);
        InvokeRepeating("SpawnBox2", 1.5f, spawnDelay2);
    }

    void SpawnBox1()
    {
        if (stackingZone != null && stackingZone.IsStackFull()) return;

        if (lastBox1 == null || Vector3.Distance(lastBox1.transform.position, startPoint1.position) > minDistance1)
        {
            GameObject box = Instantiate(boxPrefab, startPoint1.position, Quaternion.identity);
            BoxMover mover = box.AddComponent<BoxMover>();
            mover.Init(new Transform[] { startPoint1, tJunction1, endPoint1 }, speed);
            lastBox1 = box;
        }
    }

    void SpawnBox2()
    {
        if (stackingZone != null && stackingZone.IsStackFull()) return;

        if (lastBox2 == null || Vector3.Distance(lastBox2.transform.position, startPoint2.position) > minDistance2)
        {
            GameObject box = Instantiate(boxPrefab, startPoint2.position, Quaternion.identity);
            BoxMover mover = box.AddComponent<BoxMover>();
            mover.Init(new Transform[] { startPoint2, tJunction2, lJunction, endPoint2 }, speed);
            lastBox2 = box;
        }
    }
}