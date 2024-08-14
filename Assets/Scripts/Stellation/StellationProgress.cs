using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.SceneManagement;
public class StellationProgress : MonoBehaviour
{
        
    public void Start(){
        //do something if we're complete
        //to make it obvious on the map
        //unlock points etc
        
        string levelName = SceneController.curLevelName;
        JSONNode saveData = SaveGame.Load();

        if(StellationManager.instance != null){
            //set player starting position

            if(StellationManager.instance != null){
                StellationManager.instance.checkpoint = saveData[levelName]["checkpoint"];
                StellationManager.instance.startPoint = saveData[levelName]["startPoint"];
            }
            
            StellationManager.instance.Setup();

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
