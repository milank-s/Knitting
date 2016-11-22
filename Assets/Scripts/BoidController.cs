using UnityEngine;
using System.Collections;

public class BoidController : MonoBehaviour{
	public float minVelocity = 5;
	public float maxVelocity = 20;
	public float randomness = 1;
	public int flockSize = 20;
	public GameObject prefab;
	public GameObject chasee;
	
	public Vector3 flockCenter;
	public Vector3 flockVelocity;
	
	private GameObject[] boids;
	
	void Start()
	{
		StartCoroutine (FindBoids ());
	}
	
	void Update (){
		if (boids != null) {
			Vector3 theCenter = Vector3.zero;
			Vector3 theVelocity = Vector3.zero;
		
			foreach (GameObject boid in boids) {
				if (boid.GetComponent<BoidFlocking> ().enabled) {
					theCenter = theCenter + boid.transform.localPosition;
					theVelocity = theVelocity + boid.GetComponent<Rigidbody> ().velocity;
				}
				
				flockCenter = theCenter / (flockSize);
				flockVelocity = theVelocity / (flockSize);
			}
		}
	}

	IEnumerator FindBoids(){

		yield return new WaitForSeconds(0.5f);
		boids = GameObject.FindGameObjectsWithTag ("Node");

		foreach (GameObject b in boids) {
			b.GetComponent<BoidFlocking> ().SetController (gameObject);
		}
	}
}