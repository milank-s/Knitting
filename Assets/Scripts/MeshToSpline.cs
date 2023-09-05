using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshToSpline : MonoBehaviour {

	Mesh mesh;
	Spline curSpline;
	Point curPoint;
	Vector3[] vertices;
	Color[] colors;

	StellationController controller;
	List<Spline> splines;

	public void ConvertMesh(){
		//whats the current stellation?
		//how do we add existing spline to stellation?
		//are we saving these to file or just to the scene?
		
		int i = 0;

		foreach(MeshFilter r in GetComponentsInChildren<MeshFilter>()){
			GameObject g = new GameObject();
			g.name = "Stellation" + i;

			controller = g.AddComponent<StellationController>();
			splines = new List<Spline>();
			
			CreateSpline(r.sharedMesh);
			
			StellationManager m = g.GetComponentInParent<StellationManager>();

			if(m != null){
				g.transform.parent = m.transform;
				m.controllers.Add(controller);
			}

			i++;
		}
	}

	void CreateSpline (Mesh m) {
		mesh = m;
		vertices = mesh.vertices;
		colors = mesh.colors;

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

			SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, curPoint, newPoint);
			
			curSpline.transform.parent = controller.transform;

			if(spp.s != curSpline) splines.Add(curSpline);
			
			curSpline = spp.s;
			curPoint = spp.p;
		}
		
	}

}
