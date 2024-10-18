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
    public float microNoiseScale;

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

    bool hit;
    bool overX = false;
    bool overY = false;
    InputAction joystickMovement;
    
    [Header("Constraints")]

    public float minFreq, maxFreq, minSteps, maxSteps, maxTimescale, minTimescale, minAmplitude, maxAmplitude, minScale, maxScale;
    Vectrosity.VectorLine line;
    Vectrosity.VectorLine shadow;
    Vector3 center;
    void Start(){
        offsets = new float[10];
        for(int i = 0; i < offsets.Length; i++){
            offsets[i] = Random.Range(-100f, 100f);
        }
        joystickMovement = Services.main.playerInput.currentActionMap.FindAction("Navigate");
        
        line = new VectorLine("Oscillator", new List<Vector3>(), 1, LineType.Continuous);
        line.layer = LayerMask.NameToLayer("UI");
        
    }

    
    public void OnNavigate(InputAction.CallbackContext context){

        microNoiseScale += 0.1f;
    }

    public void Update(){

        time += Time.deltaTime * timeScale;
        
        CheckInput();
        Lissajous();
        line.Draw3D();

    }

    public void Gauss(){
        microNoiseScale = 0.1f;
        noiseScale = Random.Range(0.1f, 0.25f);
        hit = true;
    }

    public void Initialize(){
        center = transform.position;

        normalX = 0.5f;
        normalY = 0.5f;

        Gauss();
        //play synth noise
        SynthController.instance.pads[synth].patch.NoteOn(note);

        if(line != null){
            line.rectTransform.gameObject.SetActive(true);
        }   
    }

    public void Disable(){
        if(line != null){
            line.rectTransform.gameObject.SetActive(false);
        }

        noise = Vector2.zero;
        normalX = 0;
        normalY = 0;
        SynthController.instance.pads[synth].Stop();
    }
    
    public void CheckInput()
    {
        Vector2 input = joystickMovement.ReadValue<Vector2>() * Time.deltaTime;

        SetXSpeed(input.x/2f);
        SetYSpeed(input.y/2f);

        normalX = 0.5f + (xSpeed/2f);
        normalY = 0.5f + (ySpeed/2f);

        steps = (int)Mathf.Lerp(maxSteps/20, maxSteps, Mathf.Pow(normalY, 3));
        
        xOverflow = Mathf.Clamp01(Mathf.Abs(xSpeed + input.x) - xMax) * Mathf.Sign(xSpeed);
        yOverflow = Mathf.Clamp01(Mathf.Abs(ySpeed + input.y) - yMax) * Mathf.Sign(ySpeed);

        
        overY = yOverflow != 0;
        overX = xOverflow != 0;

        amplitude = 0.4f + Mathf.Abs(xOverflow) + Mathf.Abs(yOverflow);

        if(input.magnitude == 0){
            hit = false;
        }

        frequency = 0.05f + normalX/1.25f;

        noiseFreqY = 3 + normalY * 80;
        noiseFreqX = 3 + (1 - normalX) * 80;

        if(overY || overX){

            SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kNoiseVolume, 1f);
        }else{
            SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kNoiseVolume, 0f);
        }

        timeScale = (maxSteps*5)/steps;
        noiseScale = Mathf.Lerp(noiseScale, 0, Time.deltaTime * 5);
        microNoiseScale = Mathf.Lerp(microNoiseScale, 0, Time.deltaTime * 5);

        pitch += input.x;
        pitch = Mathf.Clamp01(pitch);

        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kSubVolume, (1-normalX));
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kCrossMod, normalX);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc1Transpose, normalX/1.5f+ 0.25f);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc1Tune, Mathf.Abs(xOverflow));

        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOscFeedbackAmount, normalX/2f);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc1Volume, normalX + 0.5f);

        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc2Volume, normalY + 0.5f);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc2Transpose, normalY/1.5f + 0.25f);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc2UnisonVoices, steps/maxSteps);
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kOsc2Tune, Mathf.Abs(yOverflow));

        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kDistortionDrive, noiseScale);
        
        SynthController.instance.pads[synth].patch.SetParameterPercent(AudioHelm.Param.kStutterFrequency, 1 - normalY);
    }

    public void SetXSpeed(float f){
        xSpeed += f;

        float overflow = Mathf.Clamp01(Mathf.Abs(xSpeed + f) - xMax) * Mathf.Sign(xSpeed);
        if(!hit && overflow != 0){
            Gauss();
        }
        microNoiseScale += Mathf.Abs(f)/2f;

        xSpeed = Mathf.Clamp(xSpeed, -xMax, xMax);
    }
    
    public void SetYSpeed(float f){
        ySpeed += f;
        
        float overflow = Mathf.Clamp01(Mathf.Abs(ySpeed + f) - yMax) * Mathf.Sign(ySpeed);
        if(!hit && overflow != 0){
            Gauss();
            hit = true;
        }
        microNoiseScale += Mathf.Abs(f)/2f;

        ySpeed = Mathf.Clamp(ySpeed, -yMax, yMax);
    }

    public void ClampSteps(float f){
        steps = (int)Mathf.Clamp(steps + f, 50, maxSteps);
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

            Vector2 microNoise = Vector2.zero;
            
            microNoise.x = Mathf.PerlinNoise(pos.y * 25 + Time.time * -20, pos.x * 25 + Time.time * 20) * microNoiseScale;
            microNoise.y = Mathf.PerlinNoise(pos.x * 56 - Time.time * 5, pos.y * 33+ Time.time * 13 ) * microNoiseScale;
            
            
            Vector2 noise = Vector2.zero;
            noise.x = Mathf.Sin(pos.y * noiseFreqX + Time.time * 30.45f) * noiseScale;
            noise.y =  Mathf.Sin(pos.x * noiseFreqY + Time.time * -20.8f) * noiseScale;


            pos += (Vector3)microNoise + (Vector3)noise;
            
            pos += center;
            positions.Add(pos);
        }

        line.points3 = positions;

    }
}
