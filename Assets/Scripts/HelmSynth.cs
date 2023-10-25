using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;
public class HelmSynth : MonoBehaviour
{
   public int octave = 0;
    public bool hasStutter = false;
    public bool hasTremelo = false;
    public bool hasDistortion = false;
    public HelmController patch;

   public void PlayNote(int note, float duration = 102, float velocity = 1){
        if(duration > 100){
            patch.NoteOn(note, velocity);
        }else{
            patch.NoteOn(note, velocity, duration);
        }
   }

   public void Stop(){
        patch.AllNotesOff();
   }

   public void Modulate(){

    //vibrate note based on speed;

    if(hasStutter){
       patch.SetParameterValue(Param.kStutterFrequency, SynthController.frequency); 
       Debug.Log("stutter frequency is " + patch.GetParameterValue(Param.kStutterFrequency));
    }
    if(hasTremelo){
        
       patch.SetParameterValue(Param.kPolyLfoFrequency, SynthController.frequency); 
       Debug.Log("LFO frequency is " + patch.GetParameterValue(Param.kPolyLfoFrequency));
       
    }
   }
   
}
