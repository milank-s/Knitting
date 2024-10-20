
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
    public List<StellationController> stellationsLoaded;

    public int curSetIndex;
    public int unlockedIndex;
    public LevelSet curLevelSet
    {
        get { return levelSets[curSetIndex]; }
    }
    
    public List<LevelSet> levelSets;
    public static int curLevel;
    public static string curLevelName;
    void Awake()
    {
        //read from json file to set unlocked index; 
        curLevelName = "";
        unlockedIndex = levelSets.Count;
        stellationsLoaded = new List<StellationController>();
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
            /////uhhh shouldnt openeditor do this anyway
			// Services.menu.Show(false);
            Services.main.ToggleEditMode();
            
		}
    }
    
    //go back to menu after level set
	public void FinishLevelSet(){
		
		StartCoroutine(CompleteLevelSet());
	}

	IEnumerator CompleteLevelSet(){
		
        //typical level ending sequence
		yield return StartCoroutine(LevelCompleteRoutine());

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
                
			    Services.menu.StartSequence();

			}else{
                //this is for something to do with leveleditor I think

				//curSetIndex = -1;
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
        
        if (Services.main.state == GameState.menu)
        {
            Services.menu.Show(false);
        }

        if (Services.main.state == GameState.paused)
        {
            Services.main.Pause(false);
        }
        
        if (!MapEditor.editing)
        {
            Services.main.ToggleEditMode();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Services.main.state = GameState.playing;
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
            GlitchEffect.Fizzle(0.25f);
        }

        Services.menu.SelectLevelSet(curLevelSet, increment, true);
    }

    public void FinishStellation()
    {
        //why do we check if the curLevelName is null my friend
        if (curSetIndex == -1 && curLevelName == "")
        {
            //we're in the editor, pop player out            
            Services.main.ToggleEditMode();
        }else{    
            Services.main.activeStellation.isComplete = true;
            
            if(Services.main.activeStellation != null){
                Services.main.activeStellation.Cleanup();
            }

            //in scenes we want to return to the level tree
            //this means loading the 0th level in the set?
            //assuming thats how we access the root level
            //do we want recursive levels? do we want to travel up and down levels?

            if(StellationManager.instance != null){

                //mark complete and return to world map
                //return to world map
                
                // SaveGame.Save();
                // LoadScene("World");
                
                FinishLevelSet();

            }else{
                curLevel++;
                
                if(curLevel < curLevelSet.levels.Count){
                    curLevelName = curLevelSet.levels[curLevel];
                    LoadWithTransition();
                }else{
                    FinishLevelSet();
                }
            }
        }
    }

    //when loading a stellation file
	public void LoadFile(string fileName)
	{
        Vector3 offset = Vector3.zero;

        bool newStellation = true;

        if(Services.main.activeStellation != null){
            
            newStellation = fileName != Services.main.activeStellation.title;

            //the camera has been following the player
            
            if(!newStellation || Services.main.activeStellation.lockX && Services.main.activeStellation.lockY){
                offset = Services.main.activeStellation.center;
            }else{
                offset = Services.PlayerBehaviour.transform.position;
            }

            if(newStellation){
                offset.z -= Services.main.activeStellation.depth/2f;
            }
            
        }else{
            Debug.Log("no active stellation");
        }

        //we need to make sure that bounds and center are set before we offset

        //this is fucking up when you die in the level editor because it uses the streaming assets directory
        
		Services.main.activeStellation = MapEditor.instance.Load(fileName);
        
        Services.main.activeStellation.OffsetPosition(offset, newStellation);
        

        //I think a better way to check this is whether curlevelIndex is -1?
        // if (!MapEditor.editing)
		if (curSetIndex != -1)
		{
			SceneController.instance.stellationsLoaded.Add(Services.main.activeStellation);
		}

        if(SceneController.instance.stellationsLoaded.Count > 1){

            StellationController c  =SceneController.instance.stellationsLoaded[0];
            SceneController.instance.stellationsLoaded.Remove(c);
            Destroy(c.gameObject);
        }
        
		Services.main.InitializeLevel();
        Services.main.ActivatePlayer();
	}

    void UnloadScene(){
        if(SceneManager.sceneCount > 1){
			if (curLevelName != "")
			{
                // Debug.Log("unloading " + curLevelName);
				SceneManager.UnloadSceneAsync(curLevelName);
			}
		}
		
		int s = curLevel;
		Services.PlayerBehaviour.Reset();
		Services.main.FullReset();
		
		curLevel = s;
		curLevelName = GetCurLevel();
    }

    public void LoadScene(string sceneName){
        
		UnloadScene();
        curLevelName = sceneName;

        if (sceneName != "")
		{
			SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
		}
    }

    public void LoadDirect(){
        LoadDirect(GetCurLevel());
    }

    //loads and starts player instantly
    public void LoadDirect(string levelTitle){
        //editor override
        if(curSetIndex == -1) {
            
            LoadFile(levelTitle); return;
        }

        if(curLevelSet.isScene){
            LoadScene(GetCurLevel());
        }else{
            
            LoadFile(levelTitle);
        }

    }

    //dollies camera to the new stellation
    public void LoadWithTransition(string levelName){
        
		StartCoroutine(LoadLevelRoutine(levelName));
    }

    
    public void LoadWithTransition(){
        
        Debug.Log("load with transition");

		StartCoroutine(LoadLevelRoutine(GetCurLevel()));
    }

    public IEnumerator LoadLevelRoutine(string levelName){
        
        Services.main.state = GameState.paused;
        
        LoadDirect(levelName);

        //You have a problem right now where ActivatePlayer is called from InitializeLevel
        //so they could be fucking around while this is happening
        //because the game is in play mode
        
        //there is aperiod of time between init and transition where
        //the player is still on the last stellation and is causing problems
        //what is our method for stopping the player from inputting anything?

        //play the animation for the camera and title
		yield return StartCoroutine(LevelTransitionRoutine());
	}

    //transition between levels in the flow of play
	public IEnumerator LevelTransitionRoutine(){

        Services.main.activeStellation.SetCameraInfo();

        yield return StartCoroutine(CameraFollow.instance.MoveRoutine());
	}

    public IEnumerator LevelCompleteRoutine(){

        Services.main.activeStellation.SetCameraInfo();
		yield return StartCoroutine(CameraFollow.instance.MoveRoutine());
	}



    public void UnloadStellation(StellationController s)
    {
        stellationsLoaded.Remove(s);
        Destroy(s.gameObject);
    }
    
    //Function for transition between levels. With text and image???
    
    // Update is called once per frame

}
