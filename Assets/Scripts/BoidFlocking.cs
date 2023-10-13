using UnityEngine;
using System.Collections;

public class BoidFlocking : MonoBehaviour
{
	private GameObject Controller;
	private bool inited = false;
	private float minVelocity;
	private float maxVelocity;
	private float randomness;
	private GameObject chasee;

	Vector3 velocity;

	public void SetVelocity(Vector3 v){
		velocity = v;
	}
	
	void Steer ()
	{
		
		if (inited)
		{
			velocity = velocity + Calc () * Time.deltaTime;
			
			// enforce minimum and maximum speeds for the boids
			float speed = velocity.magnitude;
			if (speed > maxVelocity)
			{
				velocity = velocity.normalized * maxVelocity;
			}
			else if (speed < minVelocity)
			{
				velocity = velocity.normalized * minVelocity;
			}
		}

		transform.position += velocity * Time.deltaTime;
		
	}
	
	private Vector3 Calc ()
	{
		Vector3 randomize = new Vector3 ((Random.value *2) -1, (Random.value * 2) -1, (Random.value * 2) -1);
		
		randomize.Normalize();
		BoidController boidController = Controller.GetComponent<BoidController>();
		Vector3 flockCenter = boidController.flockCenter;
		Vector3 flockVelocity = boidController.flockVelocity;
		Vector3 follow = chasee.transform.localPosition;
		
		flockCenter = flockCenter - transform.localPosition;
		flockVelocity = flockVelocity - velocity;
		follow = follow - transform.localPosition;
		
		return (flockCenter + flockVelocity + follow * 2 + randomize * randomness);
	}
	
	public void SetController (GameObject theController)
	{
		Controller = theController;
		BoidController boidController = Controller.GetComponent<BoidController>();
		minVelocity = boidController.minVelocity;
		maxVelocity = boidController.maxVelocity;
		randomness = boidController.randomness;
		chasee = boidController.chasee;
		inited = true;
	}
}