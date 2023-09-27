
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SceneController : MonoBehaviour
{
    public Point startPoint;
    public List<StellationController> activeScenes;

    public int curSetIndex;
    public int unlockedIndex;
    public LevelSet curLevelSet
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

        if(PlayerPrefs.HasKey("level")){
            curLevel = PlayerPrefs.GetInt("level");
        }else{
            PlayerPrefs.SetInt("level", 0);
        }
        
        SelectLevelSet();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Period))
        {
            SkipStellation();
        }
        
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && Services.main.state == Main.GameState.menu)
        {
            if (levelButton.gameObject == UISystem.currentSelectedGameObject)
            {
                Vector2 input = context.ReadValue<Vector2>();
                if (input.x > 0 && Mathf.Approximately(input.y, 0))
                {
                    SelectNextLevel(true);
                }
                else if (input.x < 0 && Mathf.Approximately(input.y, 0))
                {

                    SelectNextLevel(false);
                }
            }
            else
            {
                Services.main.TryChangeSetting(context);
            }
        }
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

    public string GetCurLevel(){
        return curLevelSet.levels[curLevel];
    }
    public void SelectNextLevel(bool increment)
    {

        int index = curSetIndex;
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
        
        if(index != curSetIndex){
            GlitchEffect.Fizzle(0.1f);
        }

        SelectLevelSet();
    }
    
    public void SelectLevelSet()
    {
        Services.main.ShowImage(curLevelSet.image);
        Services.main.ShowWord(curLevelSet.title);    
        Services.main.levelReadout.text = curSetIndex + ".";
    }

    public void LoadLevelSet()
    {
        if (Services.main.state == Main.GameState.menu)
        {
      
            curLevel = 0;
            Services.main.FullReset();
            Services.main.CloseMenu();

            
            StartCoroutine(Services.main.LevelIntro(curLevelSet));
            
        }
    }
    
    public void SkipStellation(){

         curLevel++;
        
        if (curSetIndex == -1 && Services.main.curLevel == "")
        {
            //we're in the editor, dont do anything
            return;

        }

        //stopgap stuff for when I want to test the level without going through the menu;
        if(curSetIndex != -1 && curLevel < curLevelSet.levels.Count){    
            
            Services.main.LoadNextLevel(curLevelSet.isScene, false);
            
        }
        else
        {
                //reopen menu, empty scene;
                
           Services.main.QuitLevel();
            
        }
    }
    public void LoadNextStellation()
    {

        curLevel++;
        
        if (curSetIndex == -1 && Services.main.curLevel == "")
        {
            //we're in the editor, dont do anything
            return;

        }

        //stopgap stuff for when I want to test the level without going through the menu;
        if(curSetIndex != -1 && curLevel < curLevelSet.levels.Count){    
            
            Services.main.LoadNextLevel(curLevelSet.isScene);
            
        }
        else
        {
                //reopen menu, empty scene;
                
           Services.main.FinishLevelSet();
            
        }
    }

    public void UnloadStellation(StellationController s)
    {
        activeScenes.Remove(s);
        Destroy(s.gameObject);
    }
    
    //Function for transition between levels. With text and image???
    
    // Update is called once per frame

}
