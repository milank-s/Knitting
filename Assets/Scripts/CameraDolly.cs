using UnityEngine;
using System.Collections;

public static class CameraDolly {


	public static float leftBound;
	public static float rightBound;
	public static float topBound;
	public static float bottomBound;

	// Calculate the frustum height at a given distance from the camera.
	public static float FrustumHeightAtDistance(Camera cam, float distance) {
		return 2.0f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
	}


	// Calculate the FOV needed to get a given frustum height at a given distance.
	public static float FOVForHeightAndDistance(float height, float distance) {
		return 2.0f * Mathf.Atan(height * 0.5f / distance) * Mathf.Rad2Deg;
	}

	// Start the dolly zoom effect.
	public static void StartDZ(Vector3 cameraPos, Vector3 targetPos) {
	}
		

	public static float SetDZ (Camera cam, Vector3 targetPos) {
		float initHeightAtDist;
		float distance = Vector3.Distance(cam.transform.position, targetPos);
		initHeightAtDist = FrustumHeightAtDistance(cam, distance);

		// Measure the new distance and readjust the FOV accordingly.
		float currDistance = Vector3.Distance(cam.transform.position, targetPos);
		return FOVForHeightAndDistance(initHeightAtDist, currDistance);
	}
}