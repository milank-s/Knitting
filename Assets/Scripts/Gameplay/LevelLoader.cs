using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public void Start(){
        //do something if we're complete
        //to make it obvious on the map
    }

    public void LoadLevel(string levelName){
        
        //check whether we're going to a completed level before warping


        SaveGame.Save();

        SceneController.instance.LoadScene(levelName);
    }
}
