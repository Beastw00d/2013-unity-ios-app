using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

using U3DXT.iOS.Native.Internals;
using U3DXT.iOS.GameKit;
using U3DXT.iOS.Native.GameKit;
using U3DXT.iOS.Social;
using U3DXT.iOS.Native.Foundation;
using U3DXT.iOS.Native.UIKit;
using U3DXT.iOS.Native.Social;
using U3DXT.iOS.UserMedia;
using U3DXT.Utils;
using U3DXT.Core;


public class GameSettings
{
	// holds important game wide variables such as ad settings and game version and handles interaction with game center

	public static int hasAds;
	public static int gameVersion;
	public static int globalVolume = -1;
	public static int isConnected = 0;


	// ------------------------------------------------------------------


	// make an initial connection to game center
	public static bool ConnectToGameCenter()
	{
		if (CoreXT.IsDevice)
		{
			SocialXT.PostCompleted += OnPostCompleted;
			
			GameKitXT.LocalPlayerAuthenticated += OnLocalPlayerAuthenticated;
			GameKitXT.LocalPlayerAuthenticationFailed += OnLocalPlayerAuthenticationFailed;
			GameKitXT.AuthenticateLocalPlayer();

			// if SL Requests cannot be made on the device, turn off the social buttons for this user
			if (U3DXT.iOS.Native.Social.SLRequest.ClassExists == false)
				return false;
			else
				return true;
		}

		else
			return false;
	}


	// called upon game kit successfully authenticating player
	static void OnLocalPlayerAuthenticated(object sender, EventArgs e)
	{
		isConnected = 1;
	}


	// called when gamekit fails to authenticate player
	static void OnLocalPlayerAuthenticationFailed(object sender, EventArgs e)
	{
		isConnected = 0;
	}


	// ----------


	// called when the user attempts to make a social post, and passes the int 1 for twitter, 2 for facebook
	public static void SendSocialPost(int socialInt)
	{
		if (socialInt == 1)
			SocialXT.Post(SLRequest.SLServiceTypeTwitter, "Fighting off waves of infection in SAVING TIMMY for iOS!", null, null);
		else if (socialInt == 2)
			SocialXT.Post(SLRequest.SLServiceTypeFacebook, "Fighting off waves of infection in SAVING TIMMY for iOS!", null, null);
	}


	// upon a successful social post
	static void OnPostCompleted(object sender, PostCompletedEventArgs e)
	{
	}


	// ----------


	// called when a player attempts to open the leaderboard
	public static void ViewLeaderboard()
	{
		if (isConnected == 1)
			GameKitXT.ShowLeaderboard("ZoneReached");
	}


	// called when a player attempts to view the game's achievements
	public static void ViewAchievements()
	{
		if (isConnected == 1)
			GameKitXT.ShowAchievements();
	}

	// ----------

	// called upon player game over.  Sends the user's current score to gamecenter
	public static void SendHighScore(int score)
	{
		// store the player's new high score on gamecenter
		if (isConnected == 1)
		{
			if (GameKitXT.localPlayer != null)
				GameKitXT.ReportScore("InfectionsKilled", score);
		}
	}
}
