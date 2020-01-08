	 using System.Collections;
	using System.Collections.Generic;
	 using UnityEngine;
	 using UnityEngine.UI;
	 using UnityEngine.SceneManagement;
	 using UnityEngine.InputSystem;

public class Main : MonoBehaviour {

	public enum GameState {playing, paused, editing}

	public GameState state;
	public GameObject cursor;
	public GameObject Player;
	public Image PauseScreen;
	public GameObject PauseMenu;
	public Text Word;
	public Image image;
	public FXManager fx;
	public GameObject canvas;
	public static bool usingJoystick;
	public Transform pointParent;
	public Transform splineParent;
	private string curLevel;
	public MapEditor editor;
	public Camera mainCam;
	public GameObject menu;
	
	[SerializeField]
	private float fadeLength = 0.1f;
	public Gamepad controller
	{
		get
		{
			return Gamepad.current;
		}
	}
	
	public bool _paused
	{
		set
		{
			paused = value;
			if (value)
			{
				PauseMenu.SetActive(true);
			}
			else
			{
				PauseMenu.SetActive(false);
			}
		}

		get { return paused; }
	}

	private bool paused;

	[SerializeField] public string loadFileName;
	

	public void Reset()
	{
		SceneController.instance.curLevel = 0;
		SceneController.instance.activeScenes.Clear();
		
		editor.DeselectPoints();
		editor.DeselectSpline();
		
		//should probably save player made stellation here
		for (int i = Spline.Splines.Count-1; i >= 0; i--)
		{
			Destroy(Spline.Splines[i]);
		}
		
		for (int i = Point.Points.Count - 1 ; i >= 0; i--)
		{
			Point.Points[i].Destroy();
		}
		
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
		else
		{
			
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

		_paused = false;
	
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
		MapEditor.editing = false;
		paused = true;

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
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
//		Time.timeScale = 0;
		paused = true;
		SceneController.instance.SelectLevelSet();
	}

	public void CloseMenu()
	{
		Time.timeScale = 1;
		menu.SetActive(false);
				
		if (curLevel != "Editor") 
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

		ShowWord("", false);
		ShowImage(null, false);
		paused = false;
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
		
		if (Input.GetKeyDown(KeyCode.P) && !MapEditor.typing)
		{
			_paused = !paused;
			if (paused)
			{
				OpenMenu();
			}
			else
			{
				CloseMenu();	
			}
		}
		
		if(Input.GetKeyDown(KeyCode.Space))
		{
			if (!MapEditor.typing)
			{
				ToggleEditMode();
			
			}
		}

		if (!paused)
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
			editor.gameObject.SetActive(MapEditor.editing);
			
			canvas.SetActive(!enter);
			Player.SetActive(!enter);
			Services.mainCam.GetComponent<CameraFollow>().enabled = !enter;
			RenderSettings.fog = !enter;
			//Services.mainCam.GetComponentInChildren<Camera>().enabled = !enter;
			if (enter)
			{
				state = GameState.editing;
				if (paused)
				{
					CloseMenu();
				}
				foreach (Point p in Point.Points)
				{
					p.Reset();
				}

				foreach (Spline s in Spline.Splines)
				{
					s.ResetVectorLine();
				}

				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				state = GameState.playing;
				editor.TogglePlayMode();
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
		
		ShowWord(l.title);
		ShowImage(l.image);
		
		
		SceneController.instance.LoadNextStellation();
		
		
		yield return new WaitForSeconds(0.25f);
		
		ShowWord("", false);
		
		yield return new WaitForSeconds(0.1f);
		
		ShowImage(null, false);
		
		
		
		
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
