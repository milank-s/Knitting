using UnityEngine;
using System.Collections;

public class LightUp : MonoBehaviour {

	public AudioClip sound;
	public float speed;
	float countDown;
	bool on = false;
	Light l;
	// Use this for initialization
	void Start () {
		l = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {
		l.enabled = on;
		l.intensity = countDown/0.5f;
		TurnOff ();
	}

	void TurnOff(){
		countDown -= Time.deltaTime / speed;
		if (countDown <= 0) {
			on = false;
		}
	}

	public bool IsOn(){
		return on;
	}

	public void SetOn(){
		on = true;
		countDown = 1;
	}

	public void OnTriggerEnter(Collider col){
		if (col.gameObject.tag == "Player") {
			SetOn ();
			col.gameObject.GetComponent<AudioSource> ().PlayOneShot (sound);
			ParticleSystem m = col.GetComponent<ParticleSystem> ();
			m.Emit(5);
		}
	}
}
