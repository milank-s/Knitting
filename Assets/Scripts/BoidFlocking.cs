using UnityEngine;
using System.Collections;

public class BoidFlocking : MonoBehaviour
{
	public float minVelocity;
	public float maxVelocity;
	public float randomness;

	public Transform target;
	public Vector3 velocity;
	public BoidController controller;

	public void Start(){
		
		BoidController.instance.AddBoid(this);
		this.enabled = false;
	}
	public void SetVelocity(Vector3 v){
		velocity = v;
	}

	public void Steer ()
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
		

		transform.position += velocity * Time.deltaTime;
		
	}
	
	private Vector3 Calc ()
	{
		Vector3 randomize = new Vector3 ((Random.value *2) -1, (Random.value * 2) -1, (Random.value * 2) -1);
		
		randomize.Normalize();
		
		Vector3 flockCenter = controller.flockCenter - transform.position;
		Vector3 flockVelocity = controller.flockVelocity - velocity;
		Vector3 follow = target.position - transform.position;
		Vector3 avoid = transform.position - Services.PlayerBehaviour.pos;
		
		return (flockCenter + flockVelocity + (follow * 100) + avoid + randomize * randomness);
	}

}