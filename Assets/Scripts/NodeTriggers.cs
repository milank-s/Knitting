using UnityEngine;
using System.Collections;

public class NodeTriggers : MonoBehaviour {

	public float speed;

	private bool pickedUp = false;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnTriggerEnter(Collider col){
		if (!pickedUp && col.gameObject.tag == "Player") {
			col.gameObject.SendMessage ("CreateWord", transform.position);
			col.gameObject.SendMessage ("Pickup", gameObject);
			pickedUp = true;
		}
	}

	void OnTriggerStay(Collider col){
		if (col.gameObject.tag == "Player" && Input.GetButtonDown("x")) {
			col.gameObject.SendMessage ("Pickup", gameObject);
			col.gameObject.SendMessage ("AddFlow", speed);
		}
	}

	public void SetPickup(bool x){
		pickedUp = x;
	}
}
