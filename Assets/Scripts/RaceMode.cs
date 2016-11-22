//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;
//
//public class RaceMode : MonoBehaviour {
//
//	[Header("UI Text")]
//	public Text Timer;
//	public Text Split;
//	public Text Score;
//	public TextMesh flowUI;
//	public BezierSpline[] tracks;
//	private BezierSpline curTrack;
//	private float timer = 1;
//	private GameObject player;
//	private float time, bestTime;
//	bool cyclingTrack = false;
//	private int trackIndex;
//	// Use this for initialization
//	void Start () {
//		player = GameObject.FindGameObjectWithTag ("Player");
//		Split.text = " ";
//		Score.text = " ";
//		bestTime = Mathf.Infinity;
//		curTrack = tracks [trackIndex];
//		GetComponent<SplineDecorator> ().spline = curTrack;
//		player.GetComponent<PlayerTraversal> ().curEdge.spline = curTrack;
//		GetComponent<SplineDecorator>().UpdateSpline(curTrack);
//	}
//	
//	// Update is called once per frame
//	void Update () {
////		LapTimes ();
//		timer -= Time.deltaTime;
//		if (timer <= 0) {
//			if (!cyclingTrack) {
//				AllLightsOn ();
//			}
//		}
//	}
//
//	void LapTimeWin(){
//		if (bestTime < curTrack.ControlPointCount *0.5f) {
//			trackIndex++;
//			trackIndex %= tracks.Length;
//			curTrack = tracks [trackIndex];
//			player.GetComponent<PlayerTraversal> ().curEdge.spline = curTrack;
//			GetComponent<SplineDecorator> ().UpdateSpline(curTrack);
//			//curTrack.transform.position += tracks [curTrack - 1 % tracks.Length].GetPoint (1) - tracks [curTrack % tracks.Length].transform.position;
//			bestTime = Mathf.Infinity;
//			time = 0;
//			Score.text = " ";
//			player.GetComponent<PlayerTraversal> ().SetFlow (0);
//		}
//	}
//
//	void AllLightsOn(){
//		bool allOn = true;
//			
//		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Node")) {
//			if (!g.GetComponent<LightUp> ().IsOn ()) {
//				allOn = false;
//				break;
//			}
//		}
//		if (allOn) {
//			
//			StartCoroutine (CycleTrack ());
//		}
//	}
//
//
//	IEnumerator CycleTrack(){
//		cyclingTrack = true;
//		trackIndex++;
//		trackIndex %= tracks.Length;
//		curTrack = tracks [trackIndex];
//		foreach (GameObject n in GameObject.FindGameObjectsWithTag("Node")) {
//			yield return new WaitForSeconds (0.1f);
//			Destroy(n.GetComponent<LightUp>());
//			Destroy(n.GetComponent<Light>());
//			n.tag = "Untagged";
//			player.GetComponent<PlayerPickups> ().Pickup (n.GetComponent<Node>());
//		}
//		yield return new WaitForSeconds (0.5f);
//		player.GetComponent<PlayerTraversal> ().curEdge.spline = curTrack;
//		GetComponent<SplineDecorator> ().UpdateSpline (curTrack);
//		//curTrack.transform.position += tracks [curTrack - 1 % tracks.Length].GetPoint (1) - tracks [curTrack % tracks.Length].transform.position;
//		bestTime = Mathf.Infinity;
//		time = 0;
//		cyclingTrack = false;
//	}
//
//	void LapTimes(){
//		time += Time.deltaTime;
//		float alpha = Split.color.a;
//		Split.color = new Color(255, 255, 255, alpha -= Time.deltaTime);
//
//
//		if( player.GetComponent<PlayerTraversal>().GetProgress() >= 1){
//
//			float split = (time - bestTime);
//			if (split > 0) {
//				Split.text = "+" + split.ToString ("F");
//			} else {
//				Split.text = split.ToString ("F");
//			}
//			Split.color = Color.white;
//			if (time < bestTime) {
//				bestTime = time;
//				Score.text = bestTime.ToString("F");
//			}
//			time = 0;
//		}
//		Timer.text = time.ToString ("F");
//		//flowUI.text = player.GetComponent<PlayerTraversal>().GetFlow().ToString ("F1");
//		//flowUI.transform.position = player.transform.position + Vector3.down + Vector3.right;
//	}
//}
