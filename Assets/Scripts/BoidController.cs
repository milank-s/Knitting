using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BoidController : MonoBehaviour{
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
	}

	void Step (){
	
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
		b.minVelocity = minVelocity;
		b.maxVelocity = maxVelocity;
		b.randomness = randomness;
		b.controller = this;
		flockSize = boids.Count;
	}
}