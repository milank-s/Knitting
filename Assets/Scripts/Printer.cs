using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour
{
    public float frequency = 0.1f;
    public GameObject textContainer;

    float lastProgress;

    void Start(){
        Services.PlayerBehaviour.OnTraversing += TrySpawnWord;
        Services.main.OnReset += Reset;
    }
    public void SpawnWord(){
        Point p = Services.PlayerBehaviour.pointDest;
        string word = p.controller.GetWord();
        if(word != ""){
            TextMesh newText = Instantiate(Services.Prefabs.text, Services.Player.transform.position, Quaternion.identity).GetComponent<TextMesh>();   
            newText.text = word;
            newText.color = Color.white;
            newText.transform.parent = textContainer.transform;
            Vector3 forward = Services.PlayerBehaviour.curSpline.GetVelocity(Services.PlayerBehaviour.progress);
            forward.Normalize();
            forward = new Vector3(-forward.y, forward.x, 0);
            newText.transform.right = forward;
        }
    }

    public void TrySpawnWord(){
        if(Mathf.Abs(Services.PlayerBehaviour.progress - lastProgress) > frequency){
            lastProgress = Services.PlayerBehaviour.progress;
            SpawnWord();
        }
    }

    public void Reset(){
        Destroy(textContainer);
        textContainer = new GameObject();
        textContainer.transform.parent = transform;
    }
}
