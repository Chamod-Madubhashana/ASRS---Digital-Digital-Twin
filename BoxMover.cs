using UnityEngine;
using System;

public class BoxMover : MonoBehaviour
{
    private Transform[] path;
    private int index = 0;
    private float speed;
    private Action onComplete;
    private bool finished = false;

    public void Init(Transform[] waypoints, float moveSpeed, Action completionCallback = null)
    {
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();

        path = waypoints;
        speed = moveSpeed;
        index = 0;
        finished = false;
        onComplete = completionCallback;
    }

    void Update()
    {
        if (path == null || index >= path.Length || finished) return;

        Transform target = path[index];
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            transform.position = target.position;
            index++;

            if (index >= path.Length)
            {
                if (onComplete != null)
                {
                    Action callback = onComplete;
                    onComplete = null;
                    callback.Invoke();
                }
                else
                {
                    finished = true;
                }
            }
        }
    }

    public void SetNewPath(Transform[] newWaypoints, Action completionCallback = null)
    {
        path = newWaypoints;
        index = 0;
        finished = false;
        onComplete = completionCallback;
    }
}