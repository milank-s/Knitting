using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using UnityEngine;
using UnityEngine.Audio;

public class SynthController : MonoBehaviour
{
	
	[Header("Keys")]
	
	public HelmSynth flySFX;
	public HelmSequencer sequencer;
    public HelmSynth[] keys;
	
	[Header("Pads")]

	//some kind of arp repeating a motif while the player moves
    public HelmSynth[] pads;
    public HelmSynth[] flutters;
    public HelmSynth noisePad;
    public HelmSynth flyingSynth;
    public HelmSynth flowSynth;

	private HelmSynth currentFlutter;
	
	public static float frequency = 0.25f;
	public static float flow = 0.25f;

	int homeNote;

	public string homeKeys;
	int[] homeNotes;
	public static int[] major = {0, 2, 4, 5, 7, 9, 11, 12};
	public static int[] minor = {0, 2, 3, 5, 7, 8, 10, 12};

    public bool hasStartedNoise;
    public static SynthController instance;

	public string[] triadKeys;
	private int[][] triads;
	int lineType = 0;
    private int curNote = 42;
	private int targetNote = 30;

	Point currentActivePoint;
	Point currentTargetPoint;

	HelmSynth[] instruments;
	int curInstrument;
	int curPatch;
    private int[] keyNote;

    public void Awake()
    {
		
		homeNote = 60;
		instruments = keys;
        instance = this;
		
		string[] homers = homeKeys.Split(new char[] {' '});
		homeNotes = new int[homeKeys.Length];
		for(int j = 0; j < homers.Length; j++){
			int curNote;
			if(int.TryParse(homers[j], out curNote)){
				homeNotes[j] = curNote;
			}
		}

		ConvertStringToTriad();
    }
	
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

	public void SetArpeggio(){
		int[] triad = triads[Random.Range(0, triads.Length)];
		sequencer.Clear();
		for(int i = 0; i < triad.Length; i++){
			sequencer.AddNote(triad[i], i, i + 1, 1);
		}
		sequencer.length = triad.Length;
	}

	public void UpdateFlyingSynth(){
		
	}

	public void StartFlying(){
		flyingSynth.PlayNote(60, 3);
	}

	public void StopFlying(){
		flyingSynth.Stop();
	}

	public void PlayNoteOnPoint(Point p){
		
		float length = 1;
		HelmSynth s;

		if(p.pointType == PointTypes.fly){
			 length = 2;
			 s = keys[4];
		}else if(p.pointType == PointTypes.stop){
			 s = keys[2];
		}else if(p.pointType == PointTypes.start){
			s = keys[5];
		}else if(p.pointType == PointTypes.normal){
			s = keys[0];
		}else if (p.pointType == PointTypes.reset){
			 s = keys[4];
		}else{
			s = keys[3];
		}

		s.PlayNote(p.note, length);
	}

	public int GetNote(Point p){
		StellationController s = p.controller;
		//get bounds of stellation 

		float depth  = p.controller.GetNormalizedDepth(p.Pos);
		int octave = (int)Mathf.Floor((depth) * 4f)-2;

		octave *= 12;
		
		//get note on scale based on x position relative to stellation bounds
		float height = p.controller.GetNormalizedHeight(p.Pos);
		float width = p.controller.GetNormalizedWidth(p.Pos);

		// int note = homeNote + major[(int)Mathf.Floor(width * (major.Length-1))]; // + octave;
		int note = homeNote + major[(int)(Random.value * major.Length-1)]; // + octave;

		return note;
	}
	public void PlaySplineChord(){
		//set up the arp
		//ChooseRandomTriad();
		
		curNote = Services.PlayerBehaviour.curPoint.note; 
		targetNote = Services.PlayerBehaviour.pointDest.note;

		//would like to create multiple voices
		//each line has a distinct voice but theres a constant between them

		// if(SynthController.frequency > 0.75){
			
		// 	currentFlutter = flutters[2];
		// 	currentFlutter.PlayNote(curNote + 2);
		// }

		// else if(SynthController.frequency > 0.5){
			
		// 	currentFlutter = flutters[1];
		// 	currentFlutter.PlayNote(curNote + 2);
		// }

		// else if(SynthController.frequency > 0.33){
			
		// 	currentFlutter = flutters[0];
		// 	currentFlutter.PlayNote(curNote + 2);
		// }
		
		//do we want to add another voice per x steps between notes?
		//to create triads quads etc
		pads[lineType].PlayNote(curNote);
		pads[lineType].PlayNote(targetNote);
	}

	public void StopSplineChord(){
		if(currentFlutter != null){
			currentFlutter.Stop();
		}
		pads[lineType].Stop();
	}
	
	public void ExitPoint(){
		
		noisePad.PlayNote(62);
		noisePad.PlayNote(64);
		
		PlaySplineChord();
		
		flowSynth.PlayNote(42);
		
		pads[lineType].Mute(false);
		
        if(currentFlutter != null){
			currentFlutter.Mute(false);
		}
	}

	public void EnterPoint(){
        
		noisePad.Stop();
		// pads[lineType].Mute(true);

        if(currentFlutter != null){
			currentFlutter.Mute(true);
		}
	}

