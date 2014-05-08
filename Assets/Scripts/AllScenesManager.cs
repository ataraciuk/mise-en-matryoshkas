using UnityEngine;
using System.Collections;

public class AllScenesManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
		DontDestroyOnLoad(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.L)) Screen.lockCursor = !Screen.lockCursor;
	}
}
