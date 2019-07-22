using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Services{
	public static Point StartPoint{ get; set; } 
	public static GameObject Cursor { get; set; }
	public static GameObject Player { get; set; }
	public static PlayerBehaviour PlayerBehaviour { get; set; }
	public static PrefabManager Prefabs { get; set; }
	public static SoundBank Sounds { get; set; }
	
	public static Text Word { get; set; }
}
