	using System.Collections;
	using System.Collections.Generic;
	using AudioHelm;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using UnityEngine.SceneManagement;
	using UnityEngine.InputSystem;

	public class Main : MonoBehaviour {

	public enum GameState {playing, paused, editing, menu}

	public GameState state;

	[Header("Components")]
	public GameObject cursor;
	public GameObject Player;
	public Image PauseScreen;
	public GameObject PauseMenu;
	public GameObject pauseResumeButton;
	public Text Word;
	public Text levelReadout;
	public Text description;
	public Image image;
	public FXManager fx;
	public GameObject canvas;
	public PrefabManager prefabs;
	public bool hasGamepad => Gamepad.current != null;
	public Transform pointParent;
	public Transform splineParent;
	public Transform stellationParent;
	public MapEditor editor;
	public GameObject editorUI;
	public Camera mainCam;
	public MenuController menuController;
	public CrawlerManager crawlerManager;
	public bool useVibration;
	public bool useGamepad;
	public StellationController activeStellation;
	public PlayerInput playerInput;
	public Text text;
	public Text levelText;


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


	public bool openFileOnStart = false;
	private bool pressedPause;
	
	[SerializeField]
	private float fadeLength = 0.1f;
	public Gamepad gamepad
	{
		get
		{
			return Gamepad.current;
		}
	}
	
	
	[SerializeField] public string loadFileName;


	public void OpenEditorFileOnLoad(string l){
		loadFileName = l;
		openFileOnStart = true;
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

		state = Main.GameState.menu;
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
			// StartCoroutine(LoadSceneRoutine());
			
		}else{
			//this doesnt work for the editor
			if(SceneController.instance.curSetIndex != -1){
				SceneController.instance.LoadFile();
			}else{
				ToggleEditMode();
			}
		}	

		//maybe I just use loadfile
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
        
        SceneController.instance.activeScenes.Clear();
        SceneController.curLevel = 0;   
		
		Spline.frequency = 5.324f;
		Spline.shake = 0;
		Spline.noiseSpeed = 5;
		Spline.amplitude = 0.5f;

		editor.DeselectPoints();
		editor.DeselectSpline();
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
		Services.Word = Word;
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
		PauseScreen.color = new Color(0,0,0,0);
		PauseMenu.SetActive(false);
	
	}

	void Start()
	{
		
		GameSettings.i.InitializeSettings();

		Cursor.lockState = CursorLockMode.None;
		
		state = GameState.menu;
		MapEditor.editing = true;
		ToggleEditMode();
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
	
//		if (Input.GetKeyDown (KeyCode.R)) {
//			SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
//		}
		
		
		if(Input.GetKeyDown(KeyCode.Space) && SceneController.instance.curSetIndex == -1)
		{
			if(state == GameState.playing){
				if (!MapEditor.typing)
				{
					ToggleEditMode();
				}
			}else{

				// SceneController.instance.LoadLevelSet();

				//WHY IS THIS HERE
			}
		}

		if (state == GameState.playing)
		{
			if (!MapEditor.editing)
			{

				if(activeStellation != null){
					activeStellation.Step();
				}

				if (Services.PlayerBehaviour.curPoint != null)// && !activeStellation.won)
				{
					Services.PlayerBehaviour.Step();
					CameraFollow.instance.FollowPlayer();
				}
					
				if(Services.PlayerBehaviour.curSpline != null){

					//Services.PlayerBehaviour.curSpline.UpdatePoints();

					// if(!Services.PlayerBehaviour.curSpline.drawingIn){
					// }
				}
				
				foreach (Spline s in Spline.Splines)
				{
					if(s.state == Spline.SplineState.on) { //!s.drawingIn){
						
						s.UpdateSpline();
						s.line.Draw3D();
					}
					
				}
			}
			else
			{
				editor.Step();
				
				foreach (Spline s in Spline.Splines)
				{
					s.UpdateSpline();
					s.line.Draw3D();
				}
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
		
		Services.PlayerBehaviour.curPoint.OnPointExit();

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

		//they are already switching in the cases when this is called
		// Services.PlayerBehaviour.SwitchState(PlayerState.Switching);
	}

	public void InitializeLevel(){
		//this is doubling up the initialization done by the manager?
		
		//the stellation initializes its points on start...
		//we may be forgiven for only initializing splines?
		if (Spline.Splines.Count > 0){
			for (int i = Spline.Splines.Count - 1; i >= 0; i--)
			{

				if (Spline.Splines[i] == null)
				{
					Spline.Splines.RemoveAt(i);
				}
				else
				{
					Spline.Splines[i].Initialize();
				}
			}
		}

		Services.main.text.text = " ";
		Services.main.levelText.text = " ";

		//this needs to work for the editor to work
		//but I dont like it
		activeStellation.Setup();
		activeStellation.OnPlayerEnter();

		if (Services.StartPoint == null && Point.Points.Count > 0)
		{
			Services.StartPoint = Point.Points[0];
		}


		OnReset.Invoke();

		if (!MapEditor.editing)
		{
			playerInput.SwitchCurrentActionMap("Player");
			Services.Player.SetActive(true);
			Services.PlayerBehaviour.Initialize();
		}

		EnterPlayMode();
		
		if(OnLoadLevel != null){
			OnLoadLevel(activeStellation);
		}
	}

	public void EnterPlayMode()
	{
		Cursor.lockState = CursorLockMode.Locked;
		state = GameState.playing;
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
					newController.transform.parent = splineParent;
				}
				else
				{
					string levelName = editor.controller.name;
					FullReset();
					activeStellation = editor.Load(levelName);
					
				}
				
				
//				foreach (Point p in Point.Points)	
//				{
//					p.Reset();
//				}
//
//				foreach (Spline s in Spline.Splines)
//				{
//					s.ResetVectorLine();
//				}

				Vector3 cameraPos = CameraFollow.instance.cam.transform.position;
				cameraPos.z = 0;
				CameraFollow.instance.WarpToPosition(cameraPos);
				
				//SynthController.instance.StopNotes();
				
				EnterUIMode();
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
	
	public IEnumerator FadeIn(){
		
		float t = 0;
		yield return new WaitForSecondsRealtime(0.33f);
		
		while (t < 1){
			//PauseScreen.color = Color.Lerp(Color.black, Color.clear, Easing.QuadEaseIn(t/fadeLength));
			GlitchEffect.SetValues(1-t);
			t += Time.unscaledDeltaTime/fadeLength;
			yield return null;
		}

		GlitchEffect.SetValues(0);
		//PauseScreen.color = Color.clear;
	}

	public IEnumerator FlashImage(bool fadeIn = false)
	{
		float t = 0;
		while (t < 1)
		{
			if (!fadeIn)
			{
				image.color = Color.Lerp(Color.white, Color.clear, t);
			}
			else
			{
				image.color = Color.Lerp(Color.white, Color.clear, 1-t);
			}
			t += Time.deltaTime/2;
			yield return null;
		} 
	}

	public IEnumerator FlashWord(bool fadeIn = false)
	{
		
		float t = 0;
		while (t < 1)
		{
			if (!fadeIn)
			{
				Word.color = Color.Lerp(Color.white, Color.clear, t);
			}
			else
			{
				Word.color = Color.Lerp(Color.white, Color.clear, 1-t);
			}

			t += Time.deltaTime * 2;
			yield return null;
		} 
	}
	
	public IEnumerator EnterLevelSet(LevelSet l)
	{
		//this is where we zoom into the oscilloscope?
		yield return null;

		SceneController.curLevel = 0;
		state = GameState.playing;
		playerInput.SwitchCurrentActionMap("Player");

		SceneController.instance.LoadLevel();
	}
	
	
	public IEnumerator FadeOut(){
		
		float t = 0;
		while (t < 1)
		{
			// PauseScreen.color = Color.Lerp(Color.clear, Color.black, Easing.QuadEaseIn(t/fadeLength));
			GlitchEffect.SetValues(t);
			t += Time.unscaledDeltaTime/fadeLength;
			yield return null;
		}

		GlitchEffect.SetValues(1);
		//PauseScreen.color = Color.black;
	}

}
