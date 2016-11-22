using UnityEngine;
using System.Collections;

public class TextHolder : MonoBehaviour {

	public GameObject wordObject;
	public string 	text;
	public bool fade, noRotation, delete, allAtOnce;
	public float speed; 

	string[]		words;
	int 			wordCount;

	void Start () {
		Cursor.visible = false;
		words = text.Split(new char[] {' '});
	}
	
	// Update is called once per frame
	void Update () {
	}

	public string GetText(){
		return words[wordCount % words.Length];
	}
	public GameObject CreateWord(Vector3 pos){
		wordCount++;
		GameObject newWord = (GameObject)Instantiate (wordObject, pos, Quaternion.identity);
		newWord.GetComponent<TextFormatting> ().setText (words [wordCount % words.Length]);
		if(allAtOnce) newWord.GetComponent<TextFormatting> ().setText (text);
		newWord.GetComponent<TextFormatting>().fade = fade;
		newWord.GetComponent<TextFormatting>().rotFixed = noRotation;
		newWord.GetComponent<TextFormatting> ().delete = delete;
		newWord.GetComponent<TextFormatting> ().fadeIn = fade;
		newWord.GetComponent<TextFormatting> ().speed = speed;
		return newWord;
	}
}
