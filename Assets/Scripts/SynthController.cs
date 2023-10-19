using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;
using UnityEngine.Audio;

public class SynthController : MonoBehaviour
{
	
	[Header("Keys")]
	public HelmController hitPointSFX;
	public HelmController boostSFX;
	public HelmController flySFX;
	public HelmSequencer sequencer;
	
	[Header("Pads")]

	//some kind of arp repeating a motif while the player moves
    public HelmController[] movementPad;
    public HelmController noisePad;
    public HelmController flyingSynth;
	

	public static int[] major = {0, 2, 4, 5, 7, 9, 11, 12};
	public static int[] minor = {0, 2, 3, 5, 7, 8, 10, 12};

    public bool hasStartedNoise;
    public static SynthController instance;
    private int[] notes = {60, 64, 67, 71};

	public string[] triadKeys;
	private int[][] triads;
    private int[] lowNotes = {30, 32, 36};
	int lineType => Services.PlayerBehaviour.curSpline.lineMaterial;
    private int curNote = 42;
	private int targetNote = 30;

	Point currentActivePoint;
	Point currentTargetPoint;

    private int[] keyNote;

    public void Awake()
    {
        instance = this;
		ConvertStringToTriad();
    }

	// TODO
	// Create different patches (or pitches?) for different point types
	// big ole switch statement for PlayNoteOnPoint
	// Choose arpeggio based on some factor. Player speed, line length?
	
	// try looping through the stellation and playing notes from a sequencer.
			// for each spline, set up a sequencer. 
			// populate this sequencer with notes for each point, with distance measured between them for rhythym, and pitch based on position

	// on play, set up crawlers to hit these notes along with the beat

	void ConvertStringToTriad(){
		triads = new int[triadKeys.Length][];
		for(int i = 0; i < triadKeys.Length; i++){
			string[] triadString = triadKeys[i].Split(new char[] {' '});
			int[] triadNotes = new int[triadString.Length];
			for(int j = 0; j < triadNotes.Length; j++){
				int curNote;
				if(int.TryParse(triadString[j], out curNote)){
					triadNotes[j] = curNote;
				}
			}
			triads[i] = triadNotes;
		}
	}

	public void ChooseRandomTriad(){
		int[] triad = triads[Random.Range(0, triads.Length)];
		sequencer.Clear();
		for(int i = 0; i < triad.Length; i++){
			sequencer.AddNote(triad[i], i, i + 1, 1);
		}
		sequencer.length = triad.Length;
	}
	public void UpdateFlyingSynth(){
		// flyingSynth.SetParameterPercent(Param.kVolume, 0.5f); 
	}
	public void PlayFlyingSynth(){
		flyingSynth.NoteOn(60);
	}

	public void StopFlying(){
		flyingSynth.AllNotesOff();
	}

	public void PlayNoteOnPoint(Point p){
		// int note = GetNote(p);
		// hitPointSFX.NoteOn(note + 12, 1f, 1f);
		
		int note = GetNote(p);
		hitPointSFX.NoteOn(note + triads[0][Random.Range(0, triads[0].Length)], 1f, 2f);
	}

	int GetNote(Point p){
		StellationController s = p.controller;
		//get bounds of stellation 

		float depth  = p.controller.GetNormalizedDepth(p.Pos);
		int octave = (int)Mathf.Floor((depth) * 4f)-2;

		octave *= 12;
		float height = p.controller.GetNormalizedHeight(p.Pos);

		// float magnitude = (s.upperRight.y - s.lowerLeft.y);
		// float normalizedY = p.Pos.y - s.lowerLeft.y;
		// normalizedY /= magnitude;
		// normalizedY = Mathf.Clamp01(normalizedY);
		
		int note = p.controller.rootKey + major[(int)Mathf.Floor(height * 7f)];// + octave;
		
		return note;
	}
	public void PlaySplineChord(){
		//set up the arp
		//ChooseRandomTriad();

		curNote = GetNote(Services.PlayerBehaviour.curPoint); 
		targetNote = GetNote(Services.PlayerBehaviour.pointDest)-12;

		movementPad[lineType].NoteOn(curNote);
		//boostSFX.NoteOn(curNote, 1, 3);
		
		int note = GetNote(Services.PlayerBehaviour.curPoint);
		
		//hitPointSFX.NoteOn(note + triads[0][Random.Range(0, triads[0].Length)], 0.15f, 1f);
		
		//hitPointSFX.NoteOn(note + major[triads[0][Random.Range(0, triads[0].Length)]], 1f, 1f);
		

	}

