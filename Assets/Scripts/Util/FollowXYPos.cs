using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowXYPos : MonoBehaviour
{
    [SerializeField] Transform target;

    private Vector3 offset;
    
    void Start()
    {
        offset = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = target.position;
        pos.z = 0;
        transform.position = pos + offset;
    }
}
