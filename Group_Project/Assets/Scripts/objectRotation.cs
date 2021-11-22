using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectRotation : MonoBehaviour
{
    public float turnSpeed = 10;

    void Update()
    {
        transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
    }
}