	public void StopSplineChord(){
		movementPad[lineType].AllNotesOff();
	}
	
	public void StartTraversing(){
		
		noisePad.NoteOn(60, 1);
		noisePad.NoteOn(55, 1);
	}

	public void StopTraversing(){
		noisePad.AllNotesOff();
	}

	public void MovementSynth(){
		
		//old code for using arp on the synth instead of using a sequencer
		//boostSFX.SetParameterPercent(Param.kArpFrequency, normalizedAccuracy);
		
		
		//distortion isn't really working
		
		// movementPad[lineType].SetParameterPercent(Param.kDistortionMix, distortion);
		//movementPad[lineType].SetParameterPercent(Param.kVolume, Mathf.Clamp(accuracy, 0.1f, 0.75f));

		//divide bounds into 12 pitches
		//based on the note's assigned pitch, move the wheel a portion of that amount to the target pitch

		//float playerY = Services.main.activeStellation.GetNormalizedHeight(Services.Player.transform.position);

		// Vector3 curPointPos = Services.PlayerBehaviour.curPoint.Pos;
		// Vector3 pointDestPos = Services.PlayerBehaviour	.pointDest.Pos;
		// Vector3 diff = pointDestPos - curPointPos;

		// int noteDiff = targetNote - curNote;

		// //float pitchBend = Utils.MidiChangeToRatio(diff);
		
		// float floor = diff.y > 0? curPointPos.y : pointDestPos.y;
		// float scaledPlayerY = (Services.Player.transform.position.y - floor) / diff.magnitude;
		// scaledPlayerY = diff.y > 0 ? scaledPlayerY : scaledPlayerY -1;
		// Debug.Log(scaledPlayerY);
		// movementPad[lineType].SetParameterValue(Param.kPitchBendRange, Mathf.Abs(noteDiff));

		// // linear for now
		
		// movementPad[lineType].SetPitchWheel(scaledPlayerY);


		//noise time
		noisePad.SetParameterPercent(Param.kVolume, Services.PlayerBehaviour.easedDistortion);
		// AudioManager.instance.pianoSampler
	}



    void PlayRandomChord(int[] n, int amount, HelmController c,  float velocity = 1, float length = 0)
    {
	    n = new int[amount];
	    
	    for (int i = 0; i < amount; i++)
	    {
		    n[i] = notes[Random.Range(0, notes.Length)];

			//hold indefinitely
			if(length == 0){
				c.NoteOn(n[i], velocity);
			}else{
		    	c.NoteOn(n[i], velocity, length);
			}
	    }
    }

    void EndNotes(int[] notes, HelmController c)
    {
	    for (int i = 0; i < notes.Length; i++)
	    {
		    c.NoteOff(notes[i]);
	    }
    }

    public void ResetSynths()
    {
	    flyingSynth.AllNotesOff(); 
	    boostSFX.AllNotesOff();
	    foreach(HelmController s in movementPad){
			s.AllNotesOff();
		}
		
		noisePad.AllNotesOff();
		hitPointSFX.AllNotesOff();
    }

