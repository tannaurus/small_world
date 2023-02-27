using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float movementSpeed = 1f;

    [SerializeField]
    float turningSpeed = 1f;

    [SerializeField]
    float jumpForce = 100f;

    Rigidbody rb;
    CharacterController controller;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        LockRotation();
    }

    // Update is called once per frame

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * 100 * jumpForce);
            Debug.Log("Jump!");
        }

        float turnMultiplier;
        RaycastHit hit;
        bool onGround = Physics.Raycast(
            transform.position,
            transform.TransformDirection(Vector3.down),
            out hit,
            1f
        );
        if (onGround)
        {
            turnMultiplier = 200f;
        }
        else
        {
            Debug.Log("in the air");
            turnMultiplier = 100f;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            Debug.Log("Rotating");
            float turn =
                Input.GetAxis("Horizontal") * turnMultiplier * turningSpeed * Time.deltaTime;
            transform.Rotate(0, turn, 0);
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            Debug.Log("Moving");
            float move = Input.GetAxis("Vertical") * 10 * movementSpeed * Time.deltaTime;
            transform.Translate(new Vector3(0, 0, move));
        }
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
