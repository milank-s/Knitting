
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Testing in Editor")]
    public bool openFileOnStart;
    public string loadFileName;
    
    
    [Header("Level progression data")]
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
    public static int curLevel;
    public static string curLevelName;
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
        
    }

    void Update()
    {

        if(openFileOnStart){
			OpenEditor();
			MapEditor.instance.LoadInEditor(loadFileName);
			openFileOnStart = false;
			Services.menu.Show(false);
		}

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
                Services.menu.TryChangeSetting(context);
            }
        }
    }
    
    //go back to menu after level set
	public void FinishLevelSet(){
		
		StartCoroutine(CompleteLevelSet());
	}

	IEnumerator CompleteLevelSet(){
		
        //typical level ending sequence
		yield return StartCoroutine(FinishLevel());

        //queue up next level on menu
		SceneController.instance.SelectNextLevel(true);

        //return to menu
		Services.main.QuitLevel();

	}

    public void OnStart(){
        	//get any open scene in order to play it
		if (SceneManager.sceneCount > 1)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				if (SceneManager.GetSceneAt(i).name != "Main")
				{
					curLevelName = SceneManager.GetSceneAt(i).name;
				}
			}
		}
		
		if(!openFileOnStart){
			if(curLevelName == ""){
				Services.main.OpenMenu();
			}else{
				curSetIndex = -1;
				Services.menu.Show(false);
			}
		}else{
			if(SceneManager.sceneCount > 1){
				SceneManager.UnloadSceneAsync(curLevelName);
				curLevelName = "";
			}

			Services.main.OpenMenu();
		}

		Time.timeScale = 1;
    }

    public void OpenEditor()
    {
        
        if (Services.main.state == Main.GameState.menu)
        {
            Services.menu.Show(false);
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

        Services.menu.SelectLevelSet(curLevelSet);
    }


    public void SkipStellation(){

         curLevel++;
        
        if (curSetIndex == -1 &&  curLevelName == "")
        {
            //we're in the editor, dont do anything
            return;

        }

        //stopgap stuff for when I want to test the level without going through the menu;
        if(curSetIndex != -1 && curLevel < curLevelSet.levels.Count){    
            
            LoadLevel(false);
            
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
        
        if (curSetIndex == -1 && curLevelName == "")
        {
            //we're in the editor, dont do anything
            return;

        }

        //stopgap stuff for when I want to test the level without going through the menu;
        if(curSetIndex != -1 && curLevel < curLevelSet.levels.Count){    
            
            LoadLevel();
            
        }
        else
        {
            //reopen menu, empty scene;
           FinishLevelSet();
            
        }
    }

    //when loading a stellation file
	public void LoadFile()
	{

		curLevelName = GetCurLevel();

		if (SceneController.instance.activeScenes.Count > 0)
		{
			SceneController.instance.UnloadStellation(SceneController.instance.activeScenes[0]);
		}
		
		StellationController c = MapEditor.instance.Load(curLevelName);
		Services.main.activeStellation = c;
		
		if (!MapEditor.editing)
		{
			SceneController.instance.activeScenes.Add(c);
		}

		Services.main.InitializeLevel();
	}

    void LoadScene(){
		
		if(SceneManager.sceneCount > 1){
			if (curLevelName != "")
			{
				SceneManager.UnloadSceneAsync(curLevel);
			}
		}
		
		int s = curLevel;
		//this could be bugged
		Services.PlayerBehaviour.Reset();
		Services.main.FullReset();
		
		curLevel = s;
		curLevelName = GetCurLevel();

		if (curLevelName != "")
		{
			SceneManager.LoadScene(curLevel, LoadSceneMode.Additive);
		}
	}

    public IEnumerator LoadLevelRoutine(bool isScene){

		yield return StartCoroutine(FinishLevel());
		LoadLevel(false);
	}

    //transition between levels in the flow of play
	public IEnumerator FinishLevel(){

		Services.main.state = Main.GameState.paused;
		yield return null;
		float t = 0;
	}

    

    public void LoadLevel(bool delay = true){

		if(delay){
			StartCoroutine(LoadLevelRoutine(curLevelSet.isScene));
		}else{
			if(curLevelSet.isScene){
				LoadScene();
			}else{
				LoadFile();
			}
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
