using UnityEngine;
using System.Collections;

public static class GameMode
{
	// static class to manage various game-wide variables of interest

	public static int gameVersion;
	public static bool hasAds;
	public static int curPlayers = 0;
	public static int selectedShip = 1;
	public static int globalVolume = -1;
	public static int startingLevel = 0;
	public static int currentLevel = 0;
	public static int lastPlayTime = 0;
	public static int controlModeInt = 0;
}