using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ConvertMode{None, Linear, Segmented, Complete}
public class MeshToSpline : MonoBehaviour {

	public MeshFilter meshTarget;

	StellationController controller;
	List<Spline> splines;
	List<Point> points;
	ConvertMode mode;

	StellationManager manager;

	Dictionary<int, Point> indicePointMap;
	int count;

	public void SubmeshReadout(){
		if(meshTarget == null) return;

		for(int i = 0; i < meshTarget.sharedMesh.subMeshCount; i++){
			Debug.Log(meshTarget.sharedMesh.GetSubMesh(i).topology + " mesh");
		}
	}
	public void ConvertMesh(ConvertMode c){
		//whats the current stellation?
		//how do we add existing spline to stellation?
		//are we saving these to file or just to the scene?
		mode = c;
		indicePointMap = new Dictionary<int, Point>();
		GameObject g = new GameObject();
		g.transform.position = transform.position;

		controller = g.AddComponent<StellationController>();
		controller._splines = new List<Spline>();
		
		splines = new List<Spline>();
		points = new List<Point>();

		manager = GetComponentInParent<StellationManager>();

		if(manager != null){
			controller.transform.parent = manager.transform;
			//manager.controllers.Add(controller);
		}

		if(meshTarget != null){
			g.name = meshTarget.gameObject.name;
			CreatePoints(meshTarget.sharedMesh);

		}else{
			g.name = gameObject.name;
			foreach(MeshFilter r in GetComponentsInChildren<MeshFilter>()){
				
				CreatePoints(r.sharedMesh);
				count++;
			}
		}

		InterpretPointTypes();

	}

	void InterpretPointTypes(){
		// foreach(Point p in controller._points){
			
		// }
	}
	void CreatePoints (Mesh m) {
		
		Vector3[] vertices = m.vertices;
		Color[] colors = m.colors;

		//vertices are multiplied per face
		//do a positional check for dupes
		
		int pointCount = 0;
		foreach(Vector3 v in vertices){

			bool dupe = false;

			foreach(Point p in points){

				//need to make a new list of pointers for vertices
				//because the indices are trying to index into dupes instead of the combined one

				if(Vector3.SqrMagnitude(transform.TransformPoint(v) - p.Pos) <= Mathf.Epsilon){
					dupe = true;
					indicePointMap.Add(pointCount, p);
					break;
				}
			}

			if(dupe){
				pointCount ++;
				continue;
			}

			Point newPoint = SplineUtil.CreatePoint (transform.TransformPoint(v));
			newPoint.transform.parent = controller.transform;
			newPoint.name = pointCount.ToString();
			indicePointMap.Add(pointCount, newPoint);
			points.Add(newPoint);
			
			pointCount ++;
		}

		switch(mode){
			case ConvertMode.None:
			break;

			case ConvertMode.Linear:
				ConnectPoints();
			break;

			case ConvertMode.Segmented:
				ConnectSegments(m);
			break;

			case ConvertMode.Complete:
				ConnectAllPoints();
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

		controller._splines = splines;
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

		
		controller._splines = splines;
	}

	void ConnectSegments(Mesh m){
		//reinterpret mesh as a series of lines, assuming its quads?
		int submeshCount = m.subMeshCount;
		for(int i = 0; i < submeshCount; i++){

			UnityEngine.Rendering.SubMeshDescriptor sub = m.GetSubMesh(i);
			MeshTopology topo = sub.topology;			
			//ok we can work with this
			int[] indices = m.GetIndices(i);
			int numIndices = indices.Length;
			int stepSize = 3;
			if(topo == MeshTopology.Points) return;
			if(topo == MeshTopology.Lines) stepSize = 2;
			if(topo == MeshTopology.Quads) stepSize = 4;
			
			for(int index = 0; index < numIndices; index+=stepSize){
				
				//connect em up fellas

				for(int curIndex = 0; curIndex < stepSize-1; curIndex++){
					
					Point curPoint = null;
					Point nextPoint = null;
					indicePointMap.TryGetValue(indices[index + curIndex], out curPoint);
					indicePointMap.TryGetValue(indices[index + curIndex + 1], out nextPoint);

					if(!curPoint._neighbours.Contains(nextPoint)){
						SplinePointPair spp = SplineUtil.ConnectPoints(null, curPoint, nextPoint);
						spp.s.transform.parent = controller.transform;
						splines.Add(spp.s);
					}
				}

				//close faces 

				if(stepSize > 2){
					Point p1 = null;
					Point p2 = null;
					
					indicePointMap.TryGetValue(indices[index], out p1);
					indicePointMap.TryGetValue(indices[index + 3], out p2);

					if(!p1._neighbours.Contains(p2)){
						SplinePointPair spp = SplineUtil.ConnectPoints(null, p1, p2);
						spp.s.transform.parent = controller.transform;
						splines.Add(spp.s);
					}
				}
			}
			
		}
		
		controller._splines = splines;
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
