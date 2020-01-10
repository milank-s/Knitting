using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Rotate : MonoBehaviour
{

    public Vector3 speed;
    
    void Update()
    {
        Vector3 s = speed * Time.deltaTime;
        transform.Rotate(s.x, s.y, s.z);
    }
}
