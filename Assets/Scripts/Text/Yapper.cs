using UnityEngine;
using System.Collections;

public class Yapper : MonoBehaviour {

	public GameObject wordObject;
	public string 	text;
	public bool fade, noRotation, delete, allAtOnce;
	public float fadeSpeed; 
	public float frequency = 0.33f;
	public float scale = 1;

	string[]		words;
	int 			wordCount;

	float timer = 0;

	void Start () {
		Cursor.visible = false;
		words = text.Split(new char[] {' '});
	}

	void Update(){
		if(timer - Time.time < 0){
			timer = Time.time + Random.Range(frequency, frequency + 0.2f);
			CreateWord(transform.position);
		}
	}

	public string GetText(){
		return words[wordCount % words.Length];
	}
	public GameObject CreateWord(Vector3 pos){
		wordCount++;
		GameObject newWord = (GameObject)Instantiate (wordObject, pos, Quaternion.identity);
		newWord.GetComponent<TextFormatting> ().setText (GetText());
		newWord.transform.localScale = newWord.transform.localScale * scale;
		if(allAtOnce) newWord.GetComponent<TextFormatting> ().setText (text);
		newWord.GetComponent<TextFormatting>().fade = fade;
		newWord.GetComponent<TextFormatting>().rotFixed = noRotation;
		newWord.GetComponent<TextFormatting> ().delete = delete;
		newWord.GetComponent<TextFormatting> ().fadeIn = fade;
		newWord.GetComponent<TextFormatting> ().speed = fadeSpeed;
		return newWord;
	}
}
