using UnityEngine;
using System.Collections;
	
public class PlayerStats
{
	// RRR
	public static bool isAlive;

	public static int health = 3;

	public static SpriteRenderer[] hearts ;
	

	public int GetHealth() 
	{
		return health;
	}
		
	public void SetHealth(int healthAmount) 
	{
		health = healthAmount;
	}
		
	//-- will subtract one from the health and return the new value
	public static int LoseHealth()
	{
		--health;
		if(health > -1) PlayerControls.hearts[health].color = new Color(0f,0f,0f,.3f);
		if(health < 1 && isAlive) isAlive = false;
		return health;
	}

	//-- will subtract the hitpoints from the current health and return the value
	public static int LoseHealth(int hitPoints)
	{
		health = health - hitPoints;
		if(health > -1) PlayerControls.hearts[health].color = new Color(0f,0f,0f,.3f);
		if(health < 1 && isAlive) isAlive = false;
		return health;
	}	
}

