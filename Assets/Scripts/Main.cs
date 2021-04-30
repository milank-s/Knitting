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
	private string curLevel;
	public MapEditor editor;
	public GameObject editorUI;
	public Camera mainCam;
	public GameObject menu;
	public GameObject settings;
	private bool settingsOpen;
	public GameObject volumeSettings;
	public GameObject settingsButton;
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


	public void OpenSettings()
	{
		settingsOpen = !settingsOpen;
		
		settings.SetActive(settingsOpen);
		
		if (settingsOpen)
		{
			EventSystem.current.SetSelectedGameObject(volumeSettings);
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(settingsButton);
		}
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
			if (settingsOpen)
			{
				OpenSettings();
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

	public void Quit()
	{
		Application.Quit();
	}
	
	public void ResetLevel()
	{	
		if(OnReset != null){
			OnReset.Invoke();
		}

		GlitchEffect.Fizzle(0.2f);

		//this is currently game breaking?
		//I would prefer not to do this.... but the OnLoadLevel func is worth calling	
		
		if(StellationManager.instance != null){
			//reset scene
		}else{
			LoadFile(0);
		}	

		//maybe I just use loadfile
	}

	public void FullReset(){
		if(OnReset != null){
			OnReset.Invoke();
		}
		
		SceneController.instance.Unload();
		
		editor.DeselectPoints();
		editor.DeselectSpline();
		Point.Points.Clear();
		Spline.Splines.Clear();
	}
	
	public void ReloadScene()
	{
		LoadSceneDelayed(0);
	}

	public void LoadFile(float delay = 0)
	{
		StartCoroutine(LoadFileTransition(delay));
	}

	public IEnumerator LoadFileTransition(float delay = 0)
	{
		Time.timeScale = 0;

		GlitchEffect.Fizzle(0.25f);
		yield return new WaitForSecondsRealtime(0.25f);
		/*if (delay > 0)
		{
			yield return StartCoroutine(FadeOut());
		}*/

		//ermmmmm, I guess I can assign it here?

		curLevel = SceneController.instance.GetCurLevel();

		if (SceneController.instance.activeScenes.Count > 0)
		{
			SceneController.instance.UnloadScene(SceneController.instance.activeScenes[0]);
		}
		
		StellationController c = editor.Load(curLevel);
		activeStellation = c;
		
		if (!MapEditor.editing)
		{
			SceneController.instance.activeScenes.Add(c);
		}

		Time.timeScale = 1;

		InitializeLevel();
		
		//awkwardddddd
		//start audio
		AudioManager.instance.PlayLevelSounds();
		/*if (delay > 0)
		{
			StartCoroutine(FadeIn());
		}*/
		
		
	}

	public void LoadSceneDelayed(float f)
	{
		StartCoroutine(LoadSceneTransition(f));
	}
	
	IEnumerator LoadSceneTransition(float delay = 0)
	{
		Time.timeScale = 0;

		
		yield return new WaitForSecondsRealtime(0.5f);
		
		Time.timeScale = 1;
		
		if (delay > 0)
		{
			yield return StartCoroutine(FadeOut());
		}

		LoadScene();
		
		if (delay > 0)
		{
			StartCoroutine(FadeIn());
		}
	}

	public void QuitLevel(bool goNext = false)
	{
		if (SceneController.instance.curSetIndex > -1 && SceneController.instance.curLevelSet.isScene)
		{
			SceneManager.UnloadSceneAsync(curLevel);
		}

		curLevel = "";
		
		Pause(false);
		FullReset();
		
		OpenMenu();
		
		if (goNext)
		{
			SceneController.instance.SelectNextLevel(true);
		}
	}
	
	public void LoadScene()
	{
		
		if (curLevel != "")
		{
			SceneManager.UnloadSceneAsync(curLevel);
		}

		//this could be bugged
		Services.PlayerBehaviour.Reset();
		curLevel = SceneController.instance.GetCurLevel();
		if (curLevel != "")
		{
			SceneManager.LoadScene(curLevel, LoadSceneMode.Additive);
		}
		else
		{
			FullReset();
			//OpenMenu();
		}

//		if (curLevel != "Editor")
//		{
//			Cursor.visible = false;
//			Cursor.lockState = CursorLockMode.Locked;
//		}
//		else
//		{
//			Cursor.lockState = CursorLockMode.None;
//			Cursor.visible = true;
//		}
	}
	
	public void Awake ()
	{
		curLevel = "";
		Point.Points = new List<Point>();
		Spline.Splines = new List<Spline>();
		Services.GameUI = canvas;
		Services.Word = Word;
		Services.mainCam = mainCam;
		Services.Prefabs = prefabs;
		Services.Player = Player;
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
		
		state = GameState.menu;
		MapEditor.editing = true;
		ToggleEditMode();
		

		if (SceneManager.sceneCount > 1)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				if (SceneManager.GetSceneAt(i).name != "Menu" && SceneManager.GetSceneAt(i).name != "Main")
				{
					curLevel = SceneManager.GetSceneAt(i).name;
				}
			}
		}

		Cursor.lockState = CursorLockMode.None;
		
		OpenMenu();

		Time.timeScale = 1;
	}

	public void TryChangeSetting(InputAction.CallbackContext context)
	{
		Vector2 input = context.ReadValue<Vector2>();
		
		if (settingsOpen)
		{
			foreach (SettingValue s in GameSettings.i.settings)
			{
				if (s.gameObject == EventSystem.current.currentSelectedGameObject)
				{
					if (input.x > 0f)
					{
						s.ChangeValue(1);
					}
					else if (input.x < 0)
					{
						s.ChangeValue(-1);
					}
				}
			}
		}
	}
	public void OpenMenu()
	{	
		if (SceneController.instance.curSetIndex < 0)
		{
			SceneController.instance.curSetIndex = 0;
		}

		Services.Player.SetActive(false);
		
		menu.SetActive(true);
		SceneController.instance.SelectLevelSet();

		state = GameState.menu;
		
		playerInput.SwitchCurrentActionMap("UI");
		
		if (MapEditor.editing)
		{
			ToggleEditMode();
		}
		
		if(OnReset != null){
			OnReset.Invoke();
		}
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		
		EventSystem.current.SetSelectedGameObject(SceneController.instance.levelButton.gameObject);
		
	}

	public void CloseMenu()
	{
		menu.SetActive(false);
			
		if (curLevel != "Editor") 
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		if (settingsOpen)
		{
			OpenSettings();
		}
		
		ShowWord("", false);
		ShowImage(null, false);
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
			}

			Cursor.visible = false;
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
		
		
		if(Input.GetKeyDown(KeyCode.Space))
		{
			if (!MapEditor.typing)
			{
				ToggleEditMode();
			}
		}

		if (state == GameState.playing)
		{
			if (!MapEditor.editing)
			{

				if (Services.PlayerBehaviour.curPoint != null)
				{
					Services.PlayerBehaviour.Step();
					CameraFollow.instance.FollowPlayer();
				}
					
				foreach (Spline s in Spline.Splines)
				{

					if (!s.locked && !s.reactToPlayer && !s.isPlayerOn)
					{
						s.DrawSpline();
						
					}
				}
			}
			else
			{
				editor.Step();
				
				foreach (Spline s in Spline.Splines)
				{
					s.DrawSplineOverride();
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
				if (state == GameState.menu)
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

		Services.PlayerBehaviour.curPoint = p;
		Services.PlayerBehaviour.transform.position = p.Pos;
		Services.PlayerBehaviour.curPoint.OnPlayerEnterPoint();
		
		Services.PlayerBehaviour.flow = flow;
		Services.PlayerBehaviour.curSpeed = curSpeed;
		
		Services.PlayerBehaviour.ResetFX();
	}
		public void InitializeLevel()
	{

		//the stellation initializes its points on start...
		//we may be forgiven for only initializing splines?
		if (Spline.Splines.Count > 0){
			for (int i = Spline.Splines.Count - 1; i >= 0; i--)
			{
				Debug.Log("reinitializing splines");

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

		activeStellation.Initialize();
		activeStellation.Setup();
		activeStellation.Draw();
		
		
		
//		foreach (StellationController c in SceneController.instance.activeScenes)
//		{
//			
//			if (c == SceneController.instance.activeScenes[SceneController.instance.activeScenes.Count-1])
//			{
//				c.MoveUp(SceneController.instance.activeScenes.Count);
//			}
//			
//			for (int i = c._points.Count - 1; i >= 0; i--)
//			{
//				c._points[i].Initialize();
//			}
//		}

/*	foreach (Point p in Point.Points)
		{
			p.Initialize();
		}*/ 

		if (Services.StartPoint == null && Point.Points.Count > 0)
		{
			Services.StartPoint = Point.Points[0];
		}

		
		if (!MapEditor.editing)
		{
			playerInput.SwitchCurrentActionMap("Player");
			Services.Player.SetActive(true);
			Services.PlayerBehaviour.Initialize();
		}
		
		
		OnReset.Invoke();

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
					CloseMenu();
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
					editor.Save();
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

	public void ShowWord(string m,  bool show = true)
	{
		Word.text = m;
		if (show)
		{
			Word.color = Color.white;
		}
		else
		{
			Word.color = Color.clear;
		}
	}
	
	public void ShowImage(Sprite s, bool show = true)
	{
		image.sprite = s;
		if (show)
		{
			image.color = Color.white;
		}
		else
		{
			image.color = Color.clear;
			
		}
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
	
	public IEnumerator LevelIntro(LevelSet l)
	{
		
//		ShowWord(l.title);
//		ShowImage(l.image);

		description.text = l.description;
		
//		yield return new WaitForSeconds(0.25f);
//		
//		ShowWord("", false);
		Word.gameObject.SetActive(false);

//		float t = 0;
//		while (t < 3)
//		{
//			t += Time.deltaTime;
//			if (Input.anyKeyDown)
//			{
//				break;
//			}
//			yield return null;
//		}

		yield return null;
		
		Word.gameObject.SetActive(true);
		SceneController.instance.LoadStellation();
		
		description.text = "";

//		ShowImage(null, false);

		state = GameState.playing;
		
		playerInput.SwitchCurrentActionMap("Player");
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
