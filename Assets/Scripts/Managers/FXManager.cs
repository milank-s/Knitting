﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;

public enum ParticleType{start, lose}

public class FXManager : MonoBehaviour
{
    public enum FXType{fizzle, burst, rotate, pulse, cross, glitch}

    public PostProcessVolume postProcessing;
    public MeshRenderer background;
    Material backgroundMat;
    public GameObject circleEffect;
    public SpriteRenderer nextPointSprite;
    public TrailRenderer cursorTrail;
    public LineRenderer nextSpline;
    public SpriteRenderer nextSplineArrow;
    private Coroutine drawDir;
    public ParticleSystem popParticles, flyingParticles, linearParticles, trailParticles, brakeParticles;
    public TrailRenderer playerTrail, flyingTrail;
    public MeshFilter flyingParticleMesh, flyingParticleTrailMesh, flyingTrailMesh, playerTrailMesh, brakeParticleMesh;
    public GameObject MeshPrefab;
    public Text readout;
    public Text title;
    public Text subtitle;
    public ParticleSystem[] particlePrefabs;
    public GameObject[] spawnableSprites;
    public List<GameObject> spawnedSprites;
    private int lineIndex;
    private List<Vector3> linePositions;
    private VectorLine graffiti;
    private VectorLine collectibles;
    private List<VectorLine> splineDir;

    public Image overlay;

    public float graffitiInterval = 0.1f;
    float graffitiTimer;

    public float collectLineInterval = 0.1f;
    float collectLineTimer;

    public bool drawGraffiti;
    [SerializeField] private GameObject fxPrefab;
  private int index;
 
  private List<Animator> fxInstances = new List<Animator>();

  List<VectorLine> linesDrawn;
  List<Coroutine> lineDirectionRoutine;
  Coroutine showPointsRoutine;
  Coroutine graffitiRoutine;
    float backgroundLerp = 0;
   public float backgroundAlpha = 1;
  void Start()
  {
     backgroundMat = background.material;
      splineDir = new List<VectorLine>();
      for (int i = 0; i < 12; i++)
      {
          GameObject newFX = Instantiate(fxPrefab, Vector3.up * 1000, Quaternion.identity);
          newFX.transform.parent = transform;
          fxInstances.Add(newFX.GetComponent<Animator>());
      }
      
      graffiti = new VectorLine (name, new List<Vector3> (20), 2, LineType.Continuous, Vectrosity.Joins.Weld);
      graffiti.color = new Color(0.33f, 0.33f, 0.33f);

      collectibles = new VectorLine (name, new List<Vector3> (0), 2, LineType.Discrete, Vectrosity.Joins.Weld);
      collectibles.color = new Color(0.33f, 0.33f, 0.33f);


      linesDrawn = new List<VectorLine>();
      lineDirectionRoutine = new List<Coroutine>();
      flyingParticleMesh.mesh = new Mesh();
      flyingParticleTrailMesh.mesh = new Mesh();
      flyingTrailMesh.mesh = new Mesh();
      playerTrailMesh.mesh = new Mesh();
      brakeParticleMesh.mesh = new Mesh();

      //onstartflying plays flying particles
      //onstopflying bakes flying particles
      //onstarttraversing playes sparks
      //onenterpoint bakes sparks

      Services.PlayerBehaviour.OnStartFlying += flyingParticles.Play;
      Services.PlayerBehaviour.OnStartFlying += PlayFlyingTrail;
      Services.PlayerBehaviour.OnStoppedFlying += BakeFlyingTrail;

      //no no no no no non on onon
      Services.PlayerBehaviour.OnExitPoint += Services.PlayerBehaviour.sparks.Play;

      //Services.PlayerBehaviour.OnStoppedTraversing += BakeTraversingParticles;
      Services.main.OnReset += Reset;
      
  }

    public IEnumerator ShowTitle(string s){

		Services.fx.title.text = s;

		yield return new WaitForSeconds(1f);

		Services.fx.title.text = "";
	}

    public IEnumerator ShowDescription(string s){

        subtitle.text = s;
        yield return new WaitForSeconds(1.33f);

        subtitle.text = "";
        

    }
    public IEnumerator FlashWord(bool fadeIn = false)
	{
		float t = 0;
		while (t < 1)
		{
			if (!fadeIn)
			{
				title.color = Color.Lerp(Color.white, Color.clear, t);
			}
			else
			{
				title.color = Color.Lerp(Color.white, Color.clear, 1-t);
			}

			t += Time.deltaTime * 2;
			yield return null;
		} 

        if(fadeIn) title.color = !fadeIn ? Color.clear : Color.white;
	}

void Update(){
    backgroundLerp = Mathf.Lerp(backgroundLerp, backgroundAlpha, Time.deltaTime * 3);
    backgroundMat.color = new Color(0,0,0, backgroundLerp);
}
public void Step(){
    //backgroundLerp = Mathf.Lerp(backgroundLerp, SynthController.flow, Time.deltaTime * 5);
    DrawGraffiti();
    DrawCollectibleConnections();
}
  public void ShowUnfinished()
  {
      showPointsRoutine = StartCoroutine(ShowUnfinishedPoints());
  }

  public void ShowSplineDirection(Spline s, Point p1, Point p2)
  {

     lineDirectionRoutine.Add(StartCoroutine(DrawSplineDirection(s, p1, p2)));
   
  }
     IEnumerator DrawSplineDirection(Spline s, Point p1, Point p2)
    {
        yield return null;

        if(lineDirectionRoutine.Count == 0) yield break;
        
        //this is probably not reliable
        bool forward = s.IsGoingForward(p1, p2);
        
        float start = forward ? 0.1f : 0.9f;
        int pointIndex = s.GetPointIndex(p1);

        if(!forward){
            pointIndex -= 1;
            // if(s.closed) pointIndex = s.CheckForLoop(pointIndex);
            pointIndex = s.CheckForLoop(pointIndex);
            //p = s.SplinePoints[pointIndex];
        }

        Vector3 offset = s.GetCachedVelocity(pointIndex, start, forward);

        //doesnt work in 3d bro, gotta get cross product
        offset = new Vector3(-offset.y, offset.x, 0) / 10f;
        
        VectorLine newLine;
        Coroutine thisRoutine = lineDirectionRoutine[lineDirectionRoutine.Count-1];

        newLine = new VectorLine (name, new List<Vector3> (), 2, LineType.Continuous, Vectrosity.Joins.Weld);
        newLine.color = new Color(1,1,1);
        newLine.smoothWidth = true;
        newLine.smoothColor = true;
        newLine.layer = LayerMask.NameToLayer("Default");
        linesDrawn.Add(newLine);

        Material newMat = Services.Prefabs.lines[0];
        Texture tex = newMat.mainTexture;
        float length = newMat.mainTextureScale.x;
        float height = newMat.mainTextureScale.y * 0.75f;
		
        newLine.texture = tex;
        newLine.textureScale = length;
        newLine.lineWidth = height * 2;

        Vector3 endpoint = s.GetCachedPoint(pointIndex, 0.5f);
        Vector3 endDir = s.GetCachedVelocity(pointIndex, 0.5f, !forward);

      for (int i = 0; i < Spline.curveFidelity; i++)
      {
            float step = (0.5f * i)/Spline.curveFidelity;
            float oneMinus = forward ? step: 1-step;

          newLine.points3.Add(s.GetCachedPoint(pointIndex, oneMinus) + offset);
          newLine.Draw3D();
          yield return new WaitForSeconds(0.02f);
      }
      
      nextSplineArrow.enabled = true;
      nextSplineArrow.transform.position = endpoint + offset;
      nextSplineArrow.transform.up = endDir;
      
      for (int i = 0; i < Spline.curveFidelity; i++)
      {
          nextSplineArrow.enabled = true;
          newLine.points3.RemoveAt(0);
          newLine.Draw3D();
          yield return new WaitForSeconds(0.02f);
      }

      nextSplineArrow.enabled = false;
      
      lineDirectionRoutine.Remove(thisRoutine);
      linesDrawn.Remove(newLine);
      VectorLine.Destroy(ref newLine);
  }

    public void BakeFlyingParticles(){
        BakeParticles(flyingParticles, flyingParticleMesh);
    }

    public void BakeFlyingTrail(){
        BakeTrail(flyingTrail, flyingTrailMesh);
    }

    public void PlayFlyingTrail(){
        flyingTrail.emitting = true;
    }

  public void ShowNextPoint(Point p)
  {
      nextPointSprite.transform.position = p.Pos;
      nextPointSprite.enabled = true;
  }
 
  
  IEnumerator ShowUnfinishedPoints()
  {
      Transform glitchfx = PlayAnimationAtPosition(FXType.glitch, Services.Player.transform);
      yield return new WaitForSeconds(0.1f);
      
      foreach (Point p in Point.Points)
      {
          if (p.pointType != PointTypes.ghost)
          {
               if(p.state == Point.PointState.off)
              {
                  
                 glitchfx.transform.position = p.Pos;
                 yield return new WaitForSeconds(0.1f);
              }
          }

      }

      showPointsRoutine = null;
  }

  public void Reset()
  {
      backgroundLerp = 0;
      cursorTrail.Clear();

      if(graffitiRoutine != null){
          StopCoroutine(graffitiRoutine);
          graffitiRoutine = null;
      }

      if(showPointsRoutine != null){
          StopCoroutine(showPointsRoutine);
          showPointsRoutine = null;
      }

      if(lineDirectionRoutine.Count > 0){
          for(int i = lineDirectionRoutine.Count -1; i >= 0; i--){
          StopCoroutine(lineDirectionRoutine[i]);
        }
        
          lineDirectionRoutine.Clear();
      } 

      for(int i = linesDrawn.Count -1; i >= 0; i--){
          VectorLine v = linesDrawn[i];
          VectorLine.Destroy(ref v);
      }

      linesDrawn.Clear();

      flyingParticleMesh.mesh = new Mesh();
      flyingParticleTrailMesh.mesh = new Mesh();
      flyingTrailMesh.mesh = new Mesh();
      playerTrailMesh.mesh = new Mesh();
      brakeParticleMesh.mesh = new Mesh();
      flyingParticles.Clear();
      linearParticles.Clear();
      flyingTrail.emitting = false;
      flyingTrail.Clear();
      playerTrail.Clear();
      readout.text = "";
      
      for (int i = spawnedSprites.Count - 1; i >= 0; i--)
      {
          Destroy(spawnedSprites[i]);
      }
      
      spawnedSprites.Clear();
      
      VectorLine.Destroy(ref graffiti);
      VectorLine.Destroy(ref collectibles);
      
      for(int i = splineDir.Count -1 ; i >= 0; i--)
      {
          VectorLine v = splineDir[i];
          VectorLine.Destroy(ref v);
      }
      
      graffiti = new VectorLine (name, new List<Vector3> (0), 1, LineType.Continuous);
      graffiti.color = new Color(1,1,1);

      collectibles = new VectorLine (name, new List<Vector3> (0), 1, LineType.Discrete);
      collectibles.color = new Color(1,1,1);
      
      drawGraffiti = false;
  }
  public void BakeTrail(TrailRenderer t, MeshFilter f)
  {
      t.emitting = false;
      Mesh m = new Mesh();
      Mesh m2 = f.mesh;
      CombineInstance[] c = new CombineInstance[2];    
     t.BakeMesh(m);
      c[0].mesh = m;    
      c[1].mesh = f.mesh;
        f.mesh = new Mesh();
      //c[0].transform = flyingParticles.transform.localToWorldMatrix;
      f.mesh.CombineMeshes(c, true, false);
     t.Clear();
  }

  public void SpawnSprite(int i, Transform t)
  {

      GameObject newSprite = GameObject.Instantiate(spawnableSprites[i], t.position, Quaternion.identity);
      newSprite.AddComponent<FadeImage>();
      //spawnedSprites.Add(newSprite);

      //StartCoroutine(ScaleObject(newSprite.transform));
      //StartCoroutine(FlashSprite(newSprite.transform));
  }

