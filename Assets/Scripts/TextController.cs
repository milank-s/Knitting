using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextController : MonoBehaviour
{
   
   public string[] sentences;
    TextMeshPro text;

    int sentenceIndex = 0;

    void Start(){
        text = GetComponent<TextMeshPro>();
        text.text = sentences[0];
    }

    public void Disable(){
        text.renderer.enabled = false;
    }

    public void NextSentence(){
        sentenceIndex ++;
        text.text = sentences[sentenceIndex % sentences.Length];
    }
   
   public void RepeatSentence(){
       text.text += '\n' + sentences[sentenceIndex];
   }
}
