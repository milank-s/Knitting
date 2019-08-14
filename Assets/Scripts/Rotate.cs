using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 speed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 s = speed * Time.deltaTime;
        transform.Rotate(s.x, s.y, s.z);
    }
}
