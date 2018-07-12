using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public GameObject Cursor;
	public GameObject Player;
	public GameObject StartPoint;
	void Awake () {

		Services.Player = Player;
		Services.PlayerBehaviour = Player.GetComponent<PlayerBehaviour>();
		Services.StartPoint = StartPoint.GetComponent<Point>();
		Services.Cursor = Cursor;
		PointManager._pointsHit = new List<Point> ();
		PointManager._connectedPoints = new List<Point> ();
		Services.Sounds = GetComponent<SoundBank> ();
	}

}