  public void PlayParticle(ParticleType p, Vector3 pos, Vector3 dir){
    ParticleSystem newParticles = Instantiate(particlePrefabs[(int)p], pos, Quaternion.identity);
    newParticles.transform.forward = dir;
  }

  IEnumerator FlashSprite(Transform tr)
  {
      SpriteRenderer[] sprites = tr.GetComponentsInChildren<SpriteRenderer>();
      float t = 0;
      while (t < 1)
      {
          if (tr != null)
          {
              foreach (SpriteRenderer r in sprites)
              {
                  r.color = Color.Lerp(Color.clear, Color.white, Easing.QuintEaseIn(1-t));
                  
              }
              t += Time.deltaTime;
              yield return null;
          }
          else
          {
              yield break;
          }
      }
  }
  
  IEnumerator ScaleObject(Transform tr)
  {
      float t = 0.1f;
      while (t < 1)
      {
          if (tr != null)
          {
              tr.localScale = Vector3.one * Easing.QuadEaseIn(t) * 0.5f;
              t += Time.deltaTime;
              yield return null;
          }
          else
          {
              yield break;
          }
      }
  }
  
  public void BakeParticleTrail(ParticleSystem p, MeshFilter f)
  {
      p.Pause();
      Mesh m = new Mesh();
      Mesh m2 = f.mesh;
      CombineInstance[] c = new CombineInstance[2];    
      p.GetComponent<ParticleSystemRenderer>().BakeTrailsMesh(m);
      c[0].mesh = m;
      c[1].mesh = f.mesh;
      f.mesh = new Mesh();
      //c[0].transform = flyingParticles.transform.localToWorldMatrix;
      f.mesh.CombineMeshes(c, true, false);
  }
  
  

  public void BakeParticles(ParticleSystem p, MeshFilter f)
  {
      p.Pause();
      Mesh m = new Mesh();
      Mesh m2 = f.mesh;
      CombineInstance[] c = new CombineInstance[2];    
      p.GetComponent<ParticleSystemRenderer>().BakeMesh(m);
      c[0].mesh = m;
      c[1].mesh = f.mesh;
      f.mesh = new Mesh();
      //c[0].transform = flyingParticles.transform.localToWorldMatrix;
      f.mesh.CombineMeshes(c, true, false);
      p.Clear();
  }

  public Transform PlayAnimationAtPosition(FXType type, Transform t)
  {
      Animator fx = fxInstances[index];
      fx.SetTrigger(type.ToString());
      fx.transform.position = t.position;
      index = (index + 1) % fxInstances.Count;
      return fx.transform;
  }

  public void PlayAnimationOnPlayer(FXType type)
  {
      Animator fx = fxInstances[index];
      fx.SetTrigger(type.ToString());

      fx.transform.position = Services.Player.transform.position;
      fx.transform.up = Services.Cursor.transform.up;
      index = (index + 1) % fxInstances.Count;
  }

  public void EmitRadialBurst(int i, float force, Transform t)
  {
      ParticleSystem.MainModule m = popParticles.main;
      m.startSpeedMultiplier = force;
      popParticles.transform.position = t.position;
      popParticles.Emit(i);
  }
  
  public void EmitTracerBurst(int i, float force, Transform t)
  {
      ParticleSystem.MainModule m = trailParticles.main;
      trailParticles.transform.position = t.position;
      trailParticles.Emit(i);
  }

  public void EmitLinearBurst(int i, float force, Transform t, Vector3 direction)
  {
      ParticleSystem.MainModule m = linearParticles.main;
      m.startSpeedMultiplier = force + 1;
      linearParticles.transform.position = t.position;
      linearParticles.transform.up = direction;
      linearParticles.Emit(i);
  }