	public void MovementSynth(){
		
		if(currentFlutter != null){
			//this isn't good because synths have different loudnesses from each other
			//need to set gain on the mixer side not on the patch side
			// currentFlutter.SetVolume(Services.PlayerBehaviour.normalizedAccuracy);
		}
		noisePad.SetVolume(Services.PlayerBehaviour.easedDistortion);
		pads[lineType].SetVolume(flow/2f);
		flowSynth.SetVolume(Mathf.Pow(flow, 3));
		//pitch bending
		//based on the note's assigned pitch, move the wheel a portion of that amount to the target pitch

		// Vector3 curPointPos = Services.PlayerBehaviour.curPoint.Pos;
		// Vector3 pointDestPos = Services.PlayerBehaviour.pointDest.Pos;
		// Vector3 diff = pointDestPos - curPointPos;

		// int noteDiff = targetNote - curNote;

		// //float pitchBend = Utils.MidiChangeToRatio(diff);
		
		// float floor = diff.y > 0? curPointPos.y : pointDestPos.y;
		// float scaledPlayerY = (Services.Player.transform.position.y - floor) / diff.magnitude;
		// scaledPlayerY = diff.y > 0 ? scaledPlayerY : scaledPlayerY -1;

		// pads[lineType].SetParameterValue(Param.kPitchBendRange, Mathf.Abs(noteDiff));

		// // linear for now
		// pads[lineType].SetPitchWheel(scaledPlayerY);


		//old code for using arp on the synth instead of using a sequencer
		//boostSFX.SetParameterPercent(Param.kArpFrequency, normalizedAccuracy);
	
	}

    void PlayRandomChord(int baseKey, int voices, HelmSynth c,  float velocity = 1, float length = 0)
    {
	    int[] n = new int[voices];
	    
	    for (int i = 1; i < voices; i++)
	    {
		    n[i] = baseKey + i * 2;

			//hold indefinitely
			if(length == 0){
				c.PlayNote(n[i]);
			}else{
		    	c.PlayNote(n[i], length, velocity);
			}
	    }
    }

    public void ResetSynths()
    {
		
	    flyingSynth.Stop();

	    foreach(HelmSynth s in pads){
			s.Stop();
		}

		foreach(HelmSynth s in keys){
			s.Stop();
		}

		foreach(HelmSynth s in flutters){
			s.Stop();
		}
		
		noisePad.Stop();
		
    }

    void TestNotes()
    {
		int note = 60 + Random.Range(0, major.Length);
		//instruments[curPatch].Modulate();

		if(Input.GetKeyDown(KeyCode.LeftShift)){
			curPatch = 0;
			curInstrument ++;

			if(curInstrument == 1){
				instruments = pads;
			}
			else if(curInstrument == 2){
				instruments = flutters;
			}else{
				instruments = keys;
				curInstrument = 0;
			}
		}

	    if (Input.GetKeyDown(KeyCode.Alpha1))
	    {
			curPatch = 0;
			instruments[curPatch].PlayNote(note);
	    }

		 if (Input.GetKeyUp(KeyCode.Alpha1))
	    {

			instruments[curPatch].Stop();
	    }
	    
	    if (Input.GetKeyDown(KeyCode.Alpha2))
	    {
			curPatch = 1;
			instruments[curPatch].PlayNote(note);
	    }

		if (Input.GetKeyUp(KeyCode.Alpha2))
	    {
			instruments[curPatch].Stop();
	    }
	    
	    if (Input.GetKeyDown(KeyCode.Alpha3))
	    {
			curPatch = 2;
			instruments[curPatch].PlayNote(note);
	    }

		 if (Input.GetKeyUp(KeyCode.Alpha3))
	    {
			instruments[curPatch].Stop();
	    }


		if (Input.GetKeyDown(KeyCode.Alpha4))
	    {
			curPatch = 3;
			instruments[curPatch].PlayNote(note);
	    }

		 if (Input.GetKeyUp(KeyCode.Alpha4))
	    {
			
			instruments[curPatch].Stop();
	    }

		if (Input.GetKeyDown(KeyCode.Alpha5))
	    {
			curPatch = 4;
			instruments[curPatch].PlayNote(note);
	    }

		 if (Input.GetKeyUp(KeyCode.Alpha5))
	    {
			instruments[curPatch].Stop();
	    }

		if (Input.GetKeyDown(KeyCode.Alpha6))
	    {
			curPatch = 5;
			instruments[curPatch].PlayNote(note);
	    }

		 if (Input.GetKeyUp(KeyCode.Alpha6))
	    {
			instruments[curPatch].Stop();
	    }

    }
    
    
    void Update()
    {
		
		//TestNotes();
		
        //Sound of noise when player goes of accuracy
        //pads[lineType].SetParameterValue(Param.kVolume,Mathf.Clamp01( 1 - (Services.PlayerBehaviour.accuracy + 0.2f)) * Mathf.Clamp01(Services.PlayerBehaviour.flow/5f));
        
        //slight pitch bend on accuracy
        //pads[1].SetParameterPercent(Param.kOsc2Tune, accuracy);
        //volume on speed
        //pads[1].SetParameterPercent(Param.kVolume, Services.PlayerBehaviour.flow/5f);
        
        
        //pads[lineType].SetParameterValue(Param.kVolume, Mathf.Lerp(0, Mathf.Clamp01( accuracy) * Mathf.Clamp01(Mathf.Pow(Services.PlayerBehaviour.flow,2)), Services.PlayerBehaviour.decelerationTimer));
        

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


//            pads[lineType].SetParameterValue(Param.kDistortionMix, 1);

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
//            boostSFX.PlayNote(24, 1, 1);
//            d = true;
//            
//        }else if (Services.PlayerBehaviour.boostTimer == 0 && d)
//        {
//            boostSFX.NoteOff(24);
//            d = false;
//        }
    }
}
