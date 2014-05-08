using UnityEngine;
using System.Collections;

public class SwitchToCamera : MonoBehaviour {
	
	public GameObject OtherCam;
	private const float period = 15.0f;
	private float lastSwtich;
	
	// Use this for initialization
	void Start () {
		lastSwtich = Time.fixedTime;
	}
	
	// Update is called once per frame
	void Update () {
		var curr = Time.fixedTime;
		if(curr >= lastSwtich + period){
			OtherCam.SetActive(!OtherCam.activeSelf);
			lastSwtich = curr;
		}
	}
}