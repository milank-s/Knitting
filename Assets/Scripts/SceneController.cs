
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update

    public Point startPoint;
    public List<StellationController> activeScenes;

    public int curSetIndex;
    public int unlockedIndex;
    
    private LevelSet curLevelSet
    {
        get { return levelSets[curSetIndex]; }
    }

    public EventSystem UISystem;
    public Button levelButton;
    
    public List<LevelSet> levelSets;
    
    public int curLevel;
    void Awake()
    {
        
        //read from json file to set unlocked index; 
        unlockedIndex = levelSets.Count;
        activeScenes = new List<StellationController>();
        instance = this;
        curLevel = 0;
        SelectLevelSet();
   
       
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Period))
        {
            LoadNextStellation(0);
        }
        
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {

        if (context.phase == InputActionPhase.Started && levelButton.gameObject == UISystem.currentSelectedGameObject && Services.main.state == Main.GameState.menu)
        {
            Vector2 input = context.ReadValue<Vector2>();
            if (input.x > 0 && Mathf.Approximately(input.y, 0))
            {
                SelectNextLevel(true);
            }
            else if(input.x < 0 && Mathf.Approximately(input.y, 0))
            {

                SelectNextLevel(true);
            }
        }
    }



    public void Reset()
    {
        foreach (StellationController s in Services.main.splineParent.GetComponentsInChildren<StellationController>())
        {
            Destroy(s.gameObject);
        }

        instance.activeScenes.Clear();
        instance.curLevel = 0;   
    }
    public void OpenEditor()
    {

        Services.main.Word.text = "";
        Services.main.ShowImage(null, false);
        if (Services.main.state == Main.GameState.menu)
        {
            Services.main.CloseMenu();
        }

        if (Services.main.state == Main.GameState.paused)
        {
            Services.main.Pause(false);
        }
        
        if (!MapEditor.editing)
        {
            Services.main.ToggleEditMode();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Services.main.state = Main.GameState.playing;
    }
    
    public static SceneController instance;

    public void SelectNextLevel(bool increment)
    {
        if (increment)
        {
            curSetIndex++;
            curSetIndex = Mathf.Clamp(curSetIndex, 0, unlockedIndex);
            if (curSetIndex >= levelSets.Count)
            {
                curSetIndex = 0;
            }
         
        }
        else
        {
            curSetIndex--;
            if (curSetIndex < 0)
            {
                curSetIndex = Mathf.Clamp(levelSets.Count - 1, 0, unlockedIndex);
            }
        }
        
        SelectLevelSet();
    }
    public void SelectLevelSet()
    {
        Services.main.ShowImage(curLevelSet.image);
        Services.main.ShowWord(curLevelSet.title);    
    }

    public void LoadLevelSet()
    {
        
        
        if (Services.main.state == Main.GameState.menu)
        {
      
        curLevel = 0;
        Services.main.Reset();
        Services.main.CloseMenu();


        //play level intro. 
        StartCoroutine(Services.main.LevelIntro(curLevelSet));
        
        }
}
    
    public void LoadNextStellation(float delay = 0)
    {

        if (curSetIndex == -1)
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
            //Services.main.LoadLevelDelayed("", 2);
            
            //reopen menu, empty scene;
            Services.main.QuitLevel(true);
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
