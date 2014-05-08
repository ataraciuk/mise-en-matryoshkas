/**
 * OpenTSPS + Unity3d Extension
 * Created by James George on 11/24/2010
 * 
 * This example is distributed under The MIT License
 *
 * Copyright (c) 2010 James George
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TSPS;
using System.Linq;

public class OpenTSPSUnityListener : MonoBehaviour  {
	
	private float mouthHeight;
	private float mouthWidth;
	private float jaw;
	private int found = 0;
	private float smileThreshold = 1.2f;
	private float surpriseThreshold = 2.5f;
	private float sadThreshold = 0.97f;
	
	private float calibWidth = -1.0f;
	private float calibHeight = -1.0f;
	private float calibJaw = -1.0f;
	private bool calibrated = false;
	
	public Transform SorryTxt;
	
	public void OnEnable(){
		UnityOSCReceiver.OSCMessageReceived += new UnityOSCReceiver.OSCMessageReceivedHandler(OSCMessageReceived);
		
	}
	public void OnDisable(){
		UnityOSCReceiver.OSCMessageReceived -= new UnityOSCReceiver.OSCMessageReceivedHandler(OSCMessageReceived);
		
	}
	
	public void OSCMessageReceived(OSC.NET.OSCMessage message){
		
		string address = message.Address;
		ArrayList args = message.Values;
		
		if(address == "/found") {
			found = (int)args[0];
		} else if (address == "/gesture/mouth/height") {
			mouthHeight = (float)args[0];
		} else if(address == "/gesture/mouth/width"){				
			mouthWidth = (float)args[0];
		} else if(address == "/pose/scale"){
			BroadcastMessage("UpdateScale", float.Parse( args[0].ToString()),  SendMessageOptions.DontRequireReceiver);
		} else if(address == "/pose/orientation"){
			//Debug.Log(string.Join("    ", args.ToArray().Select(x => x.ToString()).ToArray() ));
			BroadcastMessage("UpdateOrientation", new Vector3((float)args[0],(float)args[1],(float)args[2]),  SendMessageOptions.DontRequireReceiver);
		} else if(address == "/gesture/jaw") {
			jaw = (float)args[0];
		} else if(address == "/pose/position") {
			//Debug.Log ((float)args[0] + " " + (float)args[1]);
		}
		else{
			//print(address + " ");
		}
		if(calibrated){
			string mood = "";
			if(found > 0) {
				if(mouthHeight > 4.0f) {
					mood = "Surprised";
				} else if(mouthWidth > calibWidth * smileThreshold){
					mood = "Happy";
				} else if(mouthHeight < 2.5f && mouthWidth < calibWidth * sadThreshold) {
					mood = "Sad";
				}
			} else {
				mood = "Shy";
			}
			
			if(mood != "") {
				BroadcastMessage("ChangeMood", mood, SendMessageOptions.DontRequireReceiver);
			}
		}
		else{
			calibHeight = mouthHeight;
			calibWidth = mouthWidth;
			calibJaw = jaw;
		}
		//Debug.Log("mouth width: "+mouthWidth+"    mouth height: "+mouthHeight);
	}
	void Update() {
		if(Input.GetMouseButtonDown(0) && !calibrated){
			if(calibJaw > 0.0f && calibHeight > 0.0f && calibWidth > 0.0f){
				calibrated = true;
				BroadcastMessage("Calibrated", SendMessageOptions.DontRequireReceiver);
			} else {
				this.SorryTxt.localPosition = new Vector3(-1.0f, -0.46f, 1.55f);
			}
		}
	}
}
