	 using System.Collections;
	using System.Collections.Generic;
	 using UnityEngine;
	 using UnityEngine.UI;
	 using UnityEngine.SceneManagement;
	 using UnityEngine.InputSystem;

public class Main : MonoBehaviour {

	public enum GameState {playing, paused, editing, menu}

	public GameState state;
	public GameObject cursor;
	public GameObject Player;
	public Image PauseScreen;
	public GameObject PauseMenu;
	public Text Word;
	public Text description;
	public Image image;
	public FXManager fx;
	public GameObject canvas;
	public static bool usingJoystick;
	public Transform pointParent;
	public Transform splineParent;
	private string curLevel;
	public MapEditor editor;
	public GameObject editorUI;
	public Camera mainCam;
	public GameObject menu;
	public GameObject settings;
	private bool settingsOpen;
		
	[SerializeField]
	private float fadeLength = 0.1f;
	public Gamepad controller
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
	}

	public void Quit()
	{
		Application.Quit();
	}
	public void Reset()
	{
		
		
		SceneController.instance.Reset();
		
		editor.DeselectPoints();
		editor.DeselectSpline();

		
		Point.Points.Clear();
		Spline.Splines.Clear();
		Services.PlayerBehaviour.Reset();
		Services.fx.Reset();

	
	}
	
	public void ReloadScene()
	{
		LoadLevelDelayed(curLevel, 0);
	}

	public void LoadFile(string m, float delay = 0)
	{
		Time.timeScale = 1;
		StartCoroutine(LoadFileTransition(m, delay));
	}

	public IEnumerator LoadFileTransition(string m, float delay = 0)
	{
		yield return new WaitForSeconds(delay);
		
		StartCoroutine(FadeOut());
		yield return new WaitForSeconds(fadeLength);
		
		StellationController c = MapEditor.Load(m);
		if (!MapEditor.editing)
		{
			SceneController.instance.activeScenes.Add(c);
		}
		
		yield return null;

		InitializeLevel();
		
		
		StartCoroutine(FadeIn());
	}

	public void LoadLevelDelayed(string m, float f)
	{
		StartCoroutine(LoadTransition(m,f));
	}
	
	IEnumerator LoadTransition(string i, float delay = 0)
	{
		Time.timeScale = 1;
		
		yield return new WaitForSeconds(delay);
		
		StartCoroutine(FadeOut());
		
		yield return new WaitForSeconds(fadeLength);

		LoadLevel(i);
		
		StartCoroutine(FadeIn());
	}

	public void QuitLevel()
	{
		
		Pause(false);
		Reset();
		
		OpenMenu();
	}
	
	public void LoadLevel(string i)
	{
		
		if (curLevel != "")
		{
			SceneManager.UnloadSceneAsync(curLevel);
		}

		Services.PlayerBehaviour.Reset();

		if (i != "")
		{
			SceneManager.LoadScene(i, LoadSceneMode.Additive);
		}
		else
		{
			Reset();
			OpenMenu();
		}

		curLevel = i;

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

		state = GameState.playing;

	}
	
	
	void Awake ()
	{
		
		curLevel = "";
		Point.Points = new List<Point>();
		Spline.Splines = new List<Spline>();
		Services.GameUI = canvas;
		Services.Word = Word;
		Services.mainCam = mainCam;
		Services.Prefabs = GetComponent<PrefabManager>();
		Services.Player = Player;
		Services.fx = fx;
		Services.PlayerBehaviour = Player.GetComponent<PlayerBehaviour>();
		Services.Cursor = cursor;
		PointManager._pointsHit = new List<Point> ();
		PointManager._connectedPoints = new List<Point> ();
		Services.Sounds = GetComponent<SoundBank> ();
		Services.main = this;
		PauseScreen.color = new Color(0,0,0,0);
		PauseMenu.SetActive(false);
		
		MapEditor.editing = true;
		ToggleEditMode();
	}

	void Start()
	{
		state = GameState.menu;

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
	}

	public void OpenMenu()
	{
		menu.SetActive(true);
		SceneController.instance.SelectLevelSet();

		
		state = GameState.menu;
		
		if (MapEditor.editing)
		{
			ToggleEditMode();
		}
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
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
		if (pause)
		{
			
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
		
		Time.timeScale = pause ? 0 : 1;
		
	}
	
	void Update()
	{
		CameraFollow.instance.uiCam.fieldOfView = CameraFollow.instance.cam.fieldOfView;
		
		if (Input.GetAxis ("Joy Y") != 0 && !usingJoystick)
		{
			usingJoystick = true;
		}

//		if (Input.GetKeyDown (KeyCode.R)) {
//			SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
//		}
		
		if (Input.GetKeyDown(KeyCode.Escape) && !MapEditor.typing)
		{
			
			if (state == GameState.paused)
			{
				Pause(false);
				
			}
			else if(state != GameState.menu)
			{
				Pause(true);
			}
		}
		
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
	}

	public void WarpPlayerToNewPoint(Point p)
	{
		Services.StartPoint = p;
		float curSpeed = Services.PlayerBehaviour.curSpeed;
		float flow = Services.PlayerBehaviour.flow;
		Services.PlayerBehaviour.Initialize();
		Services.PlayerBehaviour.flow = flow;
		Services.PlayerBehaviour.curSpeed = curSpeed;
	}
	
	public void InitializeLevel()
	{
		for (int i = Point.Points.Count - 1; i >= 0; i--)
		{
			if (Point.Points[i] == null)
			{
				Point.Points.RemoveAt(i);
			}
			else
			{
				Point.Points[i].Clear();
			}
		}
		
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

		foreach (Point p in Point.Points)
		{
			p.Initialize();
		}

		if (Services.StartPoint == null && Point.Points.Count > 0)
		{
			Services.StartPoint = Point.Points[0];
		}
		
		if (!MapEditor.editing)
		{
			Services.PlayerBehaviour.Initialize();
		}
		
		Services.main.fx.Reset();
		
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
			//Services.mainCam.GetComponentInChildren<Camera>().enabled = !enter;
			if (enter)
			{
				
				
				if (state == GameState.menu)
				{
					CloseMenu();
					
					 state =  GameState.playing;
				}
				foreach (Point p in Point.Points)	
				{
					p.Reset();
				}

				foreach (Spline s in Spline.Splines)
				{
					s.ResetVectorLine();
				}

				
				state = GameState.playing;
				
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				if (state != GameState.menu)
				{
					editor.TogglePlayMode();
				}
			}		
	}
	
	public IEnumerator FadeIn(){
		float t = 0;
		yield return new WaitForSeconds(0.01f);
		
		while (t < fadeLength){
			PauseScreen.color = Color.Lerp(Color.black, Color.clear, Easing.QuadEaseIn(t/fadeLength));
			t += Time.deltaTime;
			yield return null;
		}
		
		PauseScreen.color = Color.clear;
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
		while (!Input.anyKey)
		{
			yield return null;
		}
		
		SceneController.instance.LoadNextStellation();
		
		description.text = "";

		
//		ShowImage(null, false);

		state = GameState.playing;
	}
	
	
	public IEnumerator FadeOut(){
		float t = 0;
		
		AudioManager.instance.MuteSynths(true);
		
		while (t < fadeLength)
		{
			PauseScreen.color = Color.Lerp(Color.clear, Color.black, Easing.QuadEaseIn(t/fadeLength));
			t += Time.deltaTime;
			yield return null;
		}

		PauseScreen.color = Color.black;
	}

}
