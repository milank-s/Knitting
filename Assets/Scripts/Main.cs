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
	private string curLevel;

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

	public void ReloadScene()
	{
		LoadLevelDelayed(curLevel);
	}

	public void LoadLevelDelayed(string m)
	{
		StartCoroutine(LoadTransition(m));
	}
	IEnumerator LoadTransition(string i)
	{
		StartCoroutine(FadeOut());
		
		yield return new WaitForSeconds(1);

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

		SceneManager.LoadScene(i, LoadSceneMode.Additive);

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
		StartCoroutine(FadeIn()); 
	}

	void Start()
	{
		paused = true;
	#if UNITY_STANDALONE
		SceneManager.LoadScene(1, LoadSceneMode.Additive);
	#endif

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
				SceneManager.LoadScene(1, LoadSceneMode.Additive);
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				SceneManager.UnloadSceneAsync(1);
				
				if (curLevel != "Editor")
				{
					Cursor.visible = false;
					Cursor.lockState = CursorLockMode.Locked;
				}
			}
		}

		if (!paused)
		{
			if (!MapEditor.editing)
			{
				CameraFollow.instance.FollowPlayer();
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
				Spline.Splines[i].SetUpReferences();
			}
		}
		
		for (int i = Point.Points.Count - 1; i >= 0; i--)
		{
			Point.Points[i].Initialize();
		}
		
		if (Services.StartPoint != null && !MapEditor.editing)
		{
			Services.PlayerBehaviour.Initialize();
		}
		
		Services.mainCam.GetComponent<CameraFollow>().WarpToPlayer();
	}
	
	public void EnterEditMode(bool enter)
	{
		canvas.SetActive(!enter);
		Player.SetActive(!enter);
		Services.mainCam.GetComponent<CameraFollow>().enabled = !enter;
		RenderSettings.fog = !enter;
		//Services.mainCam.GetComponentInChildren<Camera>().enabled = !enter;
		if (!enter)
		{
			
			InitializeLevel();
			
			Cursor.lockState = CursorLockMode.Locked;
		}
		else
		{
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
		while (t < 1.2f){
			PauseScreen.color = Color.Lerp(new Color (0,0,0,1), new Color (0,0,0,0), Easing.QuadEaseIn(t));
			t += Time.deltaTime * 3;
			yield return null;
		}
		
		
	}

	IEnumerator FadeInOut()
	{
		float t = 0;
		while (t < 1.2f){
			PauseScreen.color = Color.Lerp(new Color (0,0,0,1), new Color (0,0,0,0), Easing.QuadEaseIn(t));
			t += Time.deltaTime * 3;
			yield return null;
		}
		t = 0;
		yield return new WaitForSeconds(0.05f);
		
		while (t < 1.2f){
			PauseScreen.color = Color.Lerp(new Color (0,0,0,0), new Color (0,0,0,1), Easing.QuadEaseIn(t));
			t += Time.deltaTime * 3;
			yield return null;
		}
		
	}
	IEnumerator FadeOut(){
		float t = 0;
		
		AudioManager.instance.MuteSynths(true);
		
		while (t < 1.2f)
		{

			PauseScreen.color = Color.Lerp(new Color (0,0,0,0), new Color (0,0,0,1), Easing.QuadEaseIn(t));
			t += Time.deltaTime * 3;
			yield return null;
		}
	}

}
