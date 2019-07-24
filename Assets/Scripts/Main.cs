	 using System.Collections;
using System.Collections.Generic;
	 using UnityEditorInternal;
	 using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

	public GameObject cursor;
	public GameObject Player;
	public Image PauseScreen;
	public GameObject PauseMenu;
	public Text Word;
	public GameObject canvas;
	public bool _paused
	{
		set
		{
			paused = value;
			if (value)
			{
				PauseMenu.SetActive(true);
				Time.timeScale = 0;
			}
			else
			{
				PauseMenu.SetActive(false);
				Time.timeScale = 1;
			}
		}
	}

	private bool paused;
	
	void Awake ()
	{
		Point.Points = new List<Point>();
		Spline.Splines = new List<Spline>();
		Services.GameUI = canvas;
		Services.Word = Word;
		Services.mainCam = Camera.main;
		Services.Prefabs = GetComponent<PrefabManager>();
		Services.Player = Player;
		Services.PlayerBehaviour = Player.GetComponent<PlayerBehaviour>();
		Services.Cursor = cursor;
		PointManager._pointsHit = new List<Point> ();
		PointManager._connectedPoints = new List<Point> ();
		Services.Sounds = GetComponent<SoundBank> ();
		Services.main = this;
		PauseScreen.color = new Color(0,0,0,1);
		PauseMenu.SetActive(false);
		StartCoroutine(FadeIn()); 
	}

	void Start()
	{
		canvas.SetActive(!MapEditor.editing);
		
		InitializeMap();
		
		if (Services.StartPoint != null && !MapEditor.editing)
		{
			Services.PlayerBehaviour.Initialize();
		}
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			_paused = !paused;
		}
		if (!paused)
		{
			if (!MapEditor.editing)
			{
				if (Services.PlayerBehaviour.curPoint != null)
				{
					Services.PlayerBehaviour.Step();
				}
				
			}
		}
		else
		{
	
		}
	}

	public void InitializeMap()
	{
		for (int i = Point.Points.Count - 1; i >= 0; i--)
		{
			if (Point.Points[i] == null)
			{
				Point.Points.RemoveAt(i);
			}
			else
			{
				Point.Points[i].Initialize();
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
				 Spline.Splines[i].SetupSpline();
			}
		}
	}
	public void EnterEditMode(bool enter)
	{
		if (!enter)
		{
			InitializeMap();
		}
		canvas.SetActive(!enter);
		Player.SetActive(!enter);
		Services.mainCam.GetComponent<CameraFollow>().enabled = !enter;
		
		//Services.mainCam.GetComponentInChildren<Camera>().enabled = !enter;
		if (!enter)
		{
			Cursor.lockState = CursorLockMode.Locked;
			if (Services.StartPoint != null)
			{
				Services.PlayerBehaviour.Initialize();
			}
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	IEnumerator FadeIn(){
		float t = 0;
		yield return new WaitForSeconds(0.1f);
		while (t < 1.2f){
			PauseScreen.color = Color.Lerp(new Color (0,0,0,1), new Color (0,0,0,0), Easing.QuadEaseIn(t));
			t += Time.deltaTime;
			yield return null;
		}
	}
	
	IEnumerator FadeOut(){
		float t = 0;
		yield return new WaitForSeconds(0.1f);
		while (t < 1.2f){
			PauseScreen.color = Color.Lerp(new Color (0,0,0,0), new Color (0,0,0,1), Easing.QuadEaseIn(t));
			t += Time.deltaTime;
			yield return null;
		}
	}
}
