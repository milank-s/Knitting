using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class SceneSettings : MonoBehaviour
{
    // Start is called before the first frame update

    public Point startPoint;
    public List<string> levels;
    private int curLevel;
    void Start()
    {
        
        instance = this;
        curLevel = 0;
        
        if (startPoint != null)
        {
            Services.StartPoint = startPoint;
            Services.main.InitializeLevel();
        }else if (levels.Count > 0)
        {
            LoadNextLevel();
        }

    }

    public static SceneSettings instance;
    public void LoadNextLevel()
    {
        if(curLevel < levels.Count){
            MapEditor.Load(levels[curLevel]);
            
            Services.main.InitializeLevel();            
            //Services.PlayerBehaviour.Reset();
            curLevel++;
        }
    }
    // Update is called once per frame

}
