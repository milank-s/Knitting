using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using AudioHelm;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
        
    public static float loudness;
    public SynthController helmAudio;
    public AudioSource audioRecordings;
    public AudioHelmClock clock;
    public AudioMixer synthMaster;
    public AudioMixer master;

    public Sampler celloAttack;
    public Sampler celloSustain;
    
    [SerializeField] public SplineSinger splineSinger;
    
    void Awake(){
        instance = this;
    }
    void Start()
    {
        Reset();
        
        audioSamples = new float[sampleDataLength];
        updateTime = 0f;
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
    public int sampleDataLength = 1024; // Number of audio samples to analyze
    public float updateStep = 0.1f;     // Time in seconds between loudness updates

    private float[] audioSamples;
    private float currentLoudness;
    private float updateTime;


    // Method to calculate the loudness from AudioListener
    float GetLoudnessFromAudioListener()
    {
        AudioListener.GetOutputData(audioSamples, 0); // Get the raw audio data from the listener (0 for left channel)
        float sum = 0f;

        // Calculate RMS (Root Mean Square) for the audio samples
        foreach (var sample in audioSamples)
        {
            sum += sample * sample;
        }

        float rmsValue = Mathf.Sqrt(sum / sampleDataLength);
        return rmsValue; // The loudness in a 0 to 1 range
    }

    void Update(){
        updateTime += Time.deltaTime;
        if (updateTime >= updateStep)
        {
            updateTime = 0f;
            loudness = GetLoudnessFromAudioListener();
        }
    }

    public void SetVolume(float v){
        float volume = Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f;
        
        master.SetFloat("Volume", volume);   
        
        Debug.Log("volume set to " + volume);
    }
    public void PlayerDeath(){
        helmAudio.ResetSynths();
        helmAudio.keys[4].PlayNote(40);
    }

    public void PlayerLeaveStart(){

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
        SynthController.flow = Services.PlayerBehaviour.curSpeed/Services.PlayerBehaviour.maxSpeed;
        clock.bpm = Mathf.Lerp(80, 300, SynthController.frequency);
    }

    public void OnFlying(){
        SynthController.instance.UpdateFlyingSynth();
    }

    public void Reset(){
        SynthController.instance.ResetSynths();   
        // synthMaster.SetFloat("Volume", 0f);
        audioRecordings.Stop();
        clock.pause = true;
    }

    public void Pause(bool pause){
        
        if(pause) audioRecordings.Pause();
        if(!pause) audioRecordings.Play();

        clock.pause = pause;

        if(pause){
            synthMaster.SetFloat("Volume", -80f);
        }else{
            synthMaster.SetFloat("Volume", 0f);
        }
    }
    public void FlyingSound(){
        
		GranularSynth.flying.TurnOn();
    }

}
