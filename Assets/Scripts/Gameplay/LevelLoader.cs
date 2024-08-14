using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public void LoadLevel(string levelName){
        
        SceneController.instance.LoadScene(levelName);
    }
}
