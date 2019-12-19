using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEditor;
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
            LoadNextLevel(0);
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftCurlyBracket))
        {
            
        }
        
        if (Input.GetKeyDown(KeyCode.Period))
        {
            LoadNextLevel(0);
        }
    }
    public static SceneSettings instance;
    public void LoadNextLevel(float delay = 0)
    {
        if(curLevel < levels.Count){
            
            //MapEditor.Load(levels[curLevel]);


            Services.main.LoadFile(levels[curLevel], delay);
            

            //Services.main.InitializeLevel();            
            //Services.PlayerBehaviour.Reset();
            curLevel++;
        }
        else
        {
            Services.main.LoadLevelDelayed("", 2);
        }
    }
    // Update is called once per frame

}
