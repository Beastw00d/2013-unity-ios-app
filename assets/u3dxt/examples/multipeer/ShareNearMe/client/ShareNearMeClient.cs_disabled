using UnityEngine;
using System.Collections;
using U3DXT.iOS.Multipeer;
using System.IO;
/// <summary>
/// Share near me client. When connected, it will listen for a file to be sent to it.
/// </summary>
public class ShareNearMeClient : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	
		MultipeerXT.BrowserCompleted += BrowserCompleted;
		MultipeerXT.SessionStartedReceivingResourceWithName += StartReceivingResourceWithName;
		MultipeerXT.SessionFinishedReceivingResourceWithName += FinishedReceivingResourceWithName;
		MultipeerXT.SessionChanged += SessionChanged;
	}
	
	void OnDestroy () {
	
		MultipeerXT.BrowserCompleted -= BrowserCompleted;
		MultipeerXT.SessionStartedReceivingResourceWithName -= StartReceivingResourceWithName;
		MultipeerXT.SessionFinishedReceivingResourceWithName -= FinishedReceivingResourceWithName;
		MultipeerXT.SessionChanged -= SessionChanged;
	}
	
	void OnGUI() 
	{
		KitchenSink.OnGUIBack();
	}
	
	private void FinishedReceivingResourceWithName(object sender, SessionFinishedReceivingResourceWithNameEventArgs e) {
		Debug.Log ("Finished Resource With Name " + e.resourceName);
		Debug.Log ("File Written to: " + e.localURL.Path ());
		byte[] bytes = File.ReadAllBytes( e.localURL.Path() );
		Debug.Log ("Contents of File: " + bytes);

	}
	
	private void StartReceivingResourceWithName(object sender, SessionStartedReceivingResourceWithNameEventArgs e) {
		Debug.Log ("Receiving Resource With Name" + e.resourceName);
	}
	
	
	private void BrowserCompleted(object sender, System.EventArgs e)
	{
		Debug.Log ("Browser Completed");
			
		
	}
	
	private void SessionChanged(object sender, SessionChangedEventArgs e) {
		Debug.Log ("Session Changed " + e.state);
		if ( e.state == U3DXT.iOS.Native.MultipeerConnectivity.MCSessionState.Connected )
		{
			Debug.Log ("Session Connected");
		}
	}

}
