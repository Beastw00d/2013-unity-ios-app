using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelController : MonoBehaviour 
{
	/// LevelController is the main script in charge of the level including the tasks of advancing difficulty/zone, spawning blocks, and calling for the tree manager to spawn trees.
	/// Additionally, this script controls a lot of the visual 'gui' elements and pause/game over menu

	// scripts that LevelController will call to
	private TreeManager treeManager;
	private SceneFader sceneFader;

	// static bool to let all game scripts know when the game is paused so that the players cannot make actions
	public static bool isPaused = true;

	// static int to represent game time across all scripts and used to get player scores.  Also ints to indicate at what time the zone will swap to trees and advance, repsectively
	public static int startingGameTime, gameTime, treeTime, advanceTime;

	// static int to represent device game is running on: 0 for PC/web 1 for iOS.  
	public int deviceModeInt;
	public static int deviceMode;

	// the transform used to determining level and player relative locations as well as block and crate 'spawn' points
	public Transform mobileTrans;

	// various player objects and components
	public GameObject[] playerObjs, playerUIs;
	public PlayerControl[] playerControlArr;

	// various 'gui' visual objects, components, and audio
	public SpriteRenderer[] restartSprite, winnerSprite;
	public GameObject buttonRestart, buttonMenu;
	public SpriteRenderer hudNumARend, hudNumBRend, advancingTextRend, advancingNumARend, advancingNumBRend, buttonAudioRend, 
		pausedRend, gameOverRend, buttonRestartRend, buttonRestartText, buttonMenuRend, buttonMenuText;
	public Transform advancingTrans;

	public Sprite audioMuteSprite, audioUnmuteSprite;
	public AudioClip clickButtonAudio;

	// array of number sprites 0-9 and ints and renderers for the timer
	public Sprite[] numSprites;
	private int[] timerInts = new int[]{0, 0, 0};
	public SpriteRenderer[] timerRendArr;

	// transform array of row parents, currently watched row, and current row index
	public Transform[] parentTrans;
	public Transform curTrans;
	private int curIndex = 0;

	// randLimit fluxuates with each row of boxes spawned and attempts to hedge the randomness by comparing its output over time against randControl's steadily increasing value
	public float randLimit;
	public float startingRandControl;
	private float randControl;

	// the rate at which boxes will spawn over time. Higher means levels get more cluttered more quickly
	public float spawnRate;
	private bool canSpawn = false, canPause = false, hasRevertPoint = false;

	// static float for current level speed and user adjustable values for the starting speed and rate at which speed will increase with each level advance
	public static float levelSpeed;
	public float startingLevelSpeed;
	public float levelAccelerationRate;

	// array of player alive statuses and int for total alive player count
	private BitArray playersArr;
	public int alivePlayers;

	// zone color values, and the box material that will adjust box colors based on zone
	public Material boxMat, treeMat;
	public Color[] zoneColorArr;
	private Color zoneColor;
	private int zoneInt = 00;

	// array of crate spawn limits and control rates.  These work similarly to those used for box spawn rates
	private float[] crateLimitArr = new float[]{0,0,0}; //,0};
	public float[] crateSpawnRateArr;
	public GameObject[] crateObj;

	// this int array is changed each time a crate spawns in order to randomly reorder the order which crate types are tested for spawning
	int[] crateOrderArr = new int[]{0,1,2}; //,3};

	// array of background pattern transforms
	public Transform[] backPatArr;
	private int backPatIndex = 0;

	// bool set true if the player is starting a fresh run (from zone 1) and is therefore eligible for the X zones in a single run achievement
	bool isFreshRun = false;


	// -----------------------------------------------------------------------------------------------------------------------------------


	// set up intial variable and start coroutine to advance zone and difficulty over time
	void Start()
	{
		deviceMode = deviceModeInt;

		treeManager = GetComponent<TreeManager>();

		if (GameMode.globalVolume == 0)
		{
			AudioListener.volume = 0;
			buttonAudioRend.sprite = audioMuteSprite;
		}
		else
		{
			if (GameMode.globalVolume == -1) GameMode.globalVolume = 1;
			
			AudioListener.volume = 1;
			buttonAudioRend.sprite = audioUnmuteSprite;
		}

		isPaused = false;

		sceneFader = GameObject.FindGameObjectWithTag("Cover").GetComponent<SceneFader>();

		// if the current players is 0, then I'm either testing or its a bug
		if (GameMode.curPlayers == 0) GameMode.curPlayers = 1;

		// get number of alive players from the gameMode script and destroy the unneeded players
		alivePlayers = GameMode.curPlayers;

		/*if (deviceMode != 1 && alivePlayers == 1)
		{
			playerObjs[0].transform.position = new Vector3(0, playerObjs[0].transform.position.y, playerObjs[0].transform.position.z);
			playerUIs[0].transform.position = new Vector3(28.4f, 0, 0);
		}*/

		// populate playersByte based on number of alive players
		playersArr = new BitArray(alivePlayers);

		for (int i = alivePlayers - 1 ; i > -1 ; i--)
			playersArr[i] = true;

		// set initial variables
		levelSpeed = 0;

		curTrans = parentTrans[0];

		StartCoroutine("StartTheGame");
	}


	// method to advance the zone ints and set the sprites appropriately.  If bool is true, the int is the specific zone to advance to.  If false, int is increment amount
	void AdvanceZoneInt(bool setDirectly, int thisInt)
	{
		if (setDirectly) zoneInt = thisInt;
		else zoneInt += thisInt;

		// store this advance into the GameMode script
		GameMode.currentLevel = zoneInt;

		// check if this level unlocks a new achievement for the player
		if (zoneInt == 3) StatManager.UnlockAchievement(1);
		else if (zoneInt == 5) StatManager.UnlockAchievement(2);
		else if (zoneInt == 10) StatManager.UnlockAchievement(3);
		else if (zoneInt == 15) StatManager.UnlockAchievement(4);
		else if (zoneInt == 20) StatManager.UnlockAchievement(5);

		// set appropriate sprites for each digit
		int onesDigit = (zoneInt + 1) % 10;
		int tensDigit = ((zoneInt + 1) - onesDigit) / 10;

		advancingNumARend.sprite = hudNumARend.sprite = numSprites[tensDigit];
		advancingNumBRend.sprite = hudNumBRend.sprite = numSprites[onesDigit];

		// update the zone color using the zoneInt as the index in the zoneColorArr.  If we run out of colors, loop the colors back around
		if (zoneInt < zoneColorArr.Length)
		{
			zoneColor = zoneColorArr[zoneInt];
			advancingTextRend.color = advancingNumARend.color = advancingNumBRend.color = hudNumARend.color = hudNumBRend.color = zoneColor;
		}
		else
		{
			int thisZoneInt = zoneInt;

			while (thisZoneInt >= zoneColorArr.Length)
			{
				thisZoneInt -= zoneColorArr.Length;
			}

			zoneColor = zoneColorArr[thisZoneInt];
			advancingTextRend.color = advancingNumARend.color = advancingNumBRend.color = hudNumARend.color = hudNumBRend.color = zoneColor;
		}
	}


	// method to get the zone times needed for the next zone
	void GetZoneTimes()
	{
		gameTime = (zoneInt) * 20;
		treeTime = gameTime + 12;
		advanceTime = gameTime + 16;
	}

	
	// routine to maanage the passing of time and (currently due to scores being 100% time based) player scores
	IEnumerator TimerUpdate()
	{
		gameTime = (zoneInt) * 20;
		startingGameTime = gameTime;
		int tempTime = gameTime;

		for (int i = timerInts.Length ; i > 0; i--)
		{
			int thisSlot;

			if (i > 1)
			{
				int digit = 10;

				for (int j = 2 ; j < i ; j++)
				{
					digit *= 10;
				}

				int remainder = tempTime % digit;
				thisSlot = (tempTime - remainder) / digit;
				timerRendArr[timerInts.Length - i].sprite = numSprites[thisSlot];
				tempTime = remainder;
			}

			else
			{
				thisSlot = tempTime;
				timerRendArr[timerInts.Length - i].sprite = numSprites[thisSlot];
			}
		}

		float t = 0;
		while(alivePlayers > 0)
		{
			if(!isPaused)
			{
				t += Time.deltaTime;

				if (t > 1)
				{
					t -= 1;
					gameTime++;

					// for loop updates the visual timers one digit at a time starting at the far right
					for (int i = timerInts.Length - 1 ; i > -1 ; i--)
					{
						timerInts[i]++;

						if (timerInts[i] > 9)
						{
							timerInts[i] -= 10;
							timerRendArr[i].sprite = numSprites[timerInts[i]];
						}
						else
						{
							timerRendArr[i].sprite = numSprites[timerInts[i]];
							break;
						}
					}
				}
			}

			yield return null;
		}
	}


	// method to intialize the game over time
	IEnumerator StartTheGame()
	{
		// wait for the scene fader to finish fading
		while (SceneFader.isFading)
			yield return null;

		// if the GameMode has a higher current level than starting level, visually show the player reverting to the starting level
		if (GameMode.currentLevel > GameMode.startingLevel)
		{
			AdvanceZoneInt(true, GameMode.currentLevel);

			audio.pitch = -0.1f;
			audio.Play();

			advancingTrans.position = new Vector3(0, mobileTrans.position.y - 100, 0);
			float revertSpeed = 0;

			backPatArr[0].position = new Vector3(backPatArr[0].position.x, mobileTrans.position.y - 140, backPatArr[0].position.z);
			backPatArr[1].position = new Vector3(backPatArr[1].position.x, mobileTrans.position.y, backPatArr[1].position.z);
			backPatIndex = 1;

			while (true)
			{
				revertSpeed = Mathf.Lerp(revertSpeed, 400, Time.deltaTime);
				audio.pitch = -revertSpeed / 200;
				Vector3 revertVect = new Vector3(0, revertSpeed * Time.deltaTime, 0);

				if (advancingTrans.position.y > mobileTrans.position.y + 240)
				{
					if (GameMode.currentLevel <= GameMode.startingLevel)
						break;
					else
					{
						AdvanceZoneInt(false, -1);
						advancingTrans.position = new Vector3(0, mobileTrans.position.y - 150, 0);
					}
				}
				else
					advancingTrans.position += revertVect;

				if (backPatArr[backPatIndex].position.y > mobileTrans.position.y + 420)
				{
					backPatArr[backPatIndex].position -= new Vector3(0,560,0);
					backPatIndex = 1 - backPatIndex;
				}
			
				backPatArr[0].position += revertVect;
				backPatArr[1].position += revertVect;

				yield return null;
			}

			// now to smoothly slow down the 
			float tempRate = 1.5f;
			while (revertSpeed > 0.1f)
			{
				tempRate += 10 * Time.deltaTime;
				revertSpeed = Mathf.Lerp(0, revertSpeed, 1 - Time.deltaTime * tempRate);
				audio.pitch = -revertSpeed / 200;

				Vector3 revertVect = new Vector3(0, revertSpeed * Time.deltaTime, 0);				
				advancingTrans.position += revertVect;

				if (backPatArr[backPatIndex].position.y > mobileTrans.position.y + 420)
				{
					backPatArr[backPatIndex].position -= new Vector3(0,560,0);
					backPatIndex = 1 - backPatIndex;
				}

				backPatArr[0].position += revertVect;
				backPatArr[1].position += revertVect;

				yield return null;
			}

			// make sure the correct back pattern sprite renderer is being watched once we reverse directions
			if (backPatArr[0].position.y < backPatArr[1].position.y) backPatIndex = 0;
			else backPatIndex = 1;
		}
		else
		{
			isFreshRun = true;
			AdvanceZoneInt(true, GameMode.startingLevel);	
		}

		// play the audio with regular audio
		audio.pitch = 1;
		audio.Play();

		// set the starting level speed and rand control values
		float tarLevelSpeed = startingLevelSpeed + (GameMode.startingLevel - 1) * levelAccelerationRate;
		randControl = startingRandControl + (GameMode.startingLevel - 1) * spawnRate;
			
		// set the box color and prepare the advancing visual to enter player view
		int thisIndex = zoneInt;

		while(thisIndex >= zoneColorArr.Length)
		{
			thisIndex -= zoneColorArr.Length;
		}

		boxMat.color = zoneColorArr[thisIndex];	
		treeMat.color = zoneColorArr[thisIndex];
		advancingTrans.position = new Vector3(0, mobileTrans.position.y + 250, 0);

		canPause = true;
		bool timerStarted = false;

		// player vehicles begin to move and emit particles
		float t=0;
		while (t < 1f)
		{
			if(!isPaused)
			{
				t += Time.deltaTime;

				levelSpeed = Mathf.Lerp (0, tarLevelSpeed, (t / 2) * (t / 2));
			}			 
			
			yield return null;
		}
		
		// give players control
		for (int i=0 ; i < alivePlayers ; i++)
		{
			playerControlArr[i].canMove = true;
		}

		while (t < 2f)
		{	
			if(!isPaused)
			{
				t += Time.deltaTime;				
				levelSpeed = Mathf.Lerp (0, tarLevelSpeed, (t / 2) * (t / 2));

				if (!timerStarted && mobileTrans.position.y + 50 > advancingTrans.position.y)
				{
					StartCoroutine("TimerUpdate");
					timerStarted = true;
				}
			}
			
			yield return null;
		}

		// set the level speed to the exact starting speed
		levelSpeed = tarLevelSpeed;

		// if timer has not started yet, wait for it to start before moving on
		while (!timerStarted)
		{
			if (mobileTrans.position.y + 50 > advancingTrans.position.y)
			{
				StartCoroutine("TimerUpdate");
				timerStarted = true;
			}

			yield return null;
		}

		// start the main level controller loop routine
		StartCoroutine("AdvanceZone");
	}


	// method to transition the game into to the next zone
	IEnumerator AdvanceZone()
	{
		while (true)
		{
			// tell the player script that the zone advanced in order to check for the zone specific achievements & check for zone X in one run achievement
			playerControlArr[0].CrossedIntoZone();

			if (isFreshRun && zoneInt > 19) StatManager.UnlockAchievement(13);

			// allow blocks to spawn and get the times needed to spawn the tree area and advance the zone at the correct times
			canSpawn = true;
			GetZoneTimes();

			// wait until the time to switch from blocks to trees is reached
			while (gameTime < treeTime)
			{
				yield return null;
			}

			canSpawn = false;

			while (gameTime < treeTime + 0.3f)
			{
				yield return null;
			}

			// set the appropirate tree material color and start a tree area
			treeMat.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1);
			treeManager.StartTreeArea();

			// wait until the advance level time is reached + some time for the tree zone to end
			while (gameTime < advanceTime + 2.5f)
			{
				yield return null;
			}

			// advance the zone int and increase the level speed and difficulty
			AdvanceZoneInt(false, 1);
			levelSpeed = startingLevelSpeed + levelAccelerationRate * zoneInt;
			randControl = startingRandControl + spawnRate * zoneInt;

			// place the advancing trans above the mobile trans position
			advancingTrans.position = new Vector3(0, mobileTrans.position.y + 250, 0);

			// wait one more second
			while (gameTime < advanceTime + 4)
			{
				yield return null;
			}

			// update block color
			boxMat.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1);	
		}
	}


	// update to control the pause menu
	void Update()
	{
		float curMobileY = mobileTrans.position.y;
		
		// see if the currently watched row has passed the reset point
		if (curMobileY - 16 > curTrans.position.y)
		{
			// reposition and reroll the row that reached the screen bottom
			curTrans.position += new Vector3(0,252,0);
			GetRandomBoxes(parentTrans[curIndex]);
			
			curIndex ++;
			if (curIndex > 13) curIndex = 0;
			
			curTrans = parentTrans[curIndex];
		}
		
		// see if the background pattern needs to be move to the top
		if (curMobileY > backPatArr[backPatIndex].position.y + 140)
		{
			backPatArr[backPatIndex].position = backPatArr[backPatIndex].position + new Vector3(0,560,0);
			backPatIndex = 1 - backPatIndex;
		}
		
		// move the transform each update as long as we are not paused
		if (!isPaused)
		{
			mobileTrans.position = new Vector3(mobileTrans.position.x, mobileTrans.position.y + levelSpeed * Time.deltaTime, mobileTrans.position.z);

			if (Input.GetKeyDown(KeyCode.Escape))
				TryPause();
		}
	}


	// randomize the boxes that are active in the given row
	void GetRandomBoxes(Transform thisRow)
	{
		// an empty array of transforms to be filled in with this row's boxes
		Transform[] rowBoxArr = new Transform[7];
		int boxArrIndex = 0;

		// get the array of sprites parented to this row, and destroy any leftover crates
		BoxScript[] rowBoxScripts = thisRow.GetComponentsInChildren<BoxScript>();

		for (int i=0 ; i < rowBoxScripts.Length ; i++)
		{
			if (rowBoxScripts[i].boxType > 0) 
				Destroy (rowBoxScripts[i].gameObject);
			else
			{
				rowBoxArr[boxArrIndex] = rowBoxScripts[i].transform;
				boxArrIndex++;
			}
		}

		// make sure spawning is not disabled
		if (canSpawn)
		{
			// roll to see if a crate will spawn in this wave
			int crateIndex = Random.Range(0,rowBoxArr.Length);
			float crateRoll = -1f;

			while (true)
			{
				crateIndex++;
				if (crateIndex >= rowBoxArr.Length) crateIndex = 0;

				if (rowBoxArr[crateIndex].tag == "Box")
				{
					crateRoll = Random.Range(5f, 100f);
					break;
				}
			}

			// check crate spawn limits against the random we rolled
			bool crateHasSpawned = false;

			for (int j = 0 ; j < crateLimitArr.Length ; j++)
			{
				int i = crateOrderArr[j];

				crateLimitArr[i] += crateSpawnRateArr[i] * (1 + alivePlayers / 3);

				if (crateHasSpawned)
					crateLimitArr[i] = Mathf.Max(crateLimitArr[i] - (150 - crateLimitArr[i]) / (0.05f * crateLimitArr[i] + 1), Mathf.Min(crateLimitArr[i], 0));
				else if (crateRoll < crateLimitArr[i])
				{
					crateHasSpawned = true;
					Instantiate(crateObj[i], rowBoxArr[crateIndex].transform.position, Quaternion.identity);
					crateLimitArr[i] = -10;
				}
			}

			// if we spawned a crate, then reorder the crate order array for next row's test and set a penalty to make it unlikely another crate spawns in the near future
			if (crateHasSpawned)
			{
				for (int i = 0 ; i < crateOrderArr.Length ; i++)
				{
					crateOrderArr[i] = -1;
				}

				for (int i = 0 ; i < crateOrderArr.Length ; i++)
				{
					int rand = Random.Range(i,crateOrderArr.Length);

					for (int j = 0 ; j <= i ; j++)
					{
						if (crateOrderArr[rand] != -1)
						{
							rand++;
							if (rand == 3) rand = 0;
						}
						else
						{
							crateOrderArr[rand] = i;
							j = i;
						}
					}
				}
			}

			// Time to spawn the actual box row.  Clear out any leftovers in this row, then spawn the boxes based on random number generation compared to the current randLimit
			int boxesSpawned = 0;

			for (int i=0 ; i < rowBoxArr.Length ; i++)
			{
				// if  this row has a crate and this is the crate index, make sure the box in this position is disabled
				if (crateHasSpawned && i == crateIndex)
					rowBoxArr[i].GetComponent<SpriteRenderer>().enabled = rowBoxArr[i].collider.enabled = false;

				// else if we have not filled out the entire row, roll to see if a box can spawn in this slot
				else if (boxesSpawned < 6 && Random.Range(0f, 10f) < randLimit)
				{
					rowBoxArr[i].GetComponent<SpriteRenderer>().enabled = rowBoxArr[i].collider.enabled = true;
					boxesSpawned++;
				}

				// else the row is full or the roll was not in range to spawn a box, so disable the box at this location
				else
					rowBoxArr[i].GetComponent<SpriteRenderer>().enabled = rowBoxArr[i].collider.enabled = false;
			}

			float boxVariance = (Mathf.Max(randLimit, 0.1f) / 10f ) / Mathf.Max(0.1f, (boxesSpawned / (rowBoxArr.Length - 1f)));
			randLimit = Mathf.Min(Random.Range(randControl, randLimit * boxVariance), 6);
		}

		// if we cannot spawn, turn off all boxes
		else
		{
			for (int i=0 ; i < rowBoxArr.Length ; i++)
			{
				rowBoxArr[i].GetComponent<SpriteRenderer>().enabled = rowBoxArr[i].collider.enabled = false;
			}
		}
	}


	// method called by a player script upon dying, and will update player count and pause the score for the given player.  If this player was last alive, show game over
	public void PlayerDied(int thisPlayerInt)
	{
		if (playersArr[thisPlayerInt-1] == true)
		{
			playersArr[thisPlayerInt-1] = false;
			alivePlayers -= 1;

			if (alivePlayers <= 0)
			{
				// check if the zoneInt is above the player's high score, and if so then store this zone as the new high score
				if (zoneInt > StatManager.highScore)
					StatManager.SetHighScore(zoneInt);

				// store the playtime of this run in the GameMode script
				GameMode.lastPlayTime = gameTime - startingGameTime;

				// get and assign the correct zone to revert to
				if (!hasRevertPoint)
				{
					hasRevertPoint = true;
					GameMode.startingLevel = Mathf.Max(zoneInt - 1, 0);
				}

				StartCoroutine("ShowRestartOption", thisPlayerInt);
			}
		}
	}


	// display the winner and give option to restart the game
	IEnumerator ShowRestartOption (int winnerInt)
	{
		Color fadeColor = Color.white;
		fadeColor.a = 0;
		
		Color fadeBackColor = restartSprite[0].color;
		fadeBackColor.a = 0;

		Color winnerFadeColor = Color.white;
		winnerFadeColor.a = 0;

		Color buttonTextColor = buttonRestartText.color;
		buttonTextColor.a = 0;

		buttonRestartText.color = buttonMenuText.color = buttonTextColor;
		buttonRestartRend.color = buttonMenuRend.color = buttonAudioRend.color = fadeColor;
		restartSprite[0].color = restartSprite[2].color = fadeBackColor;
		buttonRestartRend.enabled = buttonRestartText.enabled = buttonMenuRend.enabled = buttonMenuText.enabled = buttonAudioRend.enabled = 
			restartSprite[0].enabled = restartSprite[2].enabled = true;

		if (winnerInt > 0)
		{
			StopCoroutine("TimerUpdate");
			StopCoroutine("StartTheGame");
			StopCoroutine("AdvanceZone");
			StartCoroutine("SlowDownTime");
		
			/*
			// multiplayer
			if (GameMode.curPlayers > 1)
			{
				restartSprite[1].color = fadeColor;
				restartSprite[1].enabled = true;

				winnerFadeColor = winnerSprite[winnerInt - 1].color;
				winnerFadeColor.a = 0;
				winnerSprite[winnerInt - 1].color = winnerFadeColor;

				winnerSprite[winnerInt - 1].enabled = true;
			}
			// single player
			else
			{*/
				gameOverRend.color = fadeColor;
				gameOverRend.enabled = true;
			//}
		}

		else
		{
			pausedRend.color = fadeColor;
			pausedRend.enabled = true;
		}

		for (float t = 0 ; t < 0.5f ; t += Time.deltaTime)
	    {
			buttonTextColor.a = fadeColor.a = fadeBackColor.a = winnerFadeColor.a = 2 * t;
			buttonRestartRend.color = buttonMenuRend.color = buttonAudioRend.color = fadeColor;
			buttonRestartText.color = buttonMenuText.color = buttonTextColor;

			restartSprite[0].color = fadeBackColor;
			fadeBackColor.a = t / 2;
			restartSprite[2].color = fadeBackColor;

			if (winnerInt > 0)
			{
				/*if (GameMode.curPlayers > 1)
				{
					winnerSprite[winnerInt - 1].color = winnerFadeColor;
					restartSprite[1].color = fadeColor;
				}
				else*/
					gameOverRend.color = fadeColor;
			}
			else
				pausedRend.color = fadeColor;

			yield return null;
	    }

		buttonRestart.collider.enabled = buttonMenu.collider.enabled = true;

		// loop until the user choose to restart the level or return to menu
		while(true)
		{
			if (winnerInt <= 0 && isPaused && Input.GetKeyDown(KeyCode.Escape))
			{
				buttonRestart.collider.enabled = buttonMenu.collider.enabled = false;

				buttonRestartRend.enabled = buttonRestartText.enabled = buttonMenuRend.enabled = buttonMenuText.enabled = buttonAudioRend.enabled = 
					restartSprite[0].enabled = restartSprite[2].enabled = pausedRend.enabled = false;

				isPaused = false;
				break;
			}
			else if (Input.GetMouseButtonDown(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100))
				{
					if (hit.collider.gameObject == buttonMenu)
					{
						AudioSource.PlayClipAtPoint(clickButtonAudio, Vector3.zero);
						sceneFader.FadeToScene(0);
						break;
					}
					else if (hit.collider.gameObject == buttonRestart)
					{
						GameMode.startingLevel = Mathf.Max(zoneInt - 1, 0);

						AudioSource.PlayClipAtPoint(clickButtonAudio, Vector3.zero);
						sceneFader.FadeToScene(Application.loadedLevel);

						break;
					}
					else if (hit.collider.gameObject.name.Equals("ButtonMute"))
					{
						if (GameMode.globalVolume != 0)
						{
							AudioListener.volume = 0;
							GameMode.globalVolume = 0;
							buttonAudioRend.sprite = audioMuteSprite;							
						}
						else
						{
							AudioListener.volume = 1;
							GameMode.globalVolume = 1;
							buttonAudioRend.sprite = audioUnmuteSprite;
						}
					}

					// unpause on a touch device
					else if (winnerInt <= 0 && isPaused)
					{
						buttonRestart.collider.enabled = buttonMenu.collider.enabled = false;
						
						buttonRestartRend.enabled = buttonRestartText.enabled = buttonMenuRend.enabled = buttonMenuText.enabled = buttonAudioRend.enabled = 
							restartSprite[0].enabled = restartSprite[2].enabled = pausedRend.enabled = false;
						
						isPaused = false;
						break;
					}
				}
				// unpause on a touch device
				else if (winnerInt <= 0 && isPaused)
				{
					buttonRestart.collider.enabled = buttonMenu.collider.enabled = false;
					
					buttonRestartRend.enabled = buttonRestartText.enabled = buttonMenuRend.enabled = buttonMenuText.enabled = buttonAudioRend.enabled = 
						restartSprite[0].enabled = restartSprite[2].enabled = pausedRend.enabled = false;
					
					isPaused = false;
					break;
				}
			}

			yield return null;
		}
	}


	// method to slow down time and fade out audio
	IEnumerator SlowDownTime()
	{
		while (levelSpeed > 0.1f)
		{
			audio.volume = Mathf.Lerp(audio.volume, 0, Time.deltaTime / 2);
			audio.pitch = audio.volume;
			levelSpeed = Mathf.Lerp(levelSpeed, 0, Time.deltaTime / 2);

			yield return null;
		}

		audio.volume = audio.pitch = levelSpeed = 0;
	}


	// this method attempts to pause the game (if the player is allowed to pause)
	public void TryPause()
	{
		if (canPause)
		{
			isPaused = true;
			StartCoroutine("PauseGame");
			StartCoroutine("ShowRestartOption", 0);
		}
	}


	// pause the game and wait for input to unpause or select a menu option
	IEnumerator PauseGame()
	{
		float oldVol = audio.volume;
		float oldSpeed = levelSpeed;

		audio.volume = audio.pitch = levelSpeed = 0;

		while (isPaused)
			yield return null;

		audio.pitch = 1;
		audio.volume = oldVol;
		levelSpeed = oldSpeed;
	}
}