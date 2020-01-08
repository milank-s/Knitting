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

    private LevelSet curLevelSet;
    public List<LevelSet> levelSets;
    
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
    
    public void OpenEditor()
    {
       
        Services.main.CloseMenu();

        if (!MapEditor.editing)
        {
            Services.main.ToggleEditMode();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    public static SceneController instance;

    public void LoadLevelSet(int i)
    {
        
        Services.main.Reset();
        Services.main.CloseMenu();
        
        if (MapEditor.editing)
        {
            Services.main.ToggleEditMode();
        }
        
        curLevelSet = levelSets[i];
        //play level intro. 
        StartCoroutine(Services.main.LevelIntro(curLevelSet));
        
    }
    
    public void LoadNextStellation(float delay = 0)
    {
        if (curLevelSet == null)
        {
            return;
        }
        if(curLevel < curLevelSet.levels.Count){
            
            //MapEditor.Load(levels[curLevel]);

            
            Services.main.LoadFile(curLevelSet.levels[curLevel], delay);
            

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
            curLevelSet = null;
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
