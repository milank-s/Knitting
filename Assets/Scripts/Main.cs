	﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

	public GameObject Cursor;
	public GameObject Player;
	public GameObject StartPoint;
	public Image PauseScreen;

	void Awake () {
		Services.Prefabs = GetComponent<PrefabManager>();
		Services.Player = Player;
		Services.PlayerBehaviour = Player.GetComponent<PlayerBehaviour>();
		Services.StartPoint = StartPoint.GetComponent<Point>();
		Services.Cursor = Cursor;
		PointManager._pointsHit = new List<Point> ();
		PointManager._connectedPoints = new List<Point> ();
		Services.Sounds = GetComponent<SoundBank> ();
		StartCoroutine(FadeIn());
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
}
