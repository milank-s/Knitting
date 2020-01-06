using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update

    public Point startPoint;
    public List<StellationController> activeScenes;
    public List<string> levels;
    
    public int curLevel;
    void Awake()
    {
        activeScenes = new List<StellationController>();
        instance = this;
        curLevel = 0;
      
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            LoadNextStellation(0);
        }
    }
    public static SceneController instance;

    public void LoadNextStellation(float delay = 0)
    {
        if(curLevel < levels.Count){
            
            //MapEditor.Load(levels[curLevel]);

            
            Services.main.LoadFile(levels[curLevel], delay);
            

            if (activeScenes.Count > 0)
            {
                
                UnloadScene(activeScenes[0]);
            }

            //Services.main.InitializeLevel();            
            //Services.PlayerBehaviour.Reset();
            curLevel++;
        }
        else
        {
            Services.main.LoadLevelDelayed("", 2);
        }
    }

    public void UnloadScene(StellationController s)
    {
        activeScenes.Remove(s);
        Destroy(s.gameObject);
    }
    
    
    //Function for transition between levels. With text and image???
    
    // Update is called once per frame

}
