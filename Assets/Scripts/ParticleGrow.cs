using UnityEngine;
using System.Collections;

public class ParticleGrow : MonoBehaviour {
	
	private float minSize;
	private float maxSize;
	
	// Use this for initialization
	void Start () {
		minSize = this.particleSystem.startSize;
		maxSize = minSize * 4.0f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void SetSize(float perctentage){
		this.particleSystem.startSize = Mathf.Lerp(minSize, maxSize, perctentage);
	}
}
