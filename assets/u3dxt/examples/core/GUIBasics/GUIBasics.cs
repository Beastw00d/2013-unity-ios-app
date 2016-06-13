using System;
using System.Collections;
using UnityEngine;
using U3DXT.Core;
using U3DXT.iOS.Native.Foundation;
using U3DXT.iOS.Native.UIKit;
using U3DXT.Utils;
using U3DXT.iOS.Native.Internals;
using System.IO;
using System.Text;
using System.Linq;
using U3DXT.iOS.GUI;

public class GUIBasics : MonoBehaviour {
	
	void Start() {
		if (CoreXT.IsDevice) {
			// subscribes to events
			GUIXT.AlertDismissed += OnAlertDismissed;
			
			GUIXT.MediaPicked += OnMediaPicked;
			GUIXT.MediaPickCancelled += OnMediaCancelled;
		}
	}
	
	void OnDestroy() {
		if (CoreXT.IsDevice) {
			// unsubscribe to events
			GUIXT.AlertDismissed -= OnAlertDismissed;
			
			GUIXT.MediaPicked -= OnMediaPicked;
			GUIXT.MediaPickCancelled -= OnMediaCancelled;
		}
	}
	
	void OnAlertDismissed(object sender, AlertViewDismissedEventArgs e) {
		Log("Alert view dismissed with button: " + e.selectedButtonTitle + " and inputs: " + e.inputString1 + ", " + e.inputString2);
	}
	
	void OnMediaPicked(object sender, MediaPickedEventArgs e) {
		Log("Image picked: " + e.image);
		
		// expensive process to convert to texture 2d and upload to GPU
		//Texture2D texture = e.image.ToTexture2D();
	}
	
	void OnMediaCancelled(object sender, EventArgs e) {
		Log("Image pick cancelled.");
	}
	
	void ShowBatteryInfo() {
		UIDevice device = UIDevice.CurrentDevice();
		device.batteryMonitoringEnabled = true; // need to enable this first
		
		Log("Battery state: " + device.batteryState);
		Log("Battery level: " + device.batteryLevel);
	}
	
	void OnGUI() {
		
		KitchenSink.OnGUIBack();
		
		if (CoreXT.IsDevice) {
			
			GUILayout.BeginArea(new Rect(50, 50, Screen.width - 100, Screen.height/2 - 50));
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Show alert", GUILayout.ExpandHeight(true))) {
				GUIXT.ShowAlert("Alert title", "Alert message", "Cancel", new string[] { "OK", "Hi", "Hello" });
			}
			
			if (GUILayout.Button("Show input", GUILayout.ExpandHeight(true))) {
				GUIXT.ShowAlert("Input Prompt", "Enter something", "Cancel", new string[] {"OK"}, UIAlertViewStyle.PlainTextInput);
			}
			
			if (GUILayout.Button("Pick image from photo library", GUILayout.ExpandHeight(true))) {
				GUIXT.ShowImagePicker();
			}
			
			if (GUILayout.Button("Take photo from camera", GUILayout.ExpandHeight(true))) {
				GUIXT.ShowImagePicker(UIImagePickerControllerSourceType.Camera);
			}
			
			if (GUILayout.Button("Show battery info", GUILayout.ExpandHeight(true))) {
				ShowBatteryInfo();
			}

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
		
		OnGUILog();
	}


#region Debug logging
	string _log = "Debug log:";
	Vector2 _scrollPosition = Vector2.zero;
	
	void OnGUILog() {
		GUILayout.BeginArea(new Rect(50, Screen.height / 2, Screen.width - 100, Screen.height / 2 - 50));
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.alignment = TextAnchor.UpperLeft;
		GUILayout.Box(_log, GUILayout.ExpandHeight(true));
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
	
	void Log(string str) {
		_log += "\n" + str;
		_scrollPosition.y = Mathf.Infinity;
	}
#endregion
}
