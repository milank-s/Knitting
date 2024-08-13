using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTrigger : MonoBehaviour
{
    public bool unlockCam;
    
    public void Fly()
    {
        if(unlockCam){
            CameraFollow.instance.lockX = false;
            CameraFollow.instance.lockY = false;
            CameraFollow.instance.lockZ = false;
        }

        Services.PlayerBehaviour.SwitchState(PlayerState.Flying);
    }
}
