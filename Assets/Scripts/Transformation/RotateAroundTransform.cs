using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundTransform : MonoBehaviour
{
    
    
    public Transform pivot;
    public float speed = 100;

    public Vector3 axis = Vector3.up;
    
    
    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(pivot.position, axis, speed * Time.deltaTime);
    }
}
