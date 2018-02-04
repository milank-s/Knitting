using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public GameObject Cursor;
	public GameObject Player;

	void Awake () {

		SplineTurtle.maxCrawlers = 0;
		SplineTurtle.maxTotalPoints = 0;

		Services.Player = Player;
		Services.PlayerBehaviour = Player.GetComponent<PlayerBehaviour>();

		Services.Cursor = Cursor;
		Services.Prefabs = GetComponent<PrefabManager> ();
		Services.Prefabs.LoadResources ();
		PointManager._pointsHit = new List<Point> ();
	}

}
