using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSPS;

public class Manager : MonoBehaviour {
	
	private int finishedDolls = 0;
	public GameObject[] Cylinders;
	private Mood[] Moods = {
		new Mood("Surprised", new Color(0.9f,0,0), "Surprised"),
		new Mood("Happy", new Color(0,0.7f,0), "Happy"),
		new Mood("Sad", new Color(0,0,0.9f), "Sad"),
		new Mood("Shy", new Color(1,1,1), "Hiding")
	};
	public GameObject[] Dolls;
	public Material[] DollMaterials;
	private Mood Current = null;
	private bool mousePressed = false;
	private bool transitioning = false;
	private Vector3[] dollPositions = {
		new Vector3(5.75f, 2.5f, 7),
		new Vector3(-5.50f, 2.25f, 6),
		new Vector3(-5.10f, -2.25f, 5),
		new Vector3(4.90f, -2.25f, 4)
	};
	private Vector3[] topDollOpenings = {
		new Vector3(0,2,0),
		new Vector3(0,1.75f,0),
		new Vector3(0, 1.5f, 0),
		new Vector3(0, 1.25f, 0)
	};
	private Vector3[] bottomDollOpenings = {
		new Vector3(0, -1.5f,0),
		new Vector3(0, -1.15f,0),
		new Vector3(0, -1.10f, 0),
		new Vector3(0, -1.0f, 0)
	};
	private Vector3[] roundCircle = {
		Vector3.up * 2,
		Vector3.left,
		Vector3.down * 2,
		Vector3.right
	};
	public GameObject[] Sounds;
	private List<GameObject> doneSounds = new List<GameObject>();
	private bool forceChange = false;
	private float cameraZoom = -6.0f;
	public GameObject MainCam;
	private Vector3 MainCamInitial;
	private bool calibrated = false;
	
	public ParticleSystem[] Particles;
	
	public GameObject BlackHoleParent;
	private List<ParticleSystem> BlackHole;
	
	private float blackHoleTime;
	private const float blackHoleAnimDuration = 3.5f;
	private const float blackHoleMaxSizeDuration = 10.0f; 
	
	public Transform calibMenu;
	
