using UnityEngine;
using System.Collections;


public class Drawing : MonoBehaviour {

	static Material lineMaterial ;
	
	static void CreateLineMaterial() 
	{
		if( !lineMaterial ) 
		{
			lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
				"SubShader { Pass { " +
				"    Blend SrcAlpha OneMinusSrcAlpha " +
				"    ZWrite Off Cull Off Fog { Mode Off } " +
				"    BindChannels {" +
				"      Bind \"vertex\", vertex Bind \"color\", color }" +
				"} } }" );
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
 	void OnPostRender() 
	{
		foreach(Spline spline in Spline.Splines)
		{
			CreateLineMaterial();
			lineMaterial.SetPass( 0 );
			GL.Begin( GL.LINES );
			

				if(spline.isSelect)
					GL.Color( Color.white );
				else
					GL.Color( new Color(1,1,1,0.4f) );
				for(int i=0; i<spline.SplinePoints.Count-1;i++)
				{
					DrawLine(spline.SplinePoints[i].Pos,spline.SplinePoints[i+1].Pos);
				}
				if(spline.closed)
				{
					DrawLine(spline.SplinePoints[0].Pos,spline.SplinePoints[spline.SplinePoints.Count-1].Pos);
				}
			
			if(spline.isSelect)
				GL.Color( Color.red );
			else
				GL.Color( new Color(1,1,1,0.4f) );
//			for(int i=0; i<spline.LineSegmentPoints.Count-1;i++)
//			{
//				DrawLine(spline.LineSegmentPoints[i],spline.LineSegmentPoints[i+1]);
//			}
//			
			GL.End();
		}
	}
	
	void DrawLine(Vector3 begin,Vector3 end)
	{
		GL.Vertex3( begin.x, begin.y, begin.z );
		GL.Vertex3( end.x, end.y, end.z );
	}
}
