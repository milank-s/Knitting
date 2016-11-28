using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	private Vector3 velocity = Vector2.zero;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.SmoothDamp (transform.position, target.position, ref velocity, 0.1f);
		transform.position = new Vector3 (transform.position.x, transform.position.y, -15);

		GetComponent<Camera>().orthographicSize = 3 + Mathf.Abs(target.GetComponent<PlayerTraversal> ().GetFlow());
	}
}
