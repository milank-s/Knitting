﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Services{
	public static MenuController menu;
	public static Point StartPoint{ get; set; } 
	public static GameObject Cursor { get; set; }
	public static GameObject Player { get; set; }
	public static PlayerBehaviour PlayerBehaviour { get; set; }
	public static PrefabManager Prefabs { get; set; }
	public static SoundBank Sounds { get; set; }
	
	public static Camera mainCam { get; set; }
	public static Main main{ get; set; }
	public static FXManager fx;
	public static GameObject GameUI { get; set; }
}
