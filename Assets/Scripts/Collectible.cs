using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    bool collected = false; 
    Vector3 startPos;

    public void Reset(){
        transform.position = startPos;
        collected = false;
    }

    public void OnPickup(){
        collected = true;
    }
}
