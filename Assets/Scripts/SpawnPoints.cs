using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints : MonoBehaviour {

	public int amount;
	// Use this for initialization
	void Start () {
		for (int i = 0; i < amount; i++) {
			Instantiate (Services.Prefabs.point, transform.position, Quaternion.identity);
		}
	}
}
