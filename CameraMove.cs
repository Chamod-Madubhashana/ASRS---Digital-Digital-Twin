using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // Update is called once per frame
    public float speed = 10f;
    public float mouseSensitivity = 300f;

    float rotationX = 0f;
    float rotationY = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    

    void Update()
    {
        // -------- Keyboard Movement --------
        if (Input.GetKey(KeyCode.I)) // Forward
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.K)) // Backward
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.J)) // Left
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.L)) // Right
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }

        // -------- Mouse Rotation --------
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationY += mouseX;

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}
