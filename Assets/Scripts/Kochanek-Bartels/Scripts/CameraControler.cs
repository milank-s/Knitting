using UnityEngine;
using System.Collections;


public class CameraControler : MonoBehaviour {
	public static Camera MainCamera;
	public float distance = 10.0f;

	public float xSpeed = 250.0f;
	public float ySpeed = 120.0f;

	private float x = 0.0f;
	private float y = 0.0f;
	
	private Vector3 Target=Vector3.zero;
	
	void Awake () 
	{
		MainCamera=GetComponent<Camera>();
	    Vector3 angles = transform.eulerAngles;
	    x = angles.y;
	    y = angles.x;
	}
	
	void LateUpdate () 
	{
	    if (Input.GetKey(KeyCode.LeftAlt))
		{
			
			if(Input.GetMouseButton(0))
			{
		        x+=Input.GetAxis("Mouse X") * xSpeed * 0.02f;
		        y-=Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
		 		
				Quaternion rotation = Quaternion.Euler(y, x, 0);
		        transform.rotation = rotation;
			}
			else if(Input.GetMouseButton(2))
			{
				Target+=transform.rotation*new Vector3(-Input.GetAxis("Mouse X") * xSpeed * 0.001f,0,0);
				Target+=transform.rotation*new Vector3(0,-Input.GetAxis("Mouse Y") * xSpeed * 0.001f,0);
			}
		}
		
		
		if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            distance++;
 
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            distance--;
		}
		
		Vector3 position = transform.rotation *new Vector3(0.0f, 0.0f, -distance) + Target;
		transform.position = position;
	}

	
}
