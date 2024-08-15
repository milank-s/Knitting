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
        SaveGame.Load();

        if(StellationManager.instance != null){
            //set player starting position

            if(StellationManager.instance != null){
                StellationManager.instance.checkpoint = SaveGame.data[levelName]["checkpoint"];
                StellationManager.instance.startPoint = SaveGame.data[levelName]["startPoint"];
            }
            
            StellationManager.instance.Setup();
        }

        
        //we need to get the level we just came from
        //and check if it was completed

        // bool markComplete = SaveGame.data[lastLevel]["complete"];
    }

    public void MarkComplete(){

    }

    public void TryLoadLevel(string levelName){
        
        if(isComplete(levelName)) return;
        
        SaveGame.Save();
        SceneController.instance.LoadScene(levelName);
        
    }

    public bool isComplete(string levelName){

        if(SaveGame.data[levelName] == null) return false;
        return SaveGame.data[levelName]["complete"].AsBool;
    }

}
