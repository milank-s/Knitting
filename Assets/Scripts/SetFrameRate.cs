using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFrameRate : MonoBehaviour
{
    public SpriteAnimation animationComponent;
    public float multiplier = 2;
    
    public void Update(){
        animationComponent.framerate = Services.PlayerBehaviour.curSpeed * multiplier;
    }
}
