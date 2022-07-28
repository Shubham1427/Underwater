using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public Vector2 sensitivity;
    public float verticalCameraRotationLimit;

    float x, y;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    // Update is called once per frame
    void Update()
    {
        x = Input.GetAxis("Mouse X");
        y = -Input.GetAxis("Mouse Y");

        float angleAfterRotation = transform.rotation.eulerAngles.x + y * sensitivity.y * Time.deltaTime;
        if (angleAfterRotation <= verticalCameraRotationLimit || angleAfterRotation >= 360f-verticalCameraRotationLimit)
            transform.Rotate(new Vector3 (y * sensitivity.y * Time.deltaTime, 0f, 0f), Space.Self);
        transform.parent.Rotate(new Vector3 (0f, x * sensitivity.x * Time.fixedDeltaTime, 0f), Space.World);
    }
}
