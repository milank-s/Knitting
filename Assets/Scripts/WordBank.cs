using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordBank : MonoBehaviour {

	public string text;
	private string[] words;
	private int index;
	// Use this for initialization
	void Start () {
		index = 0;
		words = text.Split (new char[] { ' ' });
	}
	
	public string GetWord (){
		index++;
		return words[(index  -1) % (words.Length)]; 
	}
}