	public Transform instructions;
	
	
	// Use this for initialization
	void Start () {
		ChangeMood("Happy");
		MainCamInitial = MainCam.transform.position;
		BroadcastMessage("SetDolls", this.Dolls, SendMessageOptions.DontRequireReceiver);
		foreach(var txt in instructions.GetComponentsInChildren<Transform>().Where(x => x != instructions) ){
			txt.gameObject.renderer.sharedMaterial.color = Color.black;
		}
		foreach(var ptcl in Particles){
			ptcl.renderer.enabled = false;
		}
		BlackHole = BlackHoleParent.GetComponentsInChildren<ParticleSystem>().ToList();
		BlackHole.ForEach(delegate(ParticleSystem p){
			p.renderer.enabled = false;
		});
		iTween.FadeTo(instructions.gameObject, 0.0f, 0.1f);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)){
			if (!mousePressed && !transitioning && finishedDolls < Dolls.Length && calibrated) {
				iTween.FadeTo(instructions.gameObject, iTween.Hash(
					"alpha", 0.0f,
					"time", 1.0f,
					"delay", 1.0f
				));
				transitioning = true;
				BroadcastMessage("SetInZero", SendMessageOptions.DontRequireReceiver);
				//BroadcastMessage("Transitioning", SendMessageOptions.DontRequireReceiver);
				iTween.RotateAdd(GetDollTop(), new Vector3(0,45,0), 1.0f);
				iTween.MoveTo(MainCam, iTween.Hash(
					"position", MainCamInitial + Vector3.forward * cameraZoom,
					"time", 0.4f
				));
				iTween.RotateAdd(GetDollBottom(), iTween.Hash(
					"amount", new Vector3(0,-45,0),
					"time", 1.0f,
					"delay", 1.0f,
					"oncomplete", "MaybeShowBlackHole",
					"oncompletetarget", this.gameObject
				));
				iTween.MoveAdd(GetDollTop(), iTween.Hash(
					"amount", topDollOpenings[finishedDolls],
					"time", 1.0f,
					"delay", 2.0f
				));
				iTween.MoveAdd(GetDollBottom(), iTween.Hash(
					"amount", bottomDollOpenings[finishedDolls],
					"time", 1.0f,
					"delay", 2.0f
				));
				iTween.MoveAdd(Dolls[finishedDolls], iTween.Hash(
					"amount", dollPositions[finishedDolls],
					"time", 1.0f,
					"delay", 3.0f
				));
				iTween.MoveAdd(GetDollTop(), iTween.Hash(
					"amount", topDollOpenings[finishedDolls] * -1,
					"time", 1.0f,
					"delay", 4.0f
				));
				iTween.MoveAdd(GetDollBottom(), iTween.Hash(
					"amount", bottomDollOpenings[finishedDolls] * -1,
					"time", 1.0f,
					"delay", 4.0f
				));
				iTween.RotateAdd(GetDollTop(), iTween.Hash(
					"amount", new Vector3(0,-45,0),
					"time", 1.0f,
					"delay", 5.0f
				));
				iTween.RotateAdd(GetDollBottom(), iTween.Hash(
					"amount", new Vector3(0,45,0),
					"time", 1.0f,
					"delay", 5.0f,
					"oncomplete", "SetTransitioning",
					"oncompletetarget", this.gameObject
				));
				doneSounds.Add(GetSound(Current.Name));
				finishedDolls++;
				foreach(GameObject sound in doneSounds) {
					sound.audio.volume = 1.0f;
				}
			}
			mousePressed = true;
		} else {
			mousePressed = false;
		}
		if(finishedDolls < Dolls.Length) {
			var colorMul = Mathf.Sin(Time.fixedTime * 4.0f) * 0.2f + 1.0f;
			GetDollBottom().renderer.sharedMaterial.color = Cylinders[finishedDolls].renderer.sharedMaterial.color * new Color(colorMul, colorMul, colorMul);
		}
		if(BlackHole.First().renderer.enabled){
			var elapsedTime = Time.fixedTime - blackHoleTime;
			if (elapsedTime > blackHoleAnimDuration){
				if(elapsedTime <= blackHoleAnimDuration + blackHoleMaxSizeDuration) {
					elapsedTime = blackHoleAnimDuration;
				} else {
					elapsedTime = Mathf.Max(0.0f, 2*blackHoleAnimDuration + blackHoleMaxSizeDuration - elapsedTime);
				}
			}
			elapsedTime = elapsedTime / blackHoleAnimDuration;
			BlackHole.ForEach(delegate(ParticleSystem p){
				p.gameObject.SendMessage("SetSize", elapsedTime);
			});
		}
	}
	private GameObject GetSound(string mood) {
		return Sounds.Single(x => x.name == "0" + (finishedDolls + 1) + mood);
	}
	
	private void MaybeShowBlackHole(){
		if(!this.BlackHole.First().renderer.enabled) {
			if(finishedDolls == Dolls.Length){
				this.BlackHole.ForEach(delegate(ParticleSystem p) {
					p.renderer.enabled = true;
				});
				blackHoleTime = Time.fixedTime;
			}
		} else {
			this.BlackHole.ForEach(delegate(ParticleSystem p) {
				p.renderer.enabled = false;
			});
		}
	}
	
	public void SetTransitioning() {
		if(finishedDolls < Dolls.Length) {
			BroadcastMessage("SetDoll", Dolls[finishedDolls-1], SendMessageOptions.DontRequireReceiver);
			Cylinders[finishedDolls].SetActive(true);
			foreach(GameObject sound in doneSounds){
				sound.audio.volume = 0.0f;
			}
			transitioning = false;
			forceChange = true;
			ChangeMood("Happy");
			forceChange = false;
			iTween.MoveTo(MainCam, iTween.Hash(
				"position", MainCamInitial,
				"time", 0.4f
			));
		} else {
			foreach(var ptcl in Particles) {
				ptcl.renderer.enabled = true;
			}
			float delay = 0.0f;
			int danceTimes = 20;
			float danceSpeed = 0.5f;
			float danceDelay = danceSpeed * (float)danceTimes;
			MakeRound(danceDelay);
			for(int i = 0; i < Dolls.Length; i++) {
				var doll = Dolls[i];
				Dance(danceTimes, danceSpeed, doll);
				
				delay = (3 - i) * 3.0f;
				iTween.RotateAdd(GetDollTop(i), iTween.Hash(
					"amount", new Vector3(0,45,0),
					"time", 1.0f,
					"delay", 1.0f + danceDelay
				));
				iTween.RotateAdd(GetDollBottom(i), iTween.Hash(
					"amount", new Vector3(0,-45,0),
					"time", 1.0f,
					"delay", 2.0f + danceDelay
				));
				iTween.MoveAdd(GetDollTop(i), iTween.Hash(
					"amount", topDollOpenings[i],
					"time", 1.0f,
					"delay", 3.0f + danceDelay
				));
				iTween.MoveAdd(GetDollBottom(i), iTween.Hash(
					"amount", bottomDollOpenings[i],
					"time", 1.0f,
					"delay", 3.0f + danceDelay
				));
				iTween.MoveAdd(doll, iTween.Hash(
					"amount", dollPositions[i] * -1.0f,
					"time", 1.0f,
					"delay", 4.0f + delay + danceDelay
				));
				iTween.MoveAdd(GetDollTop(i), iTween.Hash(
					"amount", topDollOpenings[i] * -1,
					"time", 1.0f,
					"delay", 5.0f + delay + danceDelay
				));
				iTween.MoveAdd(GetDollBottom(i), iTween.Hash(
					"amount", bottomDollOpenings[i] * -1,
					"time", 1.0f,
					"delay", 5.0f + delay + danceDelay,
					"oncomplete", (i == Dolls.Length - 1) ? "MaybeShowBlackHole" : "DoNothing",
					"oncompletetarget", this.gameObject
				));
				iTween.RotateAdd(GetDollTop(i), iTween.Hash(
					"amount", new Vector3(0,-45,0),
					"time", 1.0f,
					"delay", 6.0f + delay + danceDelay
				));
				iTween.RotateAdd(GetDollBottom(i), iTween.Hash(
					"amount", new Vector3(0,45,0),
					"time", 1.0f,
					"delay", 6.0f + delay + danceDelay
				));
			}
			iTween.RotateBy(Dolls[0].transform.parent.gameObject, iTween.Hash(
				"amount", new Vector3(0,5,0),
				"time", 5.0f,
				"delay", 17.0f + danceDelay,
				"easetype", iTween.EaseType.easeInOutQuad
			));
			iTween.MoveTo(MainCam, iTween.Hash(
				"position", MainCamInitial + Vector3.back * cameraZoom / 4,
				"time", 2.0f,
				"delay", 17.0f + danceDelay + 3.0f,
				"easetype", iTween.EaseType.easeInQuad,
				"oncomplete", "MuteSounds",
				"oncompletetarget", this.gameObject
			));
			foreach(var s in doneSounds){
				iTween.AudioTo(s, iTween.Hash(
					"volume", 1.0f,
					"pitch", 3.5f,
					"time", 8.0f,
					"delay", 17.0f + danceDelay,
					"oncomplete", "Restart",
					"oncompletetarget", this.gameObject
				));
			}
		}
		transitioning = false;
		//BroadcastMessage("DoneTransition", SendMessageOptions.DontRequireReceiver);
	}
	
	public void Restart() {
		Application.LoadLevel(0);
	}
	
	private void DoNothing() {
	}
	
	private void MuteSounds() {
		foreach(var sound in this.Sounds){
			sound.audio.Stop();
		}
	}
	
	private void Dance(int times, float speed, GameObject obj) {
		int angle = 30;
		int even = 1;
		for(int i = 0; i < times; i++){
			iTween.RotateAdd(obj, iTween.Hash(
				"amount", new Vector3(0, 0, angle * even * (i == 0 || i == times - 1 ? 1 : 2)),
				"time", speed,
				"easeType", iTween.EaseType.easeInOutQuad,
				"delay", i * speed
			));
			even *= -1;
		}
	}
	
	private void MakeRound(float danceDelay){
		float sectionTime = danceDelay / (float)Dolls.Length;
		for(int i = 0; i < Dolls.Length; i++){
			for(int round = 0; round < Dolls.Length; round++){
				var newPos = Dolls[(i+round+1)%Dolls.Length].transform.localPosition;
				iTween.MoveTo(Dolls[i], iTween.Hash(
					"position", new Vector3(newPos.x, newPos.y, Dolls[i].transform.localPosition.z),
					"time", sectionTime,
					"delay", sectionTime * round,
					"easetype", iTween.EaseType.linear,
					"islocal", true
				));
			}
		}
	}
	
	public void ChangeMood(string mood) {
		int finishedLocal = finishedDolls;
		if(forceChange || (!transitioning && finishedLocal < Dolls.Length && (Current == null || Current.Name != mood))) {
			if(Current != null) iTween.AudioTo(GetSound(Current.Name), 0.0f, 1.0f, 1.0f);
			IEnumerable<Mood> EnumMoods = from ms in Moods select ms;
			Current = EnumMoods.Single(m => m.Name == mood);
			iTween.ColorTo(Cylinders[finishedLocal],
				getColor(),
				1.0f);
			GetDollTop().renderer.sharedMaterial = DollMaterials.Single(x => x.name == "0" + (finishedLocal + 1) + Current.MaterialName);
			iTween.AudioTo(GetSound(mood), 1.0f, 1.0f, 1.0f);
			this.Particles[finishedLocal].startColor = Current.CylColor;
		}
	}
	
	private Color getColor(){
		var color = Current.CylColor;
		var power = finishedDolls * 0.1f;
		return new Color(color.r - power, color.g - power, color.b - power);
	}
	
	private GameObject GetDollTop() {
		return Dolls[finishedDolls].GetComponentsInChildren<Transform>()[2].gameObject;
	}
	private GameObject GetDollBottom() {
		return Dolls[finishedDolls].GetComponentsInChildren<Transform>()[1].gameObject;
	}
	
	private GameObject GetDollTop(int i) {
		return Dolls[i].GetComponentsInChildren<Transform>()[2].gameObject;
	}
	private GameObject GetDollBottom(int i) {
		return Dolls[i].GetComponentsInChildren<Transform>()[1].gameObject;
	}
	
	public void Calibrated(){
		this.calibrated = true;
		this.mousePressed = true;
		iTween.FadeTo(calibMenu.gameObject, 0.0f, 1.0f);
		iTween.FadeTo(instructions.gameObject, iTween.Hash(
			"alpha", 1.0f,
			"time", 1.0f,
			"delay", 1.0f
		));
	}
}

public class Mood {
	public string Name;
	public Color CylColor;
	public string MaterialName;
	
	public Mood(string n, Color cc, string mn) {
		this.Name = n;
		this.CylColor = cc;
		this.MaterialName = mn;
	}
}
