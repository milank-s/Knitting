using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatePoints : MonoBehaviour
{

    public bool sineWave;
    public bool perlin;
   [SerializeField] Spline[] splines;
   public float amplitude = 0.25f;
   public float frequency = 1;
   public float scrollSpeed = 1;

   void Update(){
       foreach(Spline s in splines){
       
       int i = 0;
            foreach(Point p in s.SplinePoints){
                
                Vector3 pos = Vector3.zero;
                Vector3 direction = s.GetVelocityAtIndex(i, 0).normalized;
                Vector3 moveDir = new Vector3(-direction.y, direction.x, direction.z);

                if(sineWave){
                    pos += moveDir * Mathf.Sin(Time.time * scrollSpeed * Mathf.PI * 2 + (float)i * frequency) * amplitude;
                }

                if(perlin){
                    pos += moveDir * (Mathf.PerlinNoise((-Time.time * scrollSpeed) + ((float) i * frequency), 2f) * 2f - 1f);
                }

                p.transform.position = p.anchorPos + pos;

                i++;
            }
       }
   }
}
