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
    // Start is called before the first frame update

    public AudioMixer synths;
    

    public List<HelmController> pads;
    public List<HelmController> keys;
    public bool hasStartedNoise;
    public static SynthController instance;
    private int[] notes = {60, 65, 70, 75, 80, 82, 90};
    private int[] lowNotes = {30, 32, 36};

    
    private bool a, b, c, d;

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
        GetNotes(keyNote, 1, keys[0]);
    }

    void GetNotes(int[] n, int amount, HelmController c)
    {
	    n = new int[amount];
	    
	    for (int i = 0; i < amount; i++)
	    {
		    n[i] = notes[Random.Range(0, notes.Length)];
		    c.NoteOn(n[i], 0.2f, 0.05f);
	    }
    }

    void EndNotes(int[] notes, HelmController c)
    {
	    for (int i = 0; i < notes.Length; i++)
	    {
		    c.NoteOff(notes[i]);
	    }
    }

    public void SwitchState(PlayerState s)
    {
	    LeaveState(Services.PlayerBehaviour.state);

	    switch (s)
	    {
		    case PlayerState.Traversing:

			    if (Services.PlayerBehaviour.curPoint.pointType != PointTypes.ghost)
			    {
				    pads[1].NoteOn(padNote, Services.PlayerBehaviour.flow / 5f);
				    
				    //pads[0].NoteOn(notes[Random.Range(0, notes.Length)]);
				    //pads[1].NoteOn(lowNotes[Random.Range(0, lowNotes.Length)]);
				    noiseySynth.NoteOn(50, 1);
			    }

			    break;

		    case PlayerState.Flying:
			    
			    synths.SetFloat("Volume", -80);
			    flyingSynth.NoteOn(60, 1);
			    break;

		    case PlayerState.Switching:

				

			    break;
	    }
    }

    public void StopNotes()
    {

	    foreach (HelmController h in pads)
	    {
		    h.AllNotesOff();
	    }

	    foreach (HelmController h in keys)
	    {
			h.AllNotesOff();   
	    }
	    
	    movementSynth.AllNotesOff();
	    noiseySynth.AllNotesOff();
    }

    public void LeaveState(PlayerState s)
		{
			switch (s)
			{
				case PlayerState.Traversing:

					if (Services.PlayerBehaviour.pointDest.pointType != PointTypes.ghost)
					{
						pads[1].NoteOff(padNote);

						foreach (HelmController c in pads)
						{
							c.AllNotesOff();
						}

						noiseySynth.NoteOff(50);
					}

					break;

				case PlayerState.Flying:
					synths.SetFloat("Volume", 0);
					flyingSynth.NoteOff(60);
					break;

				case PlayerState.Switching:
					break;
			}
			
		}


    void TestNotes()
    {
	    if (Input.GetKeyDown(KeyCode.Alpha1))
	    {
		    if (pads[0].IsNoteOn(60))
		    {
			    pads[0].AllNotesOff();
		    }
		    else
		    {
			    pads[0].NoteOn(60);
		    }
	    }
	    
	    if (Input.GetKeyDown(KeyCode.Alpha2))
	    {
		    if (pads[1].IsNoteOn(60))
		    {
			    pads[1].AllNotesOff();
		    }
		    else
		    {
			    pads[1].NoteOn(60);
		    }
	    }
	    
	    if (Input.GetKeyDown(KeyCode.Alpha3))
	    {
		   
			    keys[0].NoteOn(80,1);
		    
	    }
	    
	    if (Input.GetKeyDown(KeyCode.Alpha4))
	    {
		    
			    keys[1].NoteOn(60, 1);
		    
	    }
    }
    
    
    void Update()
    {
	    //TestNotes();
	    
        float accuracy = Mathf.Clamp01(Services.PlayerBehaviour.accuracy);
	
        //Sound of noise when player goes of accuracy
        noiseySynth.SetParameterValue(Param.kVolume,Mathf.Clamp01( 1 - (Services.PlayerBehaviour.accuracy + 0.2f)) * Mathf.Clamp01(Services.PlayerBehaviour.flow/5f));
        
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
