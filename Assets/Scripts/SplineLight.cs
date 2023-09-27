using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
public class Oscilloscope : MonoBehaviour
{
    public float frequency = 1;
    public float amplitude = 1;
    public int steps = 100;
    public float xSpeed = 1;
    public float ySpeed = 1;
    public float distortionX, distortionY;

    Vectrosity.VectorLine line;
    
    public float angle;

    void Start(){
        line = new VectorLine("Oscillator", new List<Vector3>(), 1);
        line.Draw3DAuto();
    }
    void Update(){
        AnimateCurve();
    }
    public void AnimateCurve(){
        List<Vector3> positions = new List<Vector3>();

        for(int i = 0; i < steps; i++){
            float newAngle = (float) (i+1) * frequency; 
            Vector3 pos = new Vector3(Mathf.Sin(newAngle), Mathf.Cos(newAngle),0) + transform.position;
            //add distortion
            positions.Add(pos);
        }

        line.points3 = positions;
    }
}
