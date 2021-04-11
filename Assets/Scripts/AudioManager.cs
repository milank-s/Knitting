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
    
    void Start()
    {
        clock.pause = true;
        instance = this;
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

    }

    public void EnterTraversing(){
        clock.pause = false;
        SynthController.instance.PlayMovementSynth();
    }

    public void ExitFlying(){

    }

    public void ExitTraversing(){

        SynthController.instance.StopMovementSynth();
    }

    public void OnTraversing(){
        SynthController.instance.UpdateMovementSynth();
    }

    public void OnFlying(){

    }

    public void HandleReset(PlayerState state){
        //do what we need to do
    }

    public void MuteSynths(bool mute)
    {
        foreach (GranularSynth s in GranularSynth.synths)
        {
            if (mute)
            {
                s.TurnOff();
            }
            else
            {
                s.TurnOn();
            }
        }
    }

    public void FlyingSound(){
        
				GranularSynth.flying.TurnOn();
    }
    void Update()
    {
        if (Services.PlayerBehaviour.state == PlayerState.Traversing)
        {
            SynthMaster.SetFloat("Distortion", Mathf.Clamp(0.5f - (Services.PlayerBehaviour.accuracy / 2f), 0, 0.5f));
        }
        else
        {
            SynthMaster.SetFloat("Distortion", 0);
        }
    }
    // Update is called once per frame
}
