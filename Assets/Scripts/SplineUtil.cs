using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineUtil : MonoBehaviour {

	static public Point CreatePoint(Vector3 pos){
		GameObject p = Instantiate(Resources.Load("Prefabs/Point")) as GameObject;
		Point newPoint = p.GetComponent<Point>();

		newPoint.transform.position = pos;
		newPoint.GetComponent<Collider> ().enabled = true;

		#if !UNITY_EDITOR
	newPoint.Initialize();
		#endif
		
		return newPoint;
	}

	static public SplinePointPair ConnectPoints(Spline s, Point p1, Point p2){

		SplinePointPair result = new SplinePointPair();

		Spline newSpline;

		if (p1 == null || p2 == null) {
			return null;

		}else if (p2 == p1) {
			result.p = p2;
			result.s = s;
			return result;
		}

		//ALL CASES WHERE THE CLICKED ON/CREATED POINTS ARE ADDED TO CURRENT SPLINE
		if (s == null || s.SplinePoints.Count > 100 ||  s.closed) {
				newSpline = CreateSpline (p1, p2);

			} else {

				if (p1 == s.StartPoint || p1 == s.EndPoint) {

					newSpline = s;

					if (p2 == s.StartPoint || p2 == s.EndPoint) {

						s.closed = true;

						p1.AddPoint (p2);
						p2.AddPoint (p1);

						if (s.GetPointIndex (p2) - s.GetPointIndex (p1) > 1) {
							s.Selected = p2;
						}

					} else if (!s.SplinePoints.Contains (p2)) {

						s.AddPoint (p1, p2);
						s.name = s.StartPoint.name + "—" + s.EndPoint.name;

					} else {

						newSpline = CreateSpline (p1, p2);
					}
				} else {

					newSpline = CreateSpline (p1, p2);
				}
				//EDGE CASE
				//Creating endpoint when you're on startpoint
				//make it so that the start/midpoint get shifted down one index, insert at startpoin
			}
		// ??? AHAHAHAHAHA

		result.p = p2;
		result.s = newSpline;

		return result;
	}


	static public Point RaycastDownToPoint(Vector3 pos, float distance, float zOffset){
		Ray ray = new Ray (pos  - (Vector3.forward * zOffset), Vector3.forward);
		//		Debug.DrawRay (ray.origin, ray.origin + ray.direction * 10);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, distance, LayerMask.GetMask("Points"))) {
			if (hit.collider.tag == "Point") {
				Point hitPoint = hit.collider.GetComponent<Point> ();

				return hitPoint;

			}
		}
		return null;
	}

	static public Point RaycastFromCamera(Vector3 pos, float distance){
		//		Ray ray = new Ray (pos + -(Vector3.forward) * 100, Vector3.forward);
		Ray ray = Camera.main.ScreenPointToRay (Camera.main.WorldToScreenPoint (pos));
		//ray.origin = ray.GetPoint(Vector3.Distance(pos, Services.mainCam.transform.position) - 1);
		Debug.DrawRay(pos, ray.direction);
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit, distance, LayerMask.GetMask("Points"))) {
			if (hit.collider.tag == "Point") {
				Point hitPoint = hit.collider.GetComponent<Point> ();

				return hitPoint;

			}
		}
		return null;
	}

		static public Spline CreateSpline (Point firstP, Point nextP){

		GameObject newSpline = (GameObject)Instantiate (Resources.Load<GameObject>("Prefabs/Spline"), Vector3.zero, Quaternion.identity);

		Spline s = newSpline.GetComponent<Spline> ();

		s.name = firstP.name + "—" + nextP.name;
		s.Selected = firstP;

		//		if (lastPoint != curPoint) {
		//			s.AddPoint (lastPoint);
		//		}

		s.AddPoint (null, firstP);
		s.AddPoint (null, nextP);

		//		s.GetComponentInChildren<SpriteRenderer> ().sprite = Services.Prefabs.Symbols [UnityEngine.Random.Range (0, Services.Prefabs.Symbols.Length)];
		//		s.GetComponentInChildren<TextMesh> ().text = Spline.Splines.Count.ToString ();

		s.transform.position = Vector3.Lerp (firstP.Pos, nextP.Pos, 0.5f);

		return s;
	}
}
