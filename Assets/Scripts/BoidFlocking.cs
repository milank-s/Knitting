using UnityEngine;
using System.Collections;

public class BoidFlocking : MonoBehaviour
{
	public float minVelocity;
	public float maxVelocity;
	public float randomness;
 	float noiseOffset;
	public Transform target;
	public Vector3 velocity;
	public BoidController controller;

	public void Start(){
		noiseOffset = Random.value * 10.0f;
		BoidController.instance.AddBoid(this);
		//this.enabled = false;
	}
	public void SetVelocity(Vector3 v){
		velocity = v;
	}

	public void SteerWithNeighbours()
    {
        var currentPosition = transform.position;
        var currentRotation = transform.rotation;

        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset) * 2.0f - 1.0f;
        var velocity = controller.minVelocity * (1.0f + noise * controller.velocityVariation);

        // Initializes the vectors.
        var separation = Vector3.zero;
        var alignment = controller.transform.forward;
        var cohesion = controller.transform.position;

        // Looks up nearby boids.
        var nearbyBoids = Physics.OverlapSphere(currentPosition, controller.neighborDist, controller.searchLayer);

        // Accumulates the vectors.
        foreach (Collider boid in nearbyBoids)
        {
            if (boid.gameObject.tag != "Collectible" || boid.gameObject == gameObject) continue;
            var t = boid.transform;
            separation += GetSeparationVector(t);
            alignment += t.forward;
            cohesion += t.position;
        }

		if(nearbyBoids.Length != 0){
			var avg = 1.0f / nearbyBoids.Length;
			alignment *= avg;
			cohesion *= avg;
			cohesion = (cohesion - currentPosition).normalized;
		}

		Vector3 follow = (target.position - transform.position);
		//Vector3 avoid = (transform.position - Services.PlayerBehaviour.pos);

        // Calculates a rotation from the vectors.
        var direction = separation + alignment + cohesion + follow; //+ avoid;
        var rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);

        // Applys the rotation with interpolation.
        if (rotation != currentRotation)
        {
            var ip = Mathf.Exp(-controller.rotationCoeff * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(rotation, currentRotation, ip);
        }

        // Moves forawrd.
        transform.position = currentPosition + transform.forward * (velocity * Time.deltaTime);
    }

	Vector3 GetSeparationVector(Transform t)
    {
        var diff = transform.position - t.position;
        var diffLen = diff.magnitude;
        var scaler = Mathf.Clamp01(1.0f - diffLen / controller.neighborDist);
        return diff * (scaler / diffLen);
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
		//use perlin noise?
		Vector3 randomize = new Vector3 ((Random.value *2) -1, (Random.value * 2) -1, (Random.value * 2) -1);
		
		randomize.Normalize();
		
		Vector3 flockCenter = controller.flockCenter - transform.position;
		Vector3 flockVelocity = controller.flockVelocity - velocity;
		Vector3 follow = target.position - transform.position;
		Vector3 avoid = transform.position - Services.PlayerBehaviour.pos;
		
		return (flockCenter + flockVelocity + (follow * 100) + avoid + randomize * randomness);
	}

}