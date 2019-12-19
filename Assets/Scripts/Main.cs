	 using System.Collections;
	using System.Collections.Generic;
	 using UnityEngine;
	 using UnityEngine.UI;
	 using UnityEngine.SceneManagement;
	 using UnityEngine.InputSystem;

public class Main : MonoBehaviour {

	public GameObject cursor;
	public GameObject Player;
	public Image PauseScreen;
	public GameObject PauseMenu;
	public Text Word;
	public FXManager fx;
	public GameObject canvas;
	public static bool usingJoystick;
	public Transform pointParent;
	public Transform splineParent;
	private string curLevel;
	public MapEditor editor;

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
	}

	private bool paused;

	[SerializeField] public string loadFileName;
	
	void Init()
	{
		
	}

	public void Reset()
	{
		
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
		if (delay != 0)
		{
			StartCoroutine(LoadFileTransition(m, delay));
		}
		else
		{
			LoadLevelFile(m);
		}
	}

	public IEnumerator LoadFileTransition(string m, float delay = 0)
	{
		yield return new WaitForSeconds(delay);
		
		StartCoroutine(FadeOut());
		yield return new WaitForSeconds(fadeLength);
		LoadLevelFile(m);
		
		StartCoroutine(FadeIn());
	}

	public void LoadLevelFile(string m)
	{
		MapEditor.Load(m);
		InitializeLevel();
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

		if (curLevel == "Editor")
		{
			LeaveEditMode();
		}
		
		Services.PlayerBehaviour.Reset();

		if (i != "")
		{
			SceneManager.LoadScene(i, LoadSceneMode.Additive);
		}
		else
		{
			Reset();
		}

		curLevel = i;

		if (curLevel != "Editor")
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		else
		{
			
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		if (SceneManager.GetSceneByName("Menu").isLoaded)
		{
			SceneManager.UnloadSceneAsync("Menu");
		}

		_paused = false;
	}
	
	
	void Awake ()
	{
		
		curLevel = "";
		Point.Points = new List<Point>();
		Spline.Splines = new List<Spline>();
		Services.GameUI = canvas;
		Services.Word = Word;
		Services.mainCam = Camera.main;
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
		OpenMenu();
	}

	void Start()
	{
		paused = true;
		
//	#if UNITY_STANDALONE
//		SceneManager.LoadScene(1, LoadSceneMode.Additive);
//	#endif

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

		ToggleEditMode(false);
		Cursor.lockState = CursorLockMode.None;
	}

	public void OpenMenu()
	{
		SceneManager.LoadScene(1, LoadSceneMode.Additive);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
//		Time.timeScale = 0;
		paused = true;
	}

	public void CloseMenu()
	{
		Time.timeScale = 1;
		SceneManager.UnloadSceneAsync(1);
				
		if (curLevel != "Editor") 
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}

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
				MapEditor.editing = !MapEditor.editing;
				ToggleEditMode(MapEditor.editing);
			}
		}

		if (!paused)
		{
			if (!MapEditor.editing)
			{
				if (!PointManager.PointsHit())
				{
					CameraFollow.instance.FollowPlayer();
				}
				else
				{
					CameraFollow.desiredFOV += Time.deltaTime * 5;
				}

				if (Services.PlayerBehaviour.curPoint != null)
				{
					Services.PlayerBehaviour.Step();
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
		Services.mainCam.GetComponent<CameraFollow>().WarpToPlayer();
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
		
		for (int i = Point.Points.Count - 1; i >= 0; i--)
		{
			Point.Points[i].Initialize();
		}

		if (Services.StartPoint == null && Point.Points.Count > 0)
		{
			Services.StartPoint = Point.Points[0];
		}
		
		if (!MapEditor.editing)
		{
			Services.PlayerBehaviour.Initialize();
		}
		
		Services.mainCam.GetComponent<CameraFollow>().WarpToPlayer();
		Services.main.fx.Reset();
	}
	
	public void ToggleEditMode(bool enter)
	{
			editor.gameObject.SetActive(MapEditor.editing);
			
			canvas.SetActive(!enter);
			Player.SetActive(!enter);
			Services.mainCam.GetComponent<CameraFollow>().enabled = !enter;
			RenderSettings.fog = !enter;
			//Services.mainCam.GetComponentInChildren<Camera>().enabled = !enter;
			if (enter)
			{
				if (paused)
				{
					CloseMenu();
					_paused = false;
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
				editor.TogglePlayMode();
			}
		
	}

	public void LeaveEditMode()
	{
		Services.mainCam.GetComponent<CameraFollow>().enabled = true;
		canvas.SetActive(true);
		Player.SetActive(true);
		MapEditor.editing = false;
	}
	
	IEnumerator FadeIn(){
		float t = 0;
		yield return new WaitForSeconds(0.01f);
		
		while (t < fadeLength){
			PauseScreen.color = Color.Lerp(Color.black, Color.clear, Easing.QuadEaseIn(t/fadeLength));
			t += Time.deltaTime;
			yield return null;
		}
		
		PauseScreen.color = Color.clear;

		if (curLevel == "")
		{
			if (curLevel == "")
			{
				Services.main.OpenMenu();
			}
		}
	}

	public void ShowWord(string m)
	{
		StartCoroutine(FlashWord(m));
	}

	IEnumerator FlashWord(string m)
	{
		
		float t = 0;
		Word.color = Color.white;
		Word.text = m;
		while (t < 1)
		{
			Word.color = Color.Lerp(Color.white, Color.clear, t);
			t += Time.deltaTime * 2;
			yield return null;
		} 
	}
	IEnumerator FadeOut(){
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
