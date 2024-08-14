using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public void LoadLevel(string levelName){
        
        //save the current point position 
        
        //you should do this based on scene name
        SaveGame.Save();

        SceneController.instance.LoadScene(levelName);
    }
}
