
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPS Input")]

public class FPSInput : MonoBehaviour
{
    public float speed = 6.0f;
    public float gravity = -9.8f;
    public float jumpHeight = 2.0f;

    private CharacterController charController;
    private float verticalVelocity;

    void Start()
    {
        charController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Get keyboard input
        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;

        Vector3 movement = new Vector3(deltaX, 0, deltaZ);

        movement = Vector3.ClampMagnitude(movement, speed);

        // Check if player is on the ground
        /*if (charController.isGrounded)
        {
            verticalVelocity = -2f;

            // Jump using Spacebar
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
            }
        }*/

        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;

        movement.y = verticalVelocity;

        // Move according to player direction
        movement = transform.TransformDirection(movement);

        charController.Move(movement * Time.deltaTime);
    }
}
