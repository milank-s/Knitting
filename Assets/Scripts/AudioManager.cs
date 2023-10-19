using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using AudioHelm;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
        
    public AudioSource audioRecordings;
    public AudioHelmClock clock;
    public AudioMixer SynthMaster;

    public Sampler celloAttack;
    public Sampler celloSustain;
    
    [SerializeField] public SplineSinger splineSinger;
    
    void Awake(){
        instance = this;
    }
    void Start()
    {
        clock.pause = true;
        
        // Services.main.OnPointEnter += EnterPoint;
       // Services.main.OnPlayerEnterPoint += EnterPoint;
        Services.main.OnPlayerEnterPoint += EnterPoint;
        Services.main.OnLoadLevel += SoundSetup;
        Services.PlayerBehaviour.OnStartFlying += EnterFlying;
        Services.PlayerBehaviour.OnStartTraversing += EnterTraversing;
        Services.PlayerBehaviour.OnEnterSpline += EnterSpline;
        Services.PlayerBehaviour.OnExitSpline += ExitSpline;
        Services.PlayerBehaviour.OnStoppedFlying += ExitFlying;
        Services.PlayerBehaviour.OnTraversing += OnTraversing;
        Services.PlayerBehaviour.OnFlying += OnFlying;
        Services.PlayerBehaviour.OnStoppedTraversing += ExitTraversing;
        Services.PlayerBehaviour.OnEnterAnyPoint += EnterAnyPoint;
        Services.PlayerBehaviour.OnLeaveAnyPoint += LeaveAnyPoint;
        Services.main.OnReset += Reset;
    }
    

    public void PlayLevelSounds(){
        if(SceneController.instance.curLevelSet.audio.Length > 0 && SceneController.instance.curLevelSet.audio.Length < SceneController.curLevel){
            audioRecordings.clip = SceneController.instance.curLevelSet.audio[SceneController.curLevel];
            audioRecordings.Play();
        }
    }
    public void SoundSetup(StellationController stellation){

      
        //iterate through splines
        //iterate through points
        //use bounds to determine pitch
        //quantize it or whatever
    }
    public void EnterPoint(Point p){
        
        clock.pause = true;
        SynthController.instance.PlayNoteOnPoint(p);
    }

    public void EnterSpline(){    
        SynthController.instance.PlaySplineChord();
    }

    
    public void ExitSpline(){
        
        SynthController.instance.StopSplineChord();
    }

    public void EnterFlying(){
        
        SynthController.instance.PlayFlyingSynth();
    }

    public void EnterTraversing(){  
        SynthController.instance.StartTraversing();
        clock.pause = false;
    }

    public void ExitFlying(){
        SynthController.instance.StopFlying();
    }

    public void ExitTraversing(){
        SynthController.instance.StopTraversing();
        clock.pause = true;
    }

    public void EnterAnyPoint(){

    }

    public void LeaveAnyPoint(){
        
    }

    public void OnTraversing(){

        SynthController.instance.MovementSynth();
        //clock.bpm = Services.PlayerBehaviour.curSpeed * 30 + 50 * (1-Services.PlayerBehaviour.decelerationTimer);
    }

    public void OnFlying(){
        SynthController.instance.UpdateFlyingSynth();
    }

    public void Reset(){
        SynthController.instance.ResetSynths();   
        SynthMaster.SetFloat("Volume", 0f);
        audioRecordings.Stop();
        clock.pause = true;
    }

    public void Pause(bool pause){
        
        if(pause) audioRecordings.Pause();
        if(!pause) audioRecordings.Play();

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
