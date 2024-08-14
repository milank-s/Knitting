using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public void LoadLevel(string levelName){
        
        //save the current point position 
        
        //you should do this based on scene name
        int level = StellationManager.instance.level;
		PlayerPrefs.SetInt("level", level);
		PlayerPrefs.SetInt("checkpoint", StellationManager.instance.stellationSets[level].controllers.IndexOf(Services.main.activeStellation));
        int curPointIndex = Services.main.activeStellation._points.IndexOf(Services.PlayerBehaviour.curPoint);

		PlayerPrefs.SetInt("pointIndex", curPointIndex);
        SceneController.instance.LoadScene(levelName);
    }
}
