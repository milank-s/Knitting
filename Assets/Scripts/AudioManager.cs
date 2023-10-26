using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using AudioHelm;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
        
    public SynthController helmAudio;
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
        Services.PlayerBehaviour.OnExitPoint += ExitPoint;
        Services.PlayerBehaviour.OnEnterSpline += EnterSpline;
        Services.PlayerBehaviour.OnExitSpline += ExitSpline;
        Services.PlayerBehaviour.OnStoppedFlying += ExitFlying;
        Services.PlayerBehaviour.OnTraversing += OnTraversing;
        Services.PlayerBehaviour.OnFlying += OnFlying;
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
        helmAudio.PlayNoteOnPoint(p);
        helmAudio.EnterPoint();
    }

     public void ExitPoint(){  
        helmAudio.ExitPoint();
        clock.pause = false;
    }

    //only happens if player switches splines
    public void EnterSpline(){    
    }

    //happens if player switches splines or flies away
    public void ExitSpline(){
        
        helmAudio.StopSplineChord();
    }

    public void EnterFlying(){
        
        helmAudio.StartFlying();
    }

    public void ExitFlying(){
        helmAudio.StopFlying();
    }

    public void OnTraversing(){

        SynthController.instance.MovementSynth();
        SynthController.frequency = Services.PlayerBehaviour.curSpeed/Services.PlayerBehaviour.maxSpeed;
        clock.bpm = Mathf.Lerp(80, 300, SynthController.frequency);
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
