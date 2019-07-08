	 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

	public GameObject Cursor;
	public GameObject Player;
	public Image PauseScreen;
	public GameObject PauseMenu;

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
	
	void Awake () {
		
		Services.Prefabs = GetComponent<PrefabManager>();
		Services.Player = Player;
		Services.PlayerBehaviour = Player.GetComponent<PlayerBehaviour>();
		Services.Cursor = Cursor;
		PointManager._pointsHit = new List<Point> ();
		PointManager._connectedPoints = new List<Point> ();
		Services.Sounds = GetComponent<SoundBank> ();
		PauseScreen.color = new Color(0,0,0,1);
		PauseMenu.SetActive(false);
		StartCoroutine(FadeIn()); 
	}

	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			_paused = !paused;
		}
		if (!paused)
		{
			
			Services.PlayerBehaviour.Step();
		}
		else
		{
	
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
