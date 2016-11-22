using UnityEngine;
using System.Collections;

public class Edge : MonoBehaviour {

	public BezierCurve curve;
	public int fidelity = 10;

	private Node[] _edgeVertices;
	private LineRenderer l;
	private float steps = 5;

	void Awake () {
		l = GetComponent<LineRenderer> ();
		curve = GetComponent<BezierCurve> ();
	}

	void Decorate (){
		l.SetVertexCount (fidelity);
		l.SetPosition (0, curve.GetPoint(0));

		for (int i = 1; i < fidelity; i++) {
			l.SetPosition (i, curve.GetPoint((float)i/(fidelity-1)));
		}
	}

	public Node[] EndingVertices(){
		return _edgeVertices;
	}

	public Node GetVert1(){
		return _edgeVertices[0];
	}
	public Node GetVert2(){
		return _edgeVertices[1];
	}

	public void SetVerts(Node from, Node to){

		_edgeVertices = new Node[]{from, to};
		curve.CreateCurve (_edgeVertices[0].transform, _edgeVertices[1].transform);
		Decorate ();
	}

	public BezierCurve Getcurve(){
		return curve;
	}

}
