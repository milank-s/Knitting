using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ConvertMode{None, Linear, Complete, Quads}
public class MeshToSpline : MonoBehaviour {

	public MeshFilter meshTarget;

	StellationController controller;
	List<Spline> splines;
	List<Point> points;
	ConvertMode mode;
	int count;

	
	public void ConvertMesh(ConvertMode c){
		//whats the current stellation?
		//how do we add existing spline to stellation?
		//are we saving these to file or just to the scene?
		mode = c;

		GameObject g = new GameObject();
		g.name = "Stellation" + count;

		controller = g.AddComponent<StellationController>();
		splines = new List<Spline>();
		points = new List<Point>();

		if(meshTarget != null){
			CreatePoints(meshTarget.sharedMesh);
		}else{

			foreach(MeshFilter r in GetComponentsInChildren<MeshFilter>()){
				
				CreatePoints(r.sharedMesh);
				count++;
			}
		}

		controller._splines = splines;
		StellationManager manager = GetComponentInParent<StellationManager>();

		if(manager != null){
			controller.transform.parent = manager.transform;
			manager.controllers.Add(controller);
		}
	}

	void CreatePoints (Mesh m) {
		
		Vector3[] vertices = m.vertices;
		Color[] colors = m.colors;

		int pointCount = 0;
		foreach(Vector3 v in vertices){
			Point newPoint = SplineUtil.CreatePoint (transform.TransformPoint(v));
			newPoint.transform.parent = controller.transform;
			newPoint.name = pointCount.ToString();
			pointCount ++;
			points.Add(newPoint);
		}

		switch(mode){
			case ConvertMode.None:
			break;

			case ConvertMode.Linear:
				ConnectPoints();
			break;

			case ConvertMode.Complete:
				ConnectAllPoints();
			break;

			case ConvertMode.Quads:
				ConnectQuads(m);
			break;
		}
	}

	void ConnectPoints(){
		Spline curSpline = null;

		for(int i = 0; i < points.Count-1; i++){

			SplinePointPair spp = SplineUtil.ConnectPoints(curSpline, points[i], points[i+1]);
			
			if(spp.s != curSpline){
				
				curSpline = spp.s;
				splines.Add(curSpline);
			}

			curSpline.transform.parent = controller.transform;
		}
	}
	

	void ConnectAllPoints(){
		List<Point> pointsOfPoints = points;
		
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

	void ConnectQuads(Mesh m){
		//reinterpret mesh as a series of lines, assuming its quads?
		int submeshCount = m.subMeshCount;
		for(int i = 0; i < submeshCount; i++){
			if(m.GetTopology(i) == MeshTopology.Quads){
				//ok we can work with this
				int[] indices = m.GetIndices(i);
				int numIndices = indices.Length;
				for(int index = 0; index < numIndices; index+=4){
					//connect em up fellas

					for(int curIndex = 0; curIndex < 3; curIndex++){
						Point curPoint = points[indices[index + curIndex]];
						Point nextPoint =  points[indices[index + curIndex+1]];
						if(!curPoint._neighbours.Contains(nextPoint)){
							SplinePointPair spp = SplineUtil.ConnectPoints(null, curPoint, nextPoint);
							spp.s.transform.parent = controller.transform;
							splines.Add(spp.s);
						}
					}

					Point p1 = points[indices[index]];
					Point p2 = points[indices[index + 3]];
					if(!points[index]._neighbours.Contains(points[index + 3])){
							SplinePointPair spp = SplineUtil.ConnectPoints(null, p1, p2);
							spp.s.transform.parent = controller.transform;
							splines.Add(spp.s);
						}
				}
			}
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
