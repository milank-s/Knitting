using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBank : MonoBehaviour
{
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LoadLevel(string levelName)
    {
        Services.main.LoadLevelDelayed(levelName);
    }
}
