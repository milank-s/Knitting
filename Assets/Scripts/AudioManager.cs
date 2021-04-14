using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using AudioHelm;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    public AudioHelmClock clock;
    
    public AudioMixer SynthMaster;
    
    public Sampler drumSampler;

    public Sampler clarinetSampler;
    public Sampler pianoSampler;
    [SerializeField] public SplineSinger splineSinger;
    
    void Awake(){
        instance = this;
    }
    void Start()
    {
        clock.pause = true;
        

        Services.main.OnPointEnter += EnterPoint;
        Services.main.OnLoadLevel += SoundSetup;
        Services.PlayerBehaviour.OnStartFlying += EnterFlying;
        Services.PlayerBehaviour.OnStartTraversing += EnterTraversing;
        Services.PlayerBehaviour.OnStoppedFlying += ExitFlying;
        Services.PlayerBehaviour.OnTraversing += OnTraversing;
        Services.PlayerBehaviour.OnFlying += OnFlying;
        Services.PlayerBehaviour.OnStoppedTraversing += ExitTraversing;
    }
    
    public void SoundSetup(StellationController stellation){

        foreach(Spline s in stellation._splines){
            //
        }
        //iterate through splines
        //iterate through points
        //use bounds to determine pitch
        //quantize it or whatever

    }
    public void EnterPoint(Point p){
        //clarinetSampler.NoteOn(64);
        clock.pause = true;
        SynthController.instance.PlayNoteOnPoint(p);
    }

    public void EnterFlying(){
        SynthController.instance.StopMovementSynth();
        SynthController.instance.PlayFlyingSynth();
    }

    public void EnterTraversing(){
        clock.pause = false;
        SynthController.instance.PlayMovementSynth();
    }

    public void ExitFlying(){
        SynthController.instance.StopFlying();
    }

    public void ExitTraversing(){
        SynthController.instance.StopMovementSynth();
        clock.pause = true;
    }

    public void OnTraversing(){

        SynthController.instance.UpdateMovementSynth();
        //clock.bpm = Services.PlayerBehaviour.curSpeed * 30 + 50 * (1-Services.PlayerBehaviour.decelerationTimer);
    }

    public void OnFlying(){
        SynthController.instance.UpdateFlyingSynth();
    }

    public void Reset(){
        SynthController.instance.ResetSynths();   
        SynthMaster.SetFloat("Volume", 0f);
        clock.pause = true;
    }

    public void Pause(bool pause){
        
        clock.pause = pause;

        if(pause){
            SynthMaster.SetFloat("Volume", -80f);
        }else{
            SynthMaster.SetFloat("Volume", 0f);
        }
    }
    public void FlyingSound(){
        
		GranularSynth.flying.TurnOn();
    }

}
