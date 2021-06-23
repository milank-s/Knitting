using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingTrigger : MonoBehaviour
{
    public bool unlockCam;
    
    // Update is called once per frame
    public void Fly()
    {
        // Services.mainCam.fieldOfView = 80;
		// CameraFollow.instance.desiredFOV = 80;
        if(unlockCam){
        	CameraFollow.instance.fixedCamera = false;
            CameraFollow.instance.lockX = false;
            CameraFollow.instance.lockY = false;
            CameraFollow.instance.lockZ = false;
        }

        Services.PlayerBehaviour.SwitchState(PlayerState.Flying);
    }
}
