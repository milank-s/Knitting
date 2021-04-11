using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;
using UnityEngine.Audio;

public class SynthController : MonoBehaviour
{
    public HelmController movementSynth;
    public HelmController noiseySynth;
    public HelmController flyingSynth;
	
	public HelmController keys;

    // Start is called before the first frame update

    public AudioMixer synths;
    public bool hasStartedNoise;
    public static SynthController instance;
    private int[] notes = {60, 64, 67, 71};
    private int[] lowNotes = {30, 32, 36};

    private int padNote = 42;

    private int[] keyNote;

    public void Awake()
    {
        instance = this;
    }

    public void PlayNote(int i)
    {
        //how many should play?
        //which note and which patch?
        //how do you modulate the frequency once the note is being played?

    }

	public void PlayMovementSynth(){
		PlayRandomChord(notes, 1, movementSynth);
		//noiseySynth.NoteOn(64);
	}

	public void UpdateMovementSynth(){
		
			float normalizedAccuracy = (1 + Services.PlayerBehaviour.accuracy)/2f;;

			//pitch bending
			//movementSynth.SetParameterPercent(Param.kArpFrequency, normalizedAccuracy);
			movementSynth.SetParameterPercent(Param.kOsc1Tune, normalizedAccuracy);
			
			
			//distortion
			float distortion = Mathf.Clamp01((1 - normalizedAccuracy) * 2)/2f;
			movementSynth.SetParameterPercent(Param.kDistortionMix, distortion);
			//noiseySynth.SetParameterPercent(Param.kVolume, distortion);
	}

	public void StopMovementSynth(){
		noiseySynth.AllNotesOff();
		movementSynth.AllNotesOff();
	}


    void PlayRandomChord(int[] n, int amount, HelmController c)
    {
	    n = new int[amount];
	    
	    for (int i = 0; i < amount; i++)
	    {
		    n[i] = notes[Random.Range(0, notes.Length)];
		    c.NoteOn(n[i]);
	    }
    }

    void EndNotes(int[] notes, HelmController c)
    {
	    for (int i = 0; i < notes.Length; i++)
	    {
		    c.NoteOff(notes[i]);
	    }
    }

    public void StopNotes()
    {

	    flyingSynth.AllNotesOff(); 
	    movementSynth.AllNotesOff();
	    noiseySynth.AllNotesOff();
    }

    void TestNotes()
    {
	    if (Input.GetKeyDown(KeyCode.Alpha1))
	    {
			Debug.Log("movement synth");

		    if (movementSynth.IsNoteOn(60))
		    {
			    movementSynth.AllNotesOff();
		    }
		    else
		    {
			    movementSynth.NoteOn(60);
		    }
	    }
	    
	    if (Input.GetKeyDown(KeyCode.Alpha2))
	    {
		    if (noiseySynth.IsNoteOn(60))
		    {
			    noiseySynth.AllNotesOff();
		    }
		    else
		    {
			    noiseySynth.NoteOn(60);
		    }
	    }
	    
	    if (Input.GetKeyDown(KeyCode.Alpha3))
	    {
		   
			if (flyingSynth.IsNoteOn(60))
		    {
			    flyingSynth.AllNotesOff();
		    }
		    else
		    {
			    flyingSynth.NoteOn(60);
		    }
	    }
	    
    }
    
    
    void Update()
    {
		

        float accuracy = Mathf.Clamp01(Services.PlayerBehaviour.accuracy);
	
        //Sound of noise when player goes of accuracy
        //noiseySynth.SetParameterValue(Param.kVolume,Mathf.Clamp01( 1 - (Services.PlayerBehaviour.accuracy + 0.2f)) * Mathf.Clamp01(Services.PlayerBehaviour.flow/5f));
        
        //slight pitch bend on accuracy
        //pads[1].SetParameterPercent(Param.kOsc2Tune, accuracy);
        //volume on speed
        //pads[1].SetParameterPercent(Param.kVolume, Services.PlayerBehaviour.flow/5f);
        
        
        //noiseySynth.SetParameterValue(Param.kVolume, Mathf.Lerp(0, Mathf.Clamp01( accuracy) * Mathf.Clamp01(Mathf.Pow(Services.PlayerBehaviour.flow,2)), Services.PlayerBehaviour.decelerationTimer));
        

        //movementSynth.SetParameterPercent(Param.kArpTempo, (Services.PlayerBehaviour.flow/5f) * accuracy);
        
		//movementSynth.SetParameterPercent(Param.kStutterResampleFrequency,  accuracy/2f);
		//movementSynth.SetParameterPercent(Param.kStutterFrequency, accuracy/2f);
		//movementSynth.SetParameterPercent(Param.kArpTempo, accuracy/10f);
		
		//movementSynth.SetParameterValue(Param.kVolume,  Mathf.Lerp(movementSynth.GetParameterValue(Param.kVolume), Mathf.Clamp01(Services.PlayerBehaviour.flow - 0.25f) / 2f, Time.deltaTime));
//        if (!hasStartedNoise && Services.PlayerBehaviour.state == PlayerState.Traversing)
//        {
//            
//            movementSynth.FrequencyOn( 261.6f * 5f);
//            
//            hasStartedNoise = true;
//        }


//            noiseySynth.SetParameterValue(Param.kDistortionMix, 1);

//        if (Services.PlayerBehaviour.curSpeed > 0.25f && !a)
//        {
//            movementSynth.FrequencyOn( 261.6f, 0.5f);
//            a = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 0.25f || Services.PlayerBehaviour.boostTimer > 0) && a)
//        {
//            movementSynth.FrequencyOff(261.6f);
//            a = false;
//        }
//        
//        if (Services.PlayerBehaviour.curSpeed > 0.75f && !b)
//        {
//            movementSynth.FrequencyOn( 261.6f * 1.5f, 0.5f);
//            b = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 0.75f || Services.PlayerBehaviour.boostTimer > 0) && b)
//        
//        {
//            movementSynth.FrequencyOff(261.6f * 1.5f);
//            b = false;
//        }
//        
//        if (Services.PlayerBehaviour.curSpeed > 1.5f && !c)
//        {
//            movementSynth.FrequencyOn( 261.6f * 1.5f * 1.5f, 0.5f);
//            c = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 1.5f || Services.PlayerBehaviour.boostTimer > 0) && c)
//        {
//            movementSynth.FrequencyOff(261.6f * 1.5f * 1.5f);
//            c = false;
//        }
//        
//        if (Services.PlayerBehaviour.boostTimer > 0 && !d)
//        {
//            movementSynth.NoteOn(24, 1, 1);
//            d = true;
//            
//        }else if (Services.PlayerBehaviour.boostTimer == 0 && d)
//        {
//            movementSynth.NoteOff(24);
//            d = false;
//        }
    }
}
