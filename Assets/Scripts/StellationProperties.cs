using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StellationProperties : MonoBehaviour
{
   [SerializeField] StellationController controller;

    public float distortion = 0;

   public void Start(){
       Spline.shake = distortion;
   }
}
