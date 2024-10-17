using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
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
    public float xMax = 0.99f;
    public float yMax = 0.98f;
    public Vector2 noise;
    public float noiseScale;

    int note = 40;
    public float noiseFreqX, noiseFreqY;

    float time;
    float noiseTimer;
    float stepAmount;
    float[] offsets;
    float xOverflow = 0;
    float yOverflow = 0;
    int synth = 5;
    float pitch = 1;

    public float normalX;
    public float normalY;

    bool overX = false;
    bool overY = false;
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

    
    public void OnNavigate(InputAction.CallbackContext context){

        Gauss();
    }

    public void Update(){

        time += Time.deltaTime * timeScale;
        
        CheckInput();
        Lissajous();
        line.Draw3D();

    }

    public void Gauss(){
        noiseScale = Random.Range(0.33f, 2f);
        noiseFreqX = Random.Range(1f, 6f);
        noiseFreqY = Random.Range(1f, 6f);
    }

    public void CheckInput()
    {
        Vector2 input = joystickMovement.ReadValue<Vector2>() * Time.deltaTime;

        SetXSpeed(input.x/2f);
        SetYSpeed(input.y/2f);


        normalX = 0.5f + (xSpeed/2f);
        normalY = 0.5f + (ySpeed/2f);

        steps = (int)Mathf.Lerp(20, maxSteps, Mathf.Pow(normalY, 3));
        SetFreq(input.x/10f);
        
        xOverflow = Mathf.Clamp01(Mathf.Abs(xSpeed + input.x) - xMax) * Mathf.Sign(xSpeed);
        yOverflow = Mathf.Clamp01(Mathf.Abs(ySpeed + input.y) - yMax) * Mathf.Sign(ySpeed);

        noiseScale += xOverflow + yOverflow;
        
        overY = yOverflow != 0;
        overX = xOverflow != 0;

        if(overY || overX){

            noiseFreqX = 10;
            noiseFreqY = 8;
            SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kNoiseVolume, 1f);
        }else{
            
            SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kNoiseVolume, 0f);
        }

        timeScale = 1500f/steps;
        noiseScale = Mathf.Lerp(noiseScale, 0, Time.deltaTime * 5);

        pitch += input.x;
        pitch = Mathf.Clamp01(pitch);

        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kSubVolume, (1-normalX));
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kCrossMod, normalX);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc1Transpose, normalX);

        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOscFeedbackAmount, normalX/2f);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc1Volume, normalX + 0.5f);

        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc2Volume, normalY + 0.5f);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc2Transpose, normalY);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc2UnisonVoices, steps/maxSteps);

        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kDistortionDrive, noiseScale);
        
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kStutterFrequency, 1 - normalY);
    }

    public void OnEnable(){
        center = transform.position;
        line = new VectorLine("Oscillator", new List<Vector3>(), 1, LineType.Continuous);
        line.layer = LayerMask.NameToLayer("UI");
        
        normalX = 0.5f;
        normalY = 0.5f;

        Gauss();
        //play synth noise
        SynthController.instance.pads[synth].patch.NoteOn(note);
        
    }

    public void OnDisable(){
        if(line != null){
            VectorLine.Destroy(ref line);
        }
        noise = Vector2.zero;
        normalX = 0;
        normalY = 0;
        SynthController.instance.pads[synth].Stop();
        
        //disable synth noise
    }
    public void SetXSpeed(float f){
        xSpeed += f;
        xSpeed = Mathf.Clamp(xSpeed, -xMax, xMax);
    }
    
    public void SetYSpeed(float f){
        ySpeed += f;
        ySpeed = Mathf.Clamp(ySpeed, -yMax, yMax);
    }

    public void ClampSteps(float f){
        steps = (int)Mathf.Clamp(steps + f, 20, maxSteps);
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
        noiseScale = f;
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

    public void Lissajous(){
        List<Vector3> positions = new List<Vector3>();
        float scaleCoefficient = 1;
        Vector3 pos = Vector3.zero;
        Vector3 oldPos;

        for(int i = 0; i < steps; i++){
            scaleCoefficient *= scale;
            //shits too small, doesnt matter anymore
            if(scaleCoefficient < 0.01f) break;
        
            float step = time + (float) (i+1) * frequency;
            float x = step * xSpeed; 
            float y = step * ySpeed; 

            //if outside bounds continue
            oldPos = pos;

            pos = new Vector3(Mathf.Sin(x)* xScale, Mathf.Cos(y)* yScale,0) * amplitude * scaleCoefficient;
            noise.x = Mathf.Sin(pos.x * noiseFreqX + Time.time) * noiseScale;
            noise.y = Mathf.Sin(pos.y * noiseFreqY + Time.time) * noiseScale;
            pos += (Vector3)noise;
            //if(Mathf.Abs(pos.x) > xBounds || Mathf.Abs(pos.y) > yBounds) continue;
            
            pos += center;
            positions.Add(pos);
        }

        line.points3 = positions;
    }

    public void Spirograph(){

    }
}
