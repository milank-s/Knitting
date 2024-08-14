using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
public class LevelLoader : MonoBehaviour
{

    public void LoadLevel(string levelName){
        
        //check whether we're going to a completed level before warping


        SaveGame.Save();

        SceneController.instance.LoadScene(levelName);
    }
}
