using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Hum : MonoBehaviour {
	
	private float Speed = 1.0f;
	private float XRadius = 0.25f;
	private float YRadius = 0.5f;
	private Vector3 StartPos;
	private bool doIt = true;
	private bool done = false;
	private float XMult = 1.0f;
	private float lastTime = -1.0f;
	private float XOffset = 0.0f;
	private Vector3 RotationMult = new Vector3(-50.0f, -100.0f, 50.0f);
	private List<GameObject> DoneDolls = new List<GameObject>();
	private IEnumerable<GameObject> Dolls;
	private bool transition = false;
	
	// Use this for initialization
	void Start () {
		DoneDolls.Add(this.gameObject);
		StartPos = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if(doIt && !done) {
			var currTime = Mathf.Repeat(Time.fixedTime, 2.0f * Mathf.PI) + Mathf.PI / 2;
			if(currTime < lastTime) {
				XMult *= -1.0f;
				XOffset = XOffset == 0.0f ? 2 * XRadius : 0;
			}
			lastTime = currTime;
			var currVect =  new Vector3(Mathf.Sin(currTime * Speed) * XRadius * XMult + XOffset, Mathf.Cos(currTime * Speed) * YRadius, 0);
			this.transform.position = StartPos + currVect;
		}
	}
	
	void UpdateOrientation(Vector3 orientation){
		if(!transition) {
			foreach (var doll in Dolls.Where(x => !DoneDolls.Contains(x))) {
				iTween.RotateTo( doll, new Vector3(RotationMult.x * orientation.x, RotationMult.y * orientation.y, RotationMult.z * orientation.z), 0.2f);
			}
		}
	}
	
	void SetDoll(GameObject doll){
		this.DoneDolls.Add(doll);
		this.transition = false;
	}
	
	void SetInZero() {
		this.transition = true;
		this.Dolls.ToList().ForEach(x => iTween.RotateTo(x.gameObject, Vector3.zero, 0.2f));
	}
	
	void SetDolls(IEnumerable<GameObject> dolls) {
		this.Dolls = dolls;
	}
	
	/*
	void Transitioning() {
		doIt = false;
	}
	
	void DoneTransition() {
		doIt = true;
	}
	*/
}
