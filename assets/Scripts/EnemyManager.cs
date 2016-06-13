using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour 
{
	// class in charge of managing enemies, enemy spawns, and the advancing of levels over time

	// enemy pool location
	public static Vector3 enemyHome = new Vector3(-20,20,0);

	// pool of enemy units (in transform and script form) and index of the next enemy to be used
	public Transform[] enemyTransArr;
	public Enemy[] enemyScriptArr;
	private int enemyIndex;

	// array of enemy spawners and default scale size
	public Transform[] enemySpawnArr;
	private Vector3 defScale;

	// move speed of an enemy, to be set when an enemy is moved to a spawn
	public float enemySpeed;

	// script of the healthy cell used to give the player an opportunity to heal once every X waves
	public HealthyCell healthyCell;

	// current level number and gui text to display it
	private int levelNum = 0;
	public Transform levelNumTrans;
	public SpriteRenderer levelNumARend, levelNumBRend;
	public Sprite[] numSprites;

	// starting amount - 1
	public int  waveLives = 19;

	// spawn rate for enemies (how often a spawner can emit an enemy unit)
	public float spawnRate;

	// audios to play upon a spawner opening intially and when spawning a unit
	public AudioClip openSpawnerAudio, spawnAudio;

	// variables to adjust the frequency (speed) and amplitude (amount) that a spawner visually wobbles when spawning a unit
	public float wobbleFrequency, wobbleAmplitude;

	// ints used as the upper bound for rolling to check enemy spawns
	private int rollLower = 100, rollUpper = 410;

	private float speedInc = .55f;

	public PlayerControls playerControls;

	// RRR
	public static int playerScore = 0;

	public SpriteRenderer scoreNumARend, scoreNumBRend, scoreNumCRend, scoreNumDRend;

	// ------------------------------------


	void Start()
	{
		defScale = enemySpawnArr[0].localScale;

		for (int i = 0 ; i < enemySpawnArr.Length ; i++)
		{
			enemySpawnArr[i].localScale = Vector3.zero;
		}

		StartCoroutine("EnemySpawner");
	}


	// main loop for spawning enemy waves over time
	IEnumerator EnemySpawner()
	{
		// wait for scene fader to finish fading in
		while (SceneFader.isFading)
			yield return null;

		// open up the spawners visually
		for (int i = 0 ; i < enemySpawnArr.Length ; i++)
		{
			StartCoroutine("OpenSpawner", i);
			yield return new WaitForSeconds(0.5f);
		}

		// bool arr where values are true when an enemy should spawn from the given spawner index
		bool[] openSpawns;

		// main loop for spawning waves (levels) of enemies
		while (PlayerStats.health > 0 )
		{
			//-- give time for all enemies to be destroyed
			yield return new WaitForSeconds(.75f);

			//------------------------------------------------------------------------------------------------------------

			//-- HEALTHY CELL 

			// this int is 0 if the wave does NOT spawn a healthy cell and a positive int to indicate which wave a healthy cell will spawn in
			int healthyInt = 0;

			if ( (levelNum + 1) % 5 == 0 )
				healthyInt = Random.Range(1, 11);

			//------------------------------------------------------------------------------------------------------------

			//-- DIFFICULTY AMP

			if (levelNum  < 1)
			{
				enemySpeed = 3f;
			}
			else if (levelNum  < 2)
			{
				enemySpeed = 4.0f;				
			}
			else if (levelNum  < 3)
			{
				enemySpeed = 4.8f;				
			}
			else if (levelNum  < 4)
			{
				enemySpeed = 5.5f;				
			}
			else if (levelNum == 4)
			{
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
			}
			else if(levelNum == 6) 
			{
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
			}
			else if (levelNum == 10)
			{
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
				//enemySpeed += speedInc;
				//speedInc = speedInc / 1.075f;
				//enemySpeed += speedInc;
				//speedInc = speedInc / 1.075f;
			}
			else if (levelNum == 13)
			{
				spawnRate += .05f;
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
			}
			else if (levelNum == 16)
			{
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
			}
			else if (levelNum == 19)
			{
				spawnRate += .05f;
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
			}
			else if (levelNum == 22)
			{
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
			}
			else if (levelNum == 25)
			{
				spawnRate += .05f;
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
			}
			else if (levelNum > 25 && levelNum % 4 == 0)
			{
				enemySpeed += speedInc;
				speedInc = speedInc / 1.075f;
			}

			//------------------------------------------------------------------------------------------------------------

			// increase level number and visually show it
			levelNum++;
		
			int onesDigit = levelNum % 10;
			int tensDigit = (levelNum - onesDigit) / 10;
			
			levelNumARend.sprite = numSprites[tensDigit];
			levelNumBRend.sprite = numSprites[onesDigit];

			//-- increase the number of enemies to spawn by one for the next wave
			waveLives++;

			Vector3 defLevelScale = levelNumTrans.localScale;

			// visual effect during level advance uses same sin wave technique as spawner wobble
			for (float tA = 0 ; tA < 0.4f ; tA += Time.deltaTime / 2)
			{
				float wobble = Mathf.Sin(tA * wobbleFrequency) * (wobbleAmplitude / 4) * (0.4f - tA);
				levelNumTrans.localScale = new Vector3(defLevelScale.x + wobble, defLevelScale.y + wobble, 0);
				yield return null;
			}

			// ints for number of waves and number of individual enemies spawned
			int waves = 0;
			int n = 0;

			// while loop that rolls once per spawn rate increment to see if and where enemies will spawn. This repeats until it has been going for the set waveTime
			while (n < waveLives && PlayerStats.health > 0)
			{
				waves++;
				openSpawns = GetOpenSpawns();

				//-- search to see how many enemies are going to spawn
				for (int i = 0; i < 4; i++) 
				{
					//-- adds one if the value is true
					n = (openSpawns[i] == true)? n+1 : n;

					//-- if the max enemies have spawned, stop more from spawning
					if(n == waveLives && i < 3)
					{
						for(int x = i+1 ; x < 4; x++)
						{
							openSpawns[x] = false;
						}
					}
				}

				// if this wave needs to spawn a healthy cell, make sure it does not have 4 infected cells in it
				if (waves == healthyInt)
				{
					int randIndex = Random.Range (0,4);

					if (openSpawns[randIndex] == true)
					{
						openSpawns[randIndex] = false;
						n--;
					}

					healthyCell.SpawnCell(enemySpawnArr[randIndex].position, enemySpeed);
					StartCoroutine("SpawnerEffect", enemySpawnArr[randIndex]);
				}


				// based on the returned bool array, spawn enemies accordingly
				for (int i = 0 ; i < 4 ; i++)
				{
					if (openSpawns[i]) 
					{
						enemyTransArr[enemyIndex].position = enemySpawnArr[i].position;
						enemyScriptArr[enemyIndex].speed = enemySpeed;
						enemyIndex++;
						if (enemyIndex >= enemyTransArr.Length) enemyIndex = 0;

						StartCoroutine("SpawnerEffect", enemySpawnArr[i]);
					}
				}

				float wait = 1 / spawnRate;
				yield return new WaitForSeconds(wait);

			}
		}
	}


	// method that handles the opening of spawner visuals and audio over time
	IEnumerator OpenSpawner(int index)
	{
		AudioSource.PlayClipAtPoint(openSpawnerAudio, enemySpawnArr[index].position);

		for (float t = 0 ; t < 0.4f ; t += Time.deltaTime)
		{
			enemySpawnArr[index].localScale = Vector3.Lerp(Vector3.zero, defScale * 1.1f, t * 2.5f);				
			//enemySpawnArr[index].Rotate(0,0, (1.3f - t) * 200 * Time.deltaTime);			
			yield return null;
		}

		// scale it to final size and rotate it into the proper position
		for (float t = 0 ; t < 0.4f ; t += Time.deltaTime)
		{
			enemySpawnArr[index].localScale = Vector3.Lerp(enemySpawnArr[index].localScale, defScale, t * 5);
			//enemySpawnArr[index].rotation = Quaternion.Slerp(enemySpawnArr[index].rotation, Quaternion.identity, t * 2.5f);
			yield return null;
		}
	}

	// method to roll for enemy spawn count and locations, returned in the form of a bool arr
	bool[] GetOpenSpawns()
	{
		bool[] outBool = new bool[4]{false, false, false, false};

		// a roll of 300 or greater indicates an enemy in all 3 spawns, 200 or greater in 2, 100 or greater in 1, less than 100 no enemies spawn this loop
		int enemiesRoll = Random.Range(rollLower, rollUpper);

		if (enemiesRoll > 399) 
			outBool = new bool[4]{true, true, true, true};

		else if (enemiesRoll > 299)
		{
			int spawnRoll = Random.Range(0,4);

			for (int i = 0 ; i < 4 ; i++)
			{
				if (i != spawnRoll) outBool[i] = true;
			}
		}

		else if (enemiesRoll > 199)
		{
			int spawnRollA = Random.Range(0,4);
			int spawnRollB = Random.Range(0,3);

			if (spawnRollB == spawnRollA) spawnRollB++;
						
			for (int i = 0 ; i < 4 ; i++)
			{
				if (i != spawnRollA && i != spawnRollB) outBool[i] = true;
			}
		}

		else if (enemiesRoll > 99)
		{
			int spawnRoll = Random.Range(0,4);
			
			for (int i = 0 ; i < 4 ; i++)
			{
				if (i == spawnRoll) outBool[i] = true;
			}
		}

		return outBool;	
	}


	// method to control spawner visual and audio over time when an enemy is spawned
	IEnumerator SpawnerEffect(Transform spawner)
	{
		// begin playing audio at spawner location
		AudioSource.PlayClipAtPoint(spawnAudio, spawner.position);

		// using a sin wave that decreases in magnitude over time, we wobble the spawner's visual size
		for (float t = 0 ; t < 0.4f ; t += Time.deltaTime)
		{
			float wobble = Mathf.Sin(t * wobbleFrequency) * wobbleAmplitude * (0.4f - t);
			spawner.localScale = new Vector3(defScale.x + wobble, defScale.y + wobble, 0);
			yield return null;
		}
	}


	// method called by a bullet script upon killing an enemy unit
	public void KilledEnemy()
	{
		playerScore++;

		int onesDigit = playerScore % 10;
		int tensDigit = ( (playerScore - onesDigit) % 100 ) / 10;
		int hundredsDigit = ( (playerScore - (tensDigit * 10 + onesDigit)) % 1000 ) / 100;
		int thousandsDigit = (playerScore - (hundredsDigit * 100 + tensDigit * 10 + onesDigit)) / 1000;

		scoreNumARend.sprite = numSprites[thousandsDigit];
		scoreNumBRend.sprite = numSprites[hundredsDigit];
		scoreNumCRend.sprite = numSprites[tensDigit];
		scoreNumDRend.sprite = numSprites[onesDigit];		
	}
}
