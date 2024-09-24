using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour {

	bool recording;
	int count;

		void Start(){
			// Time.captureFramerate = 60;
		}

	    void Update()
	    {
			// if(Input.GetKeyDown(KeyCode.R)){
			// 	recording = !recording;
			// }

			// if(recording){
			// 	count ++;
			// 	ScreenCapture.CaptureScreenshot("gif/" + "frame" + count + ".png");
			// }

    
		#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				ScreenCapture.CaptureScreenshot("Screenshots/Screenie" + (int)Time.time + ".png", 2);
			}

		#endif
    
	    }
}
