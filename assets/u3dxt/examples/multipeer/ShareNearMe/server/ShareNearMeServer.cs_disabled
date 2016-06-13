using UnityEngine;
using System.Collections;
using U3DXT.iOS.Multipeer;
using U3DXT.iOS.Native.MultipeerConnectivity;
using System.IO;

public class ShareNearMeServer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		MultipeerXT.SessionChanged += SessionChanged;
	}
	
	void OnDestroy () {
		MultipeerXT.SessionChanged -= SessionChanged;
	}
	
	void OnGUI() 
	{
		KitchenSink.OnGUIBack();
	}

	private MCSession _session;
	
	private void SessionChanged(object sender, SessionChangedEventArgs e) {
		Debug.Log ("Session Changed " + e.state);

		if (e.state == U3DXT.iOS.Native.MultipeerConnectivity.MCSessionState.Connected )
		{
			Debug.Log("CONNECTED");
		
			string path = Application.temporaryCachePath+"/someFile.bin";
			// create some arbitary binary data. or load from existing location
			FileStream someFile = new FileStream(path, FileMode.Create);
			someFile.WriteByte(0x42);
			someFile.Close();
			
path = "https://www.google.com/images/srpr/logo4w.png";
			_session = e.session;
			_session.SendResourceAtURL(new U3DXT.iOS.Native.Foundation.NSURL(path), "someFile.bin", e.peerID,  sendResourceCompleted);
		}
	}
	
	private void sendResourceCompleted(U3DXT.iOS.Native.Foundation.NSError err)
	{
		if ( err != null )
			Debug.Log("ERROR: " + err.LocalizedDescription());
	}
}
