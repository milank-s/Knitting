using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	public float speed;
	public Transform target;
	private Vector3 velocity = Vector2.zero;
	public Vector3 offset;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.SmoothDamp (transform.position, target.position + offset, ref velocity, speed);

		if (Services.PlayerBehaviour.curSpline != null) {
			GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, Mathf.Clamp(Services.PlayerBehaviour.GetFlow() * 10, 15, 40), Time.deltaTime);
		}

//		GetComponent<Camera>().orthographicSize = 10 + Mathf.Abs(target.GetComponent<PlayerTraversal> ().GetFlow());
	}
}
