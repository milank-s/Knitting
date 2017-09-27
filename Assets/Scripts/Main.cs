using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public GameObject Cursor;
	public GameObject Player;

	void Start () {
		Services.Player = Player;
		Services.Cursor = Cursor;
		Services.Nodes = GetComponent<NodeManager> ();
		Services.Prefabs = GetComponent<PrefabManager> ();
	}

}
