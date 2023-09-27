using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
public class SplineLight : MonoBehaviour
{
    public float frequency = 1;
    public float amplitude = 1;
    public int steps = 100;
    public float xSpeed = 1;
    public float ySpeed = 1;
    public float distortionX, distortionY;

    Vectrosity.VectorLine line;

    void Start(){
        line = new VectorLine("Oscillator", new List<Vector3>(), 1, LineType.Continuous, Vectrosity.Joins.Weld);
        line.Draw3DAuto();
    }
    void Update(){
        Debug.Log("drawing");
        AnimateCurve();
    }
    public void AnimateCurve(){
        List<Vector3> positions = new List<Vector3>();

        for(int i = 0; i < steps; i++){
            float step = (float) (i+1) * frequency;
            float x = step * xSpeed; 
            float y = step * ySpeed; 
            Vector3 pos = new Vector3(Mathf.Sin(x), Mathf.Cos(y),0) * amplitude + transform.position;
            //add distortion
            positions.Add(pos);
        }

        line.points3 = positions;
    }
}
