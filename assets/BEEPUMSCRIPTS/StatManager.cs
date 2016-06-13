using UnityEngine;
using System.Collections;

public class StatManager : MonoBehaviour 
{
	// this script is in charge of managing various player statistics, achievements, and unlockables

	public static BitArray achievements = new BitArray(14, false);
	public static int highScore = -1;
	
	// ---------------------------------


	// called upon the player loading into the game in order to retrieve the player's high score from gamecenter
	public static void GetGameData()
	{
		// TODO: code here to get the high score AND achievement status from gamecenter for the current player
		
		highScore = 1;
	}


	// called by the menu manager to figure out which ships are locked and unlocked based on user's achievements
	public static bool GetLocks(int index)
	{
		if (index == 1)	return !achievements[4];
		else if (index == 2) return !achievements[10];
		else if (index == 3) return !achievements[8];
		else return true;
	}

	// called by another script in order to unlock the achievement in the specified index
	public static void UnlockAchievement(int index)
	{
		// if the achievement is already earned and is NOT index of 0 (unlocked all characters), then it is repeatable
		if (achievements[index] == true && index != 0)
		{
			// TODO: code here for repeatable achievement within game center
		}

		// else if the achievement was not earned, then earn it for the first time
		else if (achievements[index] == false)
		{
			// TODO: code here to tell gamecenter that the achievement was unlocked

			achievements[index] = true;

			// check if this achievement unlocked the final ship model for this player
			if ( !achievements[0] && achievements[4] && achievements[10] && achievements[8] )
			{
				// TODO: code here to tell gamecenter that achievement 0 (all ships unlocked) was completed by this achievement

				achievements[0] = true;
			}
		}
	}


	// called by the levelController upon a player dying in order to store a new high score
	public static void SetHighScore(int score)
	{
		highScore = score;

		// TODO: code here to store the high score on gamecenter
	}
}
