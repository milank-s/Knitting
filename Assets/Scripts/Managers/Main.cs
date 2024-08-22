	using System.Collections;
	using System.Collections.Generic;
	using AudioHelm;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using UnityEngine.SceneManagement;
	using UnityEngine.InputSystem;


	public enum GameState {playing, paused, menu}

	public class Main : MonoBehaviour {

	[Header("Editor info")]
	[SerializeField] public string loadFileName;


	public bool devMode = true;
	public GameState state;

	[Header("Player References")]
	public GameObject cursor;
	public GameObject Player;
	public PlayerInput playerInput;

	[Header("Variables")]
	public bool useVibration;
	public bool useGamepad;

	[Header("Singletons")]
	public MapEditor editor;
	public Camera mainCam;
	public MenuController menuController;
	public PrefabManager prefabs;
	public FXManager fx;
	
	[Header("UI")]
	public GameObject editorUI;
	public GameObject PauseMenu;
	public GameObject pauseResumeButton;
	public GameObject canvas;
	public bool hasGamepad => Gamepad.current != null;

	[Header("Level parents")]
	public StellationController activeStellation;
	public Transform pointParent;
	public Transform splineParent;
	public Transform stellationParent;

	public delegate void StellationLoad(StellationController c);
	public delegate void PointAction(Point p);
	public delegate void SplineAction(Spline s);
	public delegate void GenericAction();

	public StellationLoad OnLoadLevel;
	
	public GenericAction OnLeaveFirstPoint;

	public GenericAction OnReset;
	public PointAction OnPointEnter;
	public PointAction OnPointExit;
	public PointAction OnPlayerEnterPoint;
	public PointAction OnPlayerExitPoint;
	public SplineAction OnSplineEnter;
	public SplineAction OnSplineExit;
	private bool pressedPause;
	
	public static float cameraDistance = 3;

	public Gamepad gamepad
	{
		get
		{
			return Gamepad.current;
		}
	}
	

	public void OpenEditorFileOnLoad(string l){
		loadFileName = l;
	}

	public void PauseGame()
	{
		if(state == GameState.playing)
			{
				Pause(true);
			}
	}
	public void CancelInput()
	{
		if (state == GameState.menu)
		{
			if (Services.menu.settingsOpen)
			{
				Services.menu.OpenSettings();
			}
			else
			{

				//ask to quit the game?
			}
		}
		else
		{
			if (!MapEditor.typing)
			{
				Pause(!PauseMenu.activeSelf);	
			}
			else
			{

				MapEditor.typing = false;
			}
		}
	}

	public void OpenMenu(){
	
		if(OnReset != null){
			OnReset.Invoke();
		}

		state = GameState.menu;
		playerInput.SwitchCurrentActionMap("UI");
		
		if (MapEditor.editing)
		{
			ToggleEditMode();
		}

		Services.menu.Show(true);
	}

	public void Quit()
	{
		Application.Quit();
	}
	
	public void ResetLevel()
	{	
		if(state != GameState.playing || MapEditor.editing) return;



		state = GameState.paused;

		if(OnReset != null){
			OnReset.Invoke();
		}
	
		GlitchEffect.Fizzle(0.2f);

		//this is currently game breaking?
		//I would prefer not to do this.... but the OnLoadLevel func is worth calling	
		
		if(StellationManager.instance != null){
			//reset scene
			//just reload the scene I guess
			StellationManager.instance.ResetToCheckpoint();
			
		}else{
			
			string activeLevel = activeStellation.title;

			//be mindful of how stellations track and reset collectibles
			//if stellations can be changed or modified then you need to read from file

			SceneController.instance.UnloadStellation(activeStellation);
			SceneController.instance.LoadDirect(activeLevel);
		}	
	}


	public void FullReset(){
		if(OnReset != null){
			OnReset.Invoke();
		}
		
		 foreach (StellationController s in stellationParent.GetComponentsInChildren<StellationController>())
        {
            Destroy(s.gameObject);
        }
		
        Services.main.editor.controller = null;
        Services.main.pointParent = null;
        Services.main.splineParent = null;

        SceneController.instance.stellationsLoaded.Clear();
        SceneController.curLevel = 0;   
		
		Spline.frequency = 5.324f;
		Spline.shake = 0;
		Spline.noiseSpeed = 5;
		Spline.amplitude = 0.5f;

		editor.DeselectAll();
		
		Point.Points.Clear();
		Spline.Splines.Clear();
	}
	
	
	public void QuitLevel()
	{
		state = GameState.menu;
		
		//more general method of unloading scenes
		if(SceneManager.sceneCount > 1){
			SceneManager.UnloadSceneAsync(SceneController.curLevelName);
		}

		SceneController.curLevelName = "";
		
		Pause(false);
		FullReset();
		OpenMenu();
	}
	
	public void Awake ()
	{

		Point.Points = new List<Point>();
		Spline.Splines = new List<Spline>();
		Services.GameUI = canvas;
		Services.mainCam = mainCam;
		Services.Prefabs = prefabs;
		Services.Player = Player;
		Services.menu = menuController;
		Services.fx = fx;
		CameraFollow.instance = mainCam.GetComponent<CameraFollow>();
		Services.PlayerBehaviour = Player.GetComponent<PlayerBehaviour>();
		Services.Cursor = cursor;
		PointManager._pointsHit = new List<Point> ();
		PointManager._connectedPoints = new List<Point> ();
		Services.Sounds = GetComponent<SoundBank> ();
		Services.main = this;
		PauseMenu.SetActive(false);
		Services.Prefabs.InitCrawlers();
	
	}

	void Start()
	{
		
		GameSettings.i.InitializeSettings();

		Cursor.lockState = CursorLockMode.None;
		
		state = GameState.menu;
		// MapEditor.editing = true;
		// ToggleEditMode();
		SceneController.instance.OnStart();
		
	}

	public void Pause(bool pause)
	{
		PauseMenu.SetActive(pause);
		AudioManager.instance.Pause(pause);
		
		if (pause)
		{
			EventSystem.current.SetSelectedGameObject(pauseResumeButton);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			state = GameState.paused;
		}
		else
		{
			if (!MapEditor.editing)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}

			state = GameState.playing;
		}

		pressedPause = true;
		
		Time.timeScale = pause ? 0 : 1;
	}
	
	void Update()
	{

		CameraFollow.instance.uiCam.fieldOfView = CameraFollow.instance.cam.fieldOfView;
	
		// if we started with a scene open, allow me to go in and edit it

		if(Input.GetKeyDown(KeyCode.Space) && SceneController.instance.curSetIndex == -1)
		{
			if(state == GameState.playing){
				if (!MapEditor.typing)
				{
					ToggleEditMode();
				}
			}
		}

		if (state == GameState.playing)
		{
			if (!MapEditor.editing)
			{

				if(activeStellation != null){
					activeStellation.Step();
				}

				Services.fx.Step();
				
				//I would like to pause this between levels
				if (Services.PlayerBehaviour.curPoint != null)
				{
					if(activeStellation != null && !activeStellation.won){
						if(Services.PlayerBehaviour.state != PlayerState.Animating){
							Services.PlayerBehaviour.Step();
						}	
						CameraFollow.instance.FollowPlayer();
					}
				}
			}
			else
			{
				editor.Step();
			}

			foreach (Spline s in Spline.Splines)
			{
				if(MapEditor.editing || s.state == Spline.SplineState.on) {
					
					s.UpdateSpline();

					//things seem to be going pretty well without having to call this
					//what does it to? update normals? update when you change texture offset?
				
					s.line.Draw3D();
				}
			}

			foreach(CrawlerManager c in Services.Prefabs.crawlerPools){
				c.Step();
			}
		}

		if (pressedPause)
		{
			pressedPause = false;
			if (state == GameState.paused)
			{
				playerInput.SwitchCurrentActionMap("UI");
			}
			else
			{
				if (state == GameState.menu || MapEditor.editing)
				{
					playerInput.SwitchCurrentActionMap("UI");
				}
				else if (state == GameState.playing)
				{
					playerInput.SwitchCurrentActionMap("Player");
				}
			}
		}

	}

	//this is fucking terrible
	public void WarpPlayerToNewPoint(Point p)
	{
		//Services.StartPoint = p;
		float curSpeed = Services.PlayerBehaviour.curSpeed;
		float flow = Services.PlayerBehaviour.flow;
		
		Services.PlayerBehaviour.curPoint.OnPlayerExitPoint();

		if (Services.PlayerBehaviour.curSpline != null)
		{
			Services.PlayerBehaviour.curSpline.OnSplineExit();
		}

		//this seems incredibly illegal
		Services.PlayerBehaviour.curPoint = p;
		Services.PlayerBehaviour.transform.position = p.Pos;
		
		Services.PlayerBehaviour.curSpline = null;
		Services.PlayerBehaviour.curPoint.OnPlayerEnterPoint();
		
		Services.PlayerBehaviour.flow = flow;
		Services.PlayerBehaviour.curSpeed = curSpeed;
		
		Services.PlayerBehaviour.ResetFX();

	}


	//this happens when levels are entered from the menu
	public void EnterLevelSet()
	{
		Services.fx.Fade(true , 1f);
		SceneController.curLevel = 0;
		
		// EnterPlayMode();

		Services.menu.Show(false);
		SceneController.instance.LoadDirect(SceneController.instance.GetCurLevel());
	}


	//This happens every time a level is loaded and reloaded
	//I want it to function separately from the animations we play between levels
	//so that when we enter levels or change levels or reset levels we can do diff things

	public void InitializeLevel(){
		
		activeStellation.Initialize();
		
		//static variables?
		Spline.shake = 0;

		if (Services.StartPoint == null && Point.Points.Count > 0)
		{
			Services.StartPoint = Point.Points[0];
		}
		
		//once again, this is the wrong place for this shit
		//is this for sound? fx? crawlers? player?
		//

		OnReset.Invoke();
	
		if(OnLoadLevel != null){
			OnLoadLevel(activeStellation);
		}
	}

	//this happens when levels are loaded and reset
	//but there is a lot of redundancy that suggests it only needs to come from the main menu and level editor
	//it is the only function that calls player.initialize
	
	public void ActivatePlayer(){
        // StartCoroutine(ShowTitle());

		playerInput.SwitchCurrentActionMap("Player");
		Cursor.lockState = CursorLockMode.Locked;
		state = GameState.playing;
		
		activeStellation.OnPlayerEnter();

		Services.Player.SetActive(true);
		Services.PlayerBehaviour.Initialize();
    }

	public IEnumerator ShowTitle(){

		Services.fx.title.text = activeStellation.title;
		// Services.fx.overlay.color = Color.black;

		yield return new WaitForSeconds(0.33f);

		// Services.fx.overlay.color = Color.clear;
		Services.fx.title.text = "";
	}

	public void EnterUIMode()
	{
		playerInput.SwitchCurrentActionMap("UI");
		Cursor.lockState = CursorLockMode.None;
	}
	
	public void ToggleEditMode()
	{
			MapEditor.editing = !MapEditor.editing;
			bool enter = MapEditor.editing;
			editorUI.SetActive(enter);
			canvas.SetActive(!enter);
			Player.SetActive(!enter);
			
			Services.mainCam.GetComponent<CameraFollow>().enabled = !enter;
			RenderSettings.fog = !enter;
			editor.l.enabled = enter;
			
			if (enter)
			{
				//should I use full reset here
				OnReset.Invoke();

				if (state == GameState.menu)
				{
					Services.menu.Show(false);
				}
				
				if (editor.controller == null)
				{
					GameObject newController = new GameObject();
					StellationController c = newController.AddComponent<StellationController>();
					editor.controller = c;
					activeStellation = c;
					newController.transform.parent = stellationParent;
					splineParent = newController.transform;
					pointParent = new GameObject().transform;
					pointParent.transform.parent = splineParent;
					editor.splinesParent = splineParent;
					editor.pointsParent = pointParent;
					newController.name = "Untitled";
					c.title = "Untitled";
					newController.transform.parent = splineParent;
				}
				else
				{
					string levelName = editor.controller.title;
					FullReset();
					activeStellation = editor.Load(levelName);
					
				}

				Vector3 cameraPos = CameraFollow.instance.cam.transform.position;
				cameraPos.z = 0;

				CameraFollow.instance.WarpToPosition(cameraPos);
				
				//SynthController.instance.StopNotes();

				//whyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy
				//just so I can bring up the pause menu?
				state = GameState.playing;
				
				SceneController.instance.curSetIndex = -1;
				editor.EnterEditMode();
			}
			else
			{
				//SynthController.instance.StopNotes();
				
				if (state != GameState.menu)
				{
					editor.Save(editor.controller);
					//do we force save before playing?
					editor.TogglePlayMode();
				}
			}		
	}

}
