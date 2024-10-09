using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using UnityEngine.InputSystem;
public class Oscilloscope : MonoBehaviour
{
    
    
    [Header("Screensize")]
    public float xScale = 1.25f;
    public float yScale = 1;

    [Header("Start Values")]
    public float frequency = 1;
    public float amplitude = 1;
    public float scale = 1.01f;
    public float timeScale = 1;
    public int steps = 100;
    public float xSpeed = 1;
    public float ySpeed = 1;
    public float noiseScale;
    public float noiseFreqX, noiseFreqY;

    float[] offsets;

    InputAction joystickMovement;
    
    [Header("Constraints")]

    public float minFreq, maxFreq, minSteps, maxSteps, maxTimescale, minTimescale, minAmplitude, maxAmplitude, minScale, maxScale;
    Vectrosity.VectorLine line;
    Vector3 center;
    void Start(){
        offsets = new float[10];
        for(int i = 0; i < offsets.Length; i++){
            offsets[i] = Random.Range(-100f, 100f);
        }
        joystickMovement = Services.main.playerInput.currentActionMap.FindAction("Navigate");
    }

    
    public void Update(){
        CheckInput();
        AttractMode();
        AnimateCurve();
        line.Draw3D();
    }


    public void CheckInput()
    {
        Vector2 input = joystickMovement.ReadValue<Vector2>() * Time.deltaTime;

        xSpeed += input.x;
        ySpeed += input.y;
    }


    public void AttractMode(){
        //perlinNoise animate values;
        scale = 1 + Mathf.PerlinNoise(offsets[0] + Time.time, offsets[1] -Time.time);
        frequency = 1 + Mathf.PerlinNoise(offsets[2]Time.time, offsets[3]-Time.time);
        amplitude = 1 + Mathf.PerlinNoise(offsets[5]Time.time, offsets[4]-Time.time);
    }

    public void OnEnable(){
        center = transform.position;
        line = new VectorLine("Oscillator", new List<Vector3>(), 2, LineType.Continuous);
        line.layer = LayerMask.NameToLayer("Oscilloscope");
    }

    public void OnDisable(){
        if(line != null){
            VectorLine.Destroy(ref line);
        }
    }
    public void SetXSpeed(float f){
        xSpeed += f;
    }
    
    public void SetYSpeed(float f){
        ySpeed += f;
    }

    public void SetAmplitude(float f){
        
        amplitude = Mathf.Clamp(f + amplitude, minAmplitude, maxAmplitude);
    }

    public void SetFreq(float f){
        frequency =  Mathf.Clamp(frequency + f, minFreq, maxFreq);
    }

    public void SetTimescale(float f){
        timeScale = Mathf.Clamp(f + timeScale, minTimescale, maxTimescale);
    }

    public void SetScale(float f){
        scale = Mathf.Clamp(f + scale, minScale, maxScale);
    }

    public void SetNoiseScale(float f){
        scale = f;
    }

    public void SetNoiseFreqY(float f){
        noiseFreqX += f;
    }

     public void SetNoiseFreqX(float f){
        noiseFreqY += f;
    }

    
    public void ResetXSpeed(){
        xSpeed =1;
    }

    public void ResetYSpeed(){
        ySpeed = 1;
    }

    public void ResetScale(){
        scale = 1;
    }

    public void ResetFreq(){
        frequency = 0.1f;
    }

    public void AnimateCurve(){
        List<Vector3> positions = new List<Vector3>();
        float scaleCoefficient = 1;
        Vector3 pos = Vector3.zero;
        Vector3 oldPos;

        for(int i = 0; i < steps; i++){
            scaleCoefficient *= scale;
            //shits too small, doesnt matter anymore
            if(scaleCoefficient < 0.01f) break;

            float time = Time.time * timeScale;
            float step = time + (float) (i+1) * frequency;
            float x = step * xSpeed; 
            float y = step * ySpeed; 

            //if outside bounds continue
            oldPos = pos;

            pos = new Vector3(Mathf.Sin(x)* xScale, Mathf.Cos(y)* yScale,0) * amplitude * scaleCoefficient;
            pos.x += Mathf.Sin(pos.y * noiseFreqX + time) * noiseScale;
            pos.y += Mathf.Sin(pos.x * noiseFreqY + time) * noiseScale;
            
            //if(Mathf.Abs(pos.x) > xBounds || Mathf.Abs(pos.y) > yBounds) continue;
            
            pos += center;
            positions.Add(pos);
        }

        line.points3 = positions;
    }
}
