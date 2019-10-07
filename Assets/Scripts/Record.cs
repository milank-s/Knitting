using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record : MonoBehaviour {

	int count;

		void Start(){
			Time.captureFramerate = 60;
		}

	    void Update()
	    {
					count ++;
	        ScreenCapture.CaptureScreenshot("gif/" + "frame" + count + ".png");
	    }
}
