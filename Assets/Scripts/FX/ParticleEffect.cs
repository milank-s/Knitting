using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    public ParticleSystem particles;
    public Transform target;
    public bool hasTarget = false;
    public void Initialize(){
        
        particles.Play();
    }
   
    void Update()
    {
        if(hasTarget){
            if(target == null){
                hasTarget = false;
                ParticleSystem.MainModule main = particles.main;
                main.loop = false;
            }else{
                transform.position = target.position;
                transform.rotation = target.rotation;
            }
        }
        
        if(!particles.isPlaying && !hasTarget){
            Destroy(gameObject);
        }
    }
}
