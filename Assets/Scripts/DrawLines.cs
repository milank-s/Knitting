using UnityEngine;
using System.Collections;

public class DrawLines : MonoBehaviour {

	private Material mat;

	void CreateLineMaterial() 
	{
		if (!mat)
		{
			
			var shader = Shader.Find("Hidden/Internal-Colored");
			mat = new Material(shader);
			mat.hideFlags = HideFlags.HideAndDontSave;
			// Set blend mode to invert destination colors.
			mat.SetInt("_SrcAlpha", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//			mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
			// Turn off backface culling, depth writes, depth test.
			mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
//			mat.SetInt("_ZWrite", 0);
			mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
		}

	}

	void Start(){
		CreateLineMaterial ();
	}

//	void OnPostRender() 
//	{
//		foreach(Spline spline in Spline.Splines)
//		{
//
//
//
//			spline.line.Draw3DAuto ();
//
//			for(int i=0; i<spline.SplinePoints.Count-1;i++)
//			{
//				DrawLine(spline.SplinePoints[i].Pos,spline.SplinePoints[i+1].Pos);
//			}
//			if(spline.closed)
//			{
//				DrawLine(spline.SplinePoints[0].Pos,spline.SplinePoints[spline.SplinePoints.Count-1].Pos);
//			}
//
//			if(spline.isSelect)
//				GL.Color( Color.red );
//			else
//
//			for(int i=0; i<spline.LineSegmentPoints.Count-1;i++){
//				DrawLine(spline.LineSegmentPoints[i],spline.LineSegmentPoints[i+1], i);
//			}
//						
//			GL.End();
//		}
//	}

	void DrawLine(Vector3 begin, Vector3 end, int l)
	{

		GL.Color(GetColor(l));
		GL.Vertex3( begin.x, begin.y, begin.z );

		GL.Color(GetColor(l + 1));
		GL.Vertex3( end.x, end.y, end.z);
	}

	Color GetColor(int i){
		float t = 1;// 0.5f + (Mathf.Sin (i/2)/2);
		return new Color (t, t, t);
	}
}