  public void DrawCollectibleConnections(){

    collectLineTimer -= Time.deltaTime;

    if(collectLineTimer < 0){
        
        collectibles.rectTransform.gameObject.SetActive(true);
        collectLineTimer = collectLineInterval;

        List<Vector3> points = new List<Vector3>();
        foreach(Collectible c in Services.main.activeStellation.collectibles){
            if(c.boidBehaviour.target != null){
                
                points.Add(c.transform.position);
                points.Add(c.boidBehaviour.target.position);
            }
        }

        collectibles.points3 = points;
        collectibles.Draw3D();
    }else{
        collectibles.rectTransform.gameObject.SetActive(false);
    }
  }
  public void DrawGraffiti()
  {
    graffitiTimer += Time.deltaTime;
    
    if(Services.PlayerBehaviour.easedAccuracy < 0.85f && graffitiTimer > graffitiInterval && Services.PlayerBehaviour.state == PlayerState.Traversing){
        
        graffiti.rectTransform.gameObject.SetActive(true);
        graffitiTimer = 0;

        List<Vector3> pos = new List<Vector3>();
        //Vector3[] positions = new Vector3[Services.fx.playerTrail.positionCount];
        //Services.fx.playerTrail.GetPositions(positions);
        //line.points3 = positions.ToList();
        List<Vector3> positions = new List<Vector3>();
        Spline s = Services.PlayerBehaviour.curSpline;
        if(!s.populatedPointPositions) return;

        for(int i = 0; i < Spline.curveFidelity; i++){
            //you sure this can't index out?
            int j = s.selectedIndex * Spline.curveFidelity + i;
            if(j < s.line.points3.Count){
                positions.Add(s.pointPositions[j]);
            }
        }

        graffiti.points3 = positions;
        graffiti.rectTransform.position = Random.insideUnitSphere * Mathf.Lerp(0.25f, 0.025f, Services.PlayerBehaviour.easedAccuracy);

    // if (playerTrail.positionCount > 0 && playerTrail.positionCount != line.points3.Count)
    // {
    //     //could also use Services.activeStellation._pointsHit;
    //     Vector3 v = playerTrail.GetPosition(playerTrail.positionCount-1);

        // line.points3.Add(v);
        
    // }

        if(graffiti.points3.Count > 0 && Random.Range(0, 100) > 50){
            int l1 = Random.Range(0, graffiti.points3.Count);
            int l2 = Random.Range(0, graffiti.points3.Count);
            Vector3 v1 = graffiti.points3[l1]; 
            Vector3 v2 = graffiti.points3[l2]; 
            graffiti.points3[l1] = v2;
            graffiti.points3[l2] = v1;
        }

        graffiti.Draw3D();

        }else{
            if(graffiti.rectTransform.gameObject.activeSelf)graffiti.rectTransform.gameObject.SetActive(false);
        }
    }
  

    public void SpawnCircle(Transform t){
        GameObject fx = Instantiate (circleEffect, t.position, Quaternion.identity);
        fx.transform.parent = t;
    }
 
 public void Fade(bool fadeIn, float time){
    if(fadeIn){
        StartCoroutine(FadeIn(time));
    }else{
        StartCoroutine(FadeOut(time));
    }
 }

public IEnumerator FadeIn(float fadeLength){
		
    float t = 0;
    
    while (t < 1){

        overlay.color = Color.Lerp(Color.black, Color.clear, Easing.QuadEaseIn(t/fadeLength));
        //GlitchEffect.SetValues(1-t);
        
        t += Time.unscaledDeltaTime/fadeLength;
        yield return null;
    }

    //GlitchEffect.SetValues(0);
    overlay.color = Color.clear;
}

	// public IEnumerator FlashImage(bool fadeIn = false)
	// {
	// 	float t = 0;
	// 	while (t < 1)
	// 	{
	// 		if (!fadeIn)
	// 		{
	// 			image.color = Color.Lerp(Color.white, Color.clear, t);
	// 		}
	// 		else
	// 		{
	// 			image.color = Color.Lerp(Color.white, Color.clear, 1-t);
	// 		}
	// 		t += Time.deltaTime/2;
	// 		yield return null;
	// 	} 
	// }
	
	
	public IEnumerator FadeOut(float fadeLength){
		
		float t = 0;
		while (t < 1)
		{
			overlay.color = Color.Lerp(Color.clear, Color.black, Easing.QuadEaseIn(t/fadeLength));
			// GlitchEffect.SetValues(t);
			t += Time.unscaledDeltaTime/fadeLength;
			yield return null;
		}

		// GlitchEffect.SetValues(1);
		overlay.color = Color.black;
	}
  
}
