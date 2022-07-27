using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Ocean ocean;
    public bool isUnderwater
    {
        get
        {
            return transform.position.y + 0.125f <= ocean.oceanLevel;    
        }
    }
    public Camera playerCam;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isUnderwater)
        {
            // playerCam.clearFlags = CameraClearFlags.SolidColor;
            playerCam.backgroundColor = RenderSettings.fogColor;
            rb.useGravity = false;
        }
        else
        {
            // playerCam.clearFlags = CameraClearFlags.Skybox;
            rb.useGravity = true;
        }
    }
}
