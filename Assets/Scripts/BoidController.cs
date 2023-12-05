using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BoidController : MonoBehaviour{

	[Range(0.0f, 0.9f)]
    public float velocityVariation = 0.5f;

    [Range(0.1f, 20.0f)]
    public float rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)]
    public float neighborDist = 2.0f;

    public LayerMask searchLayer;
	public float minVelocity = 5;
	public float maxVelocity = 20;
	public float randomness = 1;
	
	public Vector3 flockCenter;
	public Vector3 flockVelocity;
	
	private List<BoidFlocking> boids;
	
	public static BoidController instance;

	int flockSize;

	void Awake(){
		instance = this;
		boids = new List<BoidFlocking>();
		Services.main.OnReset += OnReset;
	}

	public void OnReset(){
		boids = new List<BoidFlocking>();
	}
	public void Step (){
	
		Vector3 theCenter = Vector3.zero;
		Vector3 theVelocity = Vector3.zero;
	
		foreach (BoidFlocking boid in boids) {
			if (boid.enabled) {
				theCenter = theCenter + boid.transform.position;
				theVelocity = theVelocity + boid.velocity;	
			}
			
			flockCenter = theCenter / (flockSize);
			flockVelocity = theVelocity / (flockSize);
		}
		
	}

	public void AddBoid(BoidFlocking b){
		boids.Add(b);
		b.randomness = randomness;
		b.controller = this;
		flockSize = boids.Count;
	}
}