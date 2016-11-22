using UnityEngine;
using System.Collections;

public class WaveEffect : MonoBehaviour {
	public GameObject Wave;
	public float speed;
	Material m;

	// Use this for initialization
	void Start () {
		m = GetComponent<Renderer> ().material;
		m.SetFloat ("_InnerRadius", -1.5f);
	}
	
	// Update is called once per frame
	void Update () {
		m.SetFloat ("_InnerRadius", m.GetFloat ("_InnerRadius") + Time.deltaTime/speed);
	}
}
