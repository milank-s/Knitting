using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class FadeLine : MonoBehaviour {

	VectorLine l;
	// Use this for initialization
	void Start () {
		l = GetComponent<VectorLine> ();	
	}
	
	// Update is called once per frame
	void Update () {
		l.color = Color.Lerp (l.color, Color.black, Time.deltaTime);
	}
}
