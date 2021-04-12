using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using AudioHelm;
public class AudioManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static AudioManager instance;
    
    public AudioHelmClock clock;
    
    public AudioMixer SynthMaster;
    
    public SampleSequencer drumSeqeuncer;

    public Sampler clarinetSampler;
    public Sampler pianoSampler;
    [SerializeField] public SplineSinger splineSinger;
    
    void Awake(){
        instance = this;
    }
    void Start()
    {
        clock.pause = true;
        
        Services.PlayerBehaviour.OnPointEnter += EnterPoint;
        Services.PlayerBehaviour.OnStartFlying += EnterFlying;
        Services.PlayerBehaviour.OnStartTraversing += EnterTraversing;
        Services.PlayerBehaviour.OnStoppedFlying += ExitFlying;
        Services.PlayerBehaviour.OnTraversing += OnTraversing;
        Services.PlayerBehaviour.OnFlying += OnFlying;
        Services.PlayerBehaviour.OnStoppedTraversing += ExitTraversing;
    }

    //calculate position for pitch then play on sampler?
    //or just play based on a sequence

    public void EnterPoint(){
        //clarinetSampler.NoteOn(64);
        clock.pause = true;
    }

    public void EnterFlying(){
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
        
        clock.pause = true;
        SynthController.instance.StopMovementSynth();
    }

    public void OnTraversing(){

        SynthController.instance.UpdateMovementSynth();
        clock.bpm = Services.PlayerBehaviour.curSpeed * 30 + 50 * (1-Services.PlayerBehaviour.decelerationTimer);
        Debug.Log(clock.bpm);
    }

    public void OnFlying(){

    }

    public void Reset(){
        SynthController.instance.ResetSynths();   
        SynthMaster.SetFloat("Volume", 0f);
        clock.pause = true;
    }

    public void HandlePlayerReset(PlayerState state){
        //do what we need to do
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