    void TestNotes()
    {
	    if (Input.GetKeyDown(KeyCode.Alpha1))
	    {

		    if (boostSFX.IsNoteOn(60))
		    {
			    boostSFX.AllNotesOff();
		    }
		    else
		    {
			    boostSFX.NoteOn(60);
		    }
	    }
	    
	    if (Input.GetKeyDown(KeyCode.Alpha2))
	    {
		    if (movementPad[lineType].IsNoteOn(60))
		    {
			    movementPad[lineType].AllNotesOff();
		    }
		    else
		    {
			    movementPad[lineType].NoteOn(60);
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
		

        //Sound of noise when player goes of accuracy
        //movementPad[lineType].SetParameterValue(Param.kVolume,Mathf.Clamp01( 1 - (Services.PlayerBehaviour.accuracy + 0.2f)) * Mathf.Clamp01(Services.PlayerBehaviour.flow/5f));
        
        //slight pitch bend on accuracy
        //pads[1].SetParameterPercent(Param.kOsc2Tune, accuracy);
        //volume on speed
        //pads[1].SetParameterPercent(Param.kVolume, Services.PlayerBehaviour.flow/5f);
        
        
        //movementPad[lineType].SetParameterValue(Param.kVolume, Mathf.Lerp(0, Mathf.Clamp01( accuracy) * Mathf.Clamp01(Mathf.Pow(Services.PlayerBehaviour.flow,2)), Services.PlayerBehaviour.decelerationTimer));
        

        //boostSFX.SetParameterPercent(Param.kArpTempo, (Services.PlayerBehaviour.flow/5f) * accuracy);
        
		//boostSFX.SetParameterPercent(Param.kStutterResampleFrequency,  accuracy/2f);
		//boostSFX.SetParameterPercent(Param.kStutterFrequency, accuracy/2f);
		//boostSFX.SetParameterPercent(Param.kArpTempo, accuracy/10f);
		
		//boostSFX.SetParameterValue(Param.kVolume,  Mathf.Lerp(boostSFX.GetParameterValue(Param.kVolume), Mathf.Clamp01(Services.PlayerBehaviour.flow - 0.25f) / 2f, Time.deltaTime));
//        if (!hasStartedNoise && Services.PlayerBehaviour.state == PlayerState.Traversing)
//        {
//            
//            boostSFX.FrequencyOn( 261.6f * 5f);
//            
//            hasStartedNoise = true;
//        }


//            movementPad[lineType].SetParameterValue(Param.kDistortionMix, 1);

//        if (Services.PlayerBehaviour.curSpeed > 0.25f && !a)
//        {
//            boostSFX.FrequencyOn( 261.6f, 0.5f);
//            a = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 0.25f || Services.PlayerBehaviour.boostTimer > 0) && a)
//        {
//            boostSFX.FrequencyOff(261.6f);
//            a = false;
//        }
//        
//        if (Services.PlayerBehaviour.curSpeed > 0.75f && !b)
//        {
//            boostSFX.FrequencyOn( 261.6f * 1.5f, 0.5f);
//            b = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 0.75f || Services.PlayerBehaviour.boostTimer > 0) && b)
//        
//        {
//            boostSFX.FrequencyOff(261.6f * 1.5f);
//            b = false;
//        }
//        
//        if (Services.PlayerBehaviour.curSpeed > 1.5f && !c)
//        {
//            boostSFX.FrequencyOn( 261.6f * 1.5f * 1.5f, 0.5f);
//            c = true;
//            
//        }else if ((Services.PlayerBehaviour.curSpeed < 1.5f || Services.PlayerBehaviour.boostTimer > 0) && c)
//        {
//            boostSFX.FrequencyOff(261.6f * 1.5f * 1.5f);
//            c = false;
//        }
//        
//        if (Services.PlayerBehaviour.boostTimer > 0 && !d)
//        {
//            boostSFX.NoteOn(24, 1, 1);
//            d = true;
//            
//        }else if (Services.PlayerBehaviour.boostTimer == 0 && d)
//        {
//            boostSFX.NoteOff(24);
//            d = false;
//        }
    }
}
