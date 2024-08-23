using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceMaterial : MonoBehaviour
{
    public Renderer rend;
    public Material material;
    void Start()
    {
        rend.material = material;
    }
}
