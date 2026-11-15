
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;

    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;

    private float verticalRot = 0;

    void Update()
    {
        if (axes == RotationAxes.MouseX)
        {
            float rotationX = Input.GetAxis("Mouse X") * sensitivityHor;

            transform.Rotate(0, rotationX, 0);
        }
        else if (axes == RotationAxes.MouseY)
        {
            verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVert;

            verticalRot = Mathf.Clamp(
                verticalRot,
                minimumVert,
                maximumVert
            );

            transform.localEulerAngles = new Vector3(
                verticalRot, 0, 0
            );
        }
    }
}
