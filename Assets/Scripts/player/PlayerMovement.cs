using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 1f;
    public float turningSpeed = 1f;

    float speedIncreaseRate = 1000f;
    Vector3 currentVelocity = new Vector3(0, 0, 0);

    Rigidbody rb;
    CharacterController controller;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        LockRotation();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float turn = Input.GetAxis("Horizontal") * turningSpeed * Time.deltaTime;
        transform.TransformDirection(new Vector3(0, 0, turn));
        transform.Rotate(0, turn, 0);
        float move = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        //rb.AddForce(Vector3.back * move);
        transform.Translate(0, 0, move);
    }

    void LockRotation()
    {
        rb.constraints =
            RigidbodyConstraints.FreezeRotationZ
            | RigidbodyConstraints.FreezeRotationX
            | RigidbodyConstraints.FreezeRotationY;
        ;
    }
}
