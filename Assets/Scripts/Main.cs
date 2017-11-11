using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public GameObject Cursor;
	public GameObject Player;

	void Awake () {


		Services.Player = Player;
		Services.PlayerBehaviour = Player.GetComponent<PlayerBehaviour>();

		Services.Cursor = Cursor;
		Services.Points = GetComponent<PointManager> ();
		Services.Prefabs = GetComponent<PrefabManager> ();
	}

}
