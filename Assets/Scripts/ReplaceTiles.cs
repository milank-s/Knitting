using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceTiles : MonoBehaviour {

	public string tileTag;
	public GameObject tilePrefab;
	[ExecuteInEditMode]
	void Start () {
		Replace();
	}

	void Replace(){
		foreach(GameObject g in GameObject.FindGameObjectsWithTag(tileTag)){
			//spawn prefab
		}
	}
}
