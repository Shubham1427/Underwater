using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerSwimController : MonoBehaviour
{
    public float swimSpeed;
    Player player;

    float h, f, v;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        h = Input.GetAxis("Horizontal");
        f = Input.GetAxis("Vertical");
        v = Input.GetAxis("AcsDec");
    }

    void FixedUpdate ()
    {
        Vector3 velocity = player.playerCam.transform.forward * f + transform.right * h + Vector3.up * v;
        velocity = velocity.normalized;
        player.rb.MovePosition(player.rb.position + velocity * swimSpeed * Time.fixedDeltaTime);
    }
}
