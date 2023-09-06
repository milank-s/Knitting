using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshToSpline : MonoBehaviour {

	Mesh mesh;
	Spline curSpline;
	Point curPoint;
	public bool connectAll = false;
	public MeshFilter meshTarget;
	StellationController controller;
	List<Spline> splines;

	int count;

	
	public void ConvertMesh(){
		//whats the current stellation?
		//how do we add existing spline to stellation?
		//are we saving these to file or just to the scene?
		
		GameObject g = new GameObject();
		g.name = "Stellation" + count;

		controller = g.AddComponent<StellationController>();
		splines = new List<Spline>();

		if(meshTarget != null){
			CreateSpline(meshTarget.sharedMesh);
		}else{

			foreach(MeshFilter r in GetComponentsInChildren<MeshFilter>()){
				
				CreateSpline(r.sharedMesh);
				count++;
			}
		}

		controller._splines= splines;
		StellationManager manager = GetComponentInParent<StellationManager>();

		if(manager != null){
			controller.transform.parent = manager.transform;
			manager.controllers.Add(controller);
		}
	}

	void CreateSpline (Mesh m) {
		mesh = m;
		Vector3[] vertices = mesh.vertices;
		Color[] colors = mesh.colors;

		Point firstPoint = SplineUtil.CreatePoint (transform.TransformPoint(vertices [0]));
		Point secondPoint = SplineUtil.CreatePoint (transform.TransformPoint(vertices [1]));

		curPoint = secondPoint;
		curSpline = SplineUtil.CreateSpline (firstPoint, secondPoint);
		splines.Add(curSpline);
		firstPoint.transform.parent = curSpline.transform;
		secondPoint.transform.parent = curSpline.transform;
		firstPoint.name = "0";
		secondPoint.name = "1";

		for(int i = 2; i < vertices.Length; i++) {
			// Point nextPoint = SplineUtil.RaycastDownToPoint(transform.TransformPoint(vertices [i]), raycastDist, raycastHeight);
			// if (nextPoint == null) {
			Point newPoint = SplineUtil.CreatePoint (transform.TransformPoint (vertices [i]));
			newPoint.transform.parent = curSpline.transform;
			newPoint.name = i.ToString();

			SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, curPoint, newPoint);
			
			curSpline.transform.parent = controller.transform;

			if(spp.s != curSpline) splines.Add(curSpline);
			
			curSpline = spp.s;
			curPoint = spp.p;
		}

		List<Point> points = curSpline.SplinePoints;
		List<Point> pointsOfPoints = curSpline.SplinePoints;
		
		if(connectAll){
			for(int i = 0; i < points.Count; i++){
				foreach(Point p in pointsOfPoints){
					if(p != points[i] && !points[i]._neighbours.Contains(p)){
						SplinePointPair spp = SplineUtil.ConnectPoints(null, points[i], p);
						spp.s.transform.parent = controller.transform;
						splines.Add(spp.s);
					}
				}

				pointsOfPoints.Remove(points[i]);
			}
		}

			//how are you encoding point type in the mesh data?

			// Color c = colors[i];

			// if(c.r > 100){
			// 	if(c.g > 100){
			// 		if(c.b > 100){
			// 			//white = end
			// 			newPoint.SetPointType(PointTypes.end);
			// 		}else{
			// 			//yellow = start
			// 			newPoint.SetPointType(PointTypes.normal);
			// 		}
			// 	}else{
					
			// 		//red == stop
			// 		newPoint.SetPointType(PointTypes.stop);
			// 	}

			// }else if(c.g > 100){
			// 	//green = fly
			// 	newPoint.SetPointType(PointTypes.fly);
			// }else if (c.b > 100){
			// 	newPoint.SetPointType(PointTypes.normal);
			// }else{
			// 	newPoint.SetPointType(PointTypes.ghost);
			// }

			// }

		
		
	}

}
