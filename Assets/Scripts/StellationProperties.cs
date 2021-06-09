using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StellationProperties : MonoBehaviour
{
   [SerializeField] StellationController controller;

    public float distortion = 0.1f;
    public float amplitude = 1;
    public float frequency = 1;

    public float scrollSpeed = 10;

   public void Update(){
       Spline.shake = distortion;
       Spline.amplitude = amplitude;
       Spline.frequency = frequency;
       Spline.noiseSpeed = scrollSpeed;
   }
}
