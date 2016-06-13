using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour 
{
	// script in charge of controlling the player through user input and player interaction with the level

	public bool testMode;

	public LevelController levelController;
	public GameCam gameCam;
	public Transform playerTrans;

	public AudioClip hitAudio, bigHitAudio, shieldedHitAudio, mineHitAudio;

	public AudioSource scrapeAudio;
	public ParticleSystem collisionSys;

	// int for inputMode aka which keyboard buttons this player uses and controlmask for touch controls
	public int inputMode, controlMode;
	public LayerMask controlsMask;
	public float joystickDeadZone;
	public Transform joystickTrans;
	private Vector2 joystickPos;

	// array of the colors that the player ship's lights, name renderer, and hp renderers could be
	private int shipIndex;
	public SpriteRenderer nameRend;
	public Sprite[] nameSpriteArr;
	public Color[] shipColors;

	public GameObject[] bulletPrefab, minePrefab;
	public SpriteRenderer[] ammoLights;
	private int ammo;

	private int mineCount = 0;

	// int for the currently recharging slot. If -1, then none of the slots are recharging
	private int rechargingSlot = -1;

	// how long the recharge checker will wait in between recharge checks (in seconds);
	public int rechargeTime;

	// how far above the player ship transform (on the Y axis) should bullets be instantiated, and drop point for dropable items
	public float weaponShootOffset, weaponDropOffset;
	
	public float agility;
//	private float horiz = 0, vert = 0;

	public ParticleSystem[] playerThrusters;

	public float moveAmtH, moveAmtV, h, v, rightBorder, leftBorder;
//	private float velocFloatH = 0, velocFloatV = 0;

//	private float lastPressedDown = 9;

	public bool canMove;

	public int playerHP;
	public SpriteRenderer[] hpBar;

	// bool is set to true following a player being healed (mainly used to check for the triple power-up achievement)
	bool isRecentlyHealed = false;

	// is the player currently able to fire penetrating shots
	public bool isPenetrating = false;

	// is the player currently shielded, and int used for the shield spree achievement
	public bool isShielded = false;
	public SpriteRenderer bubbleSpriteRend;
	private int boxShieldedCount = 0;

	// array of current powerup renderers for each powerup type. Set null when that powerup isnt active on the player
	public SpriteRenderer[] curPowerupArr = new SpriteRenderer[]{null, null, null};

	// array of currently in inventory mines
	public GameObject[] mineArr = new GameObject[]{null,null,null};

	// how many contact points with trees does this player currently have, if > 0 then will run a coroutine to do damage over time
	private int treeCollisionPoints = 0;
	private float colTimer = 0.5f;

	// ints used to measure various achievements. Set to -1 when they are failed, otherwise incremented by 1 each sucessful zone clear
	private int zoneCountDamage = -1, zoneCountShots = -1, zoneCountPowerups = -1;

	// -----------------------------------------
	
	void Awake()
	{
		// get control mode from game mode script
		controlMode = GameMode.controlModeInt;

		// get the ship index and set the emission to true for the appropriate colored particle system
		shipIndex = GameMode.selectedShip - 1;
		playerThrusters[shipIndex].enableEmission = true;

		// set the lights to the appropriate color
		for (int i = 0 ; i < ammoLights.Length ; i++)
		{
			ammoLights[i].color = shipColors[shipIndex];
		}

		// set the name sprite and color and the bubble shield renderer color
		nameRend.sprite = nameSpriteArr[shipIndex];
		nameRend.color = bubbleSpriteRend.color = shipColors[shipIndex];

		// set the hp bar color
		for (int i = 0 ; i < hpBar.Length ; i++)
		{
			hpBar[i].color = shipColors[shipIndex];
		}

		// get the joystick X and Y positions
		joystickPos.x = joystickTrans.position.x;
		joystickPos.y = joystickTrans.position.y;

		// fill up player ammo and hp
		ammo = 3;
		playerHP = 7;
	}

	// Update is called once per frame
	void Update () 
	{
		if (!LevelController.isPaused && canMove)
		{
			// based on input mode (which player this is) get the horizontal, vertical, and shoot inputs
			if (LevelController.deviceMode == 1)
			{
				h = v = 0;

				// on an actual touch device
				if (!testMode && !LevelController.isPaused)
				{
					// touch joystick control
					if (controlMode == 0)
					{
						int joystickTouches = 0;
						int i = 0;
						while (i < Input.touchCount)
						{
							Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
							RaycastHit2D hit;

							hit = Physics2D.GetRayIntersection(ray, 20f, controlsMask);

							// if the player touches above this range on the Y axis, then open up the pause menu
							if (hit.point.y > -25)
							{
								if (Input.GetTouch(i).phase == TouchPhase.Began && hit.collider != null)
									levelController.TryPause();
							}

							else if (hit.point.x < -10)
							{
								float thisH = (hit.point.x - joystickPos.x) / 6;
								float thisV = (hit.point.y - joystickPos.y) / 6;

								if (thisH > joystickDeadZone) thisH = Mathf.Min(thisH - joystickDeadZone, 1);
								else if (thisH < -joystickDeadZone) thisH = Mathf.Max(thisH + joystickDeadZone, -1);
								else thisH = 0;

								if (thisV > joystickDeadZone) thisV = Mathf.Min(thisV - joystickDeadZone, 1);
								else if (thisV < -joystickDeadZone) thisV = Mathf.Max(thisV + joystickDeadZone, -1);
								else thisV = 0;

								h = (h * joystickTouches + thisH) / (joystickTouches + 1);
								v = (v * joystickTouches + thisV) / (joystickTouches + 1);

								joystickTouches++;
							}
							else if (ammo > 0 && Input.GetTouch(i).phase == TouchPhase.Began && hit.point.x > 10)
								FireWeapon();

							i++;
						}
					}

					// accelerometer control
					else if (controlMode == 1)
					{
						Vector3 accel = new Vector3(Input.acceleration.x * 2, -Input.acceleration.z * 2, 0);
						
						// clamp acceleration vector to unit sphere
						if (accel.sqrMagnitude > 1)	accel.Normalize();

						accel *= 2;

						h = Mathf.Clamp( (h + accel.x) / 1.9f, -1, 1 );
						v = Mathf.Clamp( (v + accel.y) / 1.9f, -1, 1 );

						int i = 0;
						while (i < Input.touchCount)
						{
							if (Input.GetTouch(i).phase == TouchPhase.Began && ammo > 0)
							{
								FireWeapon();
								break;
							}
							i++;
						}
					}
				}

				// test mode using mouse to simulate touching
				else
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit2D hit;

					hit = Physics2D.GetRayIntersection(ray, 20f, controlsMask);

					// if the player touches above this range on the Y axis, then open up the pause menu
					if (hit.point.y > -25)
					{
						if (Input.GetMouseButtonDown(0) && hit.collider != null)
							levelController.TryPause();
					}

					else if (hit.point.x < -10)
					{
						float thisH = (hit.point.x - joystickPos.x) / 8;
						float thisV = (hit.point.y - joystickPos.y) / 8;
						
						if (thisH > joystickDeadZone) thisH = Mathf.Min(thisH - joystickDeadZone, 1);
						else if (thisH < -joystickDeadZone) thisH = Mathf.Max(thisH + joystickDeadZone, -1);
						else thisH = 0;
						
						if (thisV > joystickDeadZone) thisV = Mathf.Min(thisV - joystickDeadZone, 1);
						else if (thisV < -joystickDeadZone) thisV = Mathf.Max(thisV + joystickDeadZone, -1);
						else thisV = 0;
						
						h = Mathf.Pow(thisH, 2);
						v = Mathf.Pow(thisV, 2);

						if (thisH < 0) h *= -1;
						if (thisV < 0) v *= -1;
					}
					else if (ammo > 0 && Input.GetMouseButtonDown(0) && hit.point.x > 10)
						FireWeapon();
				}
			}

			else if (LevelController.deviceMode == 0)
			{
				if (inputMode == 1)
				{
					if ( (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) || (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) ) h = 0;
					else if (Input.GetKey(KeyCode.A)) h = -1;
					else if (Input.GetKey(KeyCode.D)) h = 1;

					if ( (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)) || (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) ) v = 0;
					else if (Input.GetKey(KeyCode.S)) v = -1;
					else if (Input.GetKey(KeyCode.W)) v = 1;

					if (Input.GetKeyDown (KeyCode.C) && ammo > 0)
						FireWeapon();

					/*// check for double tap of down key to use a mine
					if (Input.GetKeyDown (KeyCode.S) && mineCount > 0)
					{
						if (lastPressedDown < 0.5f)	UseMine();
						else lastPressedDown = 0;
					}
					else
						lastPressedDown += Time.deltaTime;*/
				}
				else if (inputMode == 2)
				{
					if ( (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow)) || (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow)) ) h = 0;
					else if (Input.GetKey(KeyCode.LeftArrow)) h = -1;
					else if (Input.GetKey(KeyCode.RightArrow)) h = 1;

					if ( (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow)) || (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)) ) v = 0;
					else if (Input.GetKey(KeyCode.DownArrow)) v = -1;
					else if (Input.GetKey(KeyCode.UpArrow)) v = 1;
					
					if ( (Input.GetKeyDown (KeyCode.RightShift) || Input.GetKeyDown (KeyCode.Keypad0)) && ammo > 0)
						FireWeapon();

					/* // check for double tap of down key to use a mine
					if (Input.GetKeyDown (KeyCode.DownArrow) && mineCount > 0)
					{
						if (lastPressedDown < 0.5f)	UseMine();
						else lastPressedDown = 0;
					}
					else
						lastPressedDown += Time.deltaTime;*/
				}
				else if (inputMode == 3)
				{
					if ( (Input.GetKey(KeyCode.J) && Input.GetKey(KeyCode.L)) || (!Input.GetKey(KeyCode.J) && !Input.GetKey(KeyCode.L)) ) h = 0;
					else if (Input.GetKey(KeyCode.J)) h = -1;
					else if (Input.GetKey(KeyCode.L)) h = 1;

					if ( (Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.K)) || (!Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K)) ) v = 0;
					else if (Input.GetKey(KeyCode.K)) v = -1;
					else if (Input.GetKey(KeyCode.I)) v = 1;
					
					if (Input.GetKeyDown (KeyCode.Period) && ammo > 0)
						FireWeapon();

					/* // check for double tap of down key to use a mine
					if (Input.GetKeyDown (KeyCode.K) && mineCount > 0)
					{
						if (lastPressedDown < 0.5f)	UseMine();
						else lastPressedDown = 0;
					}
					else if (lastPressedDown < 0.5f)
						lastPressedDown += Time.deltaTime;*/
				}

				// if v and h are both non-zero, we need to limit the diagonal movement
				if ( v != 0 && h != 0 )
				{
					v *= 0.8f;
					h *= 0.8f;
				}
			}

			//float curX = playerTrans.position.x;
			//horiz = Mathf.SmoothDamp(horiz, moveAmtH * h, ref velocFloatH, agility * Time.deltaTime);
			//float tarX = Mathf.Clamp(curX + horiz * Time.deltaTime, leftBorder, rightBorder);
			//float tarX = curX + horiz * Time.deltaTime;
			float tarX = Mathf.Clamp(playerTrans.position.x + h * moveAmtH * Time.deltaTime, leftBorder, rightBorder);

			//float curY = playerTrans.position.y;
			//vert = Mathf.SmoothDamp(vert, moveAmtV * v, ref velocFloatV, agility * Time.deltaTime);
			//float tarY = Mathf.Clamp(curY + vert * Time.deltaTime, transform.parent.position.y + 30, transform.parent.position.y + 202);
			//float tarY = curY + vert * Time.deltaTime;
			float tarY = Mathf.Clamp(playerTrans.position.y + v * moveAmtV * Time.deltaTime, transform.parent.position.y + 30, transform.parent.position.y + 202);

			Vector3 outVector = new Vector3(tarX, tarY, 0);	
			//outVector.x = Mathf.Clamp(outVector.x, leftBorder, rightBorder);
			//outVector.y = Mathf.Clamp(outVector.y, transform.parent.position.y + 30, transform.parent.position.y + 202);

			transform.position = outVector;
		}

		if (!LevelController.isPaused && playerHP > 0)
		{
			if (treeCollisionPoints > 0)
			{
				if (colTimer >= 0.3f)
				{
					colTimer = 0;
					PlayerTakeDamage(1, "Tree");
				}
				else
					colTimer += Time.deltaTime;
			}
			else if (colTimer > 0.21f)
				colTimer = Mathf.Max (0.2f,colTimer - Time.deltaTime);
			else if (colTimer < 0.19f)
				colTimer = Mathf.Min (0.2f, colTimer + Time.deltaTime);
		}
	}

	// method to fire the user's weapon
	void FireWeapon()
	{
		// reduce ammo counnt and instantiate shot
		ammo -= 1;
		Vector3 thisShotPos = new Vector3(playerTrans.position.x, playerTrans.position.y + weaponShootOffset, playerTrans.position.z);
		GameObject newShot = (GameObject) Instantiate(bulletPrefab[shipIndex], thisShotPos, Quaternion.identity);
		newShot.GetComponent<BulletScript>().playerOwner = gameObject;

		// if the player has the penetrating shots powerup, give penetration to the bullet script
		if (isPenetrating)
			newShot.GetComponent<BulletScript>().isPenetrating = true;

		// set the shot counter to -1 for this zone
		zoneCountShots = -1;

		// adjust ammo visual lights
		Color ammoLightColor = ammoLights[ammo].color;
		ammoLightColor.a = 0.35f;

		// if the user is currently recharging an ammo slot, swap that slot down to this slot visually and turn that old slot off
		if (rechargingSlot > -1)
		{
			rechargingSlot -= 1;

			ammoLights[rechargingSlot + 1].color = ammoLightColor;
		}

		// otherwise simply dull out the current ammo slot
		else
			ammoLights[ammo].color = ammoLightColor;

		// if the ammo was full, we now need to start the ammo recharge checker
		if (ammo == 2)
		{
			StartCoroutine("RechargeAmmoChecker");
		}
	}

	// recharge ammo over time if it is not full
	IEnumerator RechargeAmmoChecker()
	{
		// wait for any currently running recharges to finish
		yield return new WaitForSeconds(rechargeTime);

		while (ammo < 3)
		{
			rechargingSlot = ammo;
			StartCoroutine("RechargeAmmoSlot");

			float t = 0;
			while (t < rechargeTime)
			{
				if (!LevelController.isPaused) t += Time.deltaTime;
				yield return null;
			}
		}
	}


	// recharge the given ammo slot
	IEnumerator RechargeAmmoSlot()
	{
		Color lerpColor = ammoLights[rechargingSlot].color;
		float startAlpha = lerpColor.a;

		float t = 0;
		while (t < 1)
		{
			if (!LevelController.isPaused)
			{
				t += Time.deltaTime;
				lerpColor.a = Mathf.Lerp(startAlpha, 0.6f, t);
				ammoLights[rechargingSlot].color = lerpColor;
			}

			yield return null;
		}

		lerpColor.a = 1;
		ammoLights[rechargingSlot].color = lerpColor;

		ammo += 1;
		rechargingSlot = -1;
		StopCoroutine("RechargeAmmoSlot");
	}


	// grant the player penetrating bullets for a given duration
	public IEnumerator PlayerGetPenetrating(int thisTime)
	{
		isPenetrating = true;

		// check if the player has all 3 power-ups
		CheckPowerupStatus();

		float t = 0;
		while (t < thisTime)
		{
			if (!LevelController.isPaused) t += Time.deltaTime;
			yield return null;
		}

		isPenetrating = false;
	}


	// grant the player a shield for a given duration
	public IEnumerator PlayerGetShield(int thisTime)
	{
		boxShieldedCount = 0;
		isShielded = true;

		// check if the player has all 3 power-ups
		CheckPowerupStatus();

		Color thisColor = bubbleSpriteRend.color;
		thisColor.a = 0;
		bubbleSpriteRend.color = thisColor;
		bubbleSpriteRend.enabled = true;

		float t = 0;
		while( t < 0.25f )
		{
			if (!LevelController.isPaused)
			{
				t += Time.deltaTime;
				thisColor.a = 4 * t;
				bubbleSpriteRend.color = thisColor;
			}

			yield return null;
		}

		thisColor.a = 1;
		bubbleSpriteRend.color = thisColor;

		t = 1.1f;
		while (t < thisTime)
		{
			if (!LevelController.isPaused) t += Time.deltaTime;
			yield return null;
		}

		// blink the shield before it disappears
		t = 0.25f;
		while (t > 0.1f)
		{
			if (!LevelController.isPaused)
			{
				t -= Time.deltaTime;
				thisColor.a = 4 * t;
				bubbleSpriteRend.color = thisColor;
			}
			yield return null;
		}

		while (t < 0.25f)
		{
			if (!LevelController.isPaused)
			{			
				t += Time.deltaTime;
				thisColor.a = 4 * t;
				bubbleSpriteRend.color = thisColor;
			}
			yield return null;
		}

		while (t > 0.1f)
		{
			if (!LevelController.isPaused)
			{
				t -= Time.deltaTime;
				thisColor.a = 4 * t;
				bubbleSpriteRend.color = thisColor;
			}
			yield return null;
		}
		
		while (t < 0.25f)
		{
			if (!LevelController.isPaused)
			{			
				t += Time.deltaTime;
				thisColor.a = 4 * t;
				bubbleSpriteRend.color = thisColor;
			}
			yield return null;
		}

		while (t > 0f)
		{
			if (!LevelController.isPaused)
			{
				t -= Time.deltaTime;
				thisColor.a = 4 * t;
				bubbleSpriteRend.color = thisColor;
			}

			yield return null;
		}

		thisColor.a = 0;
		bubbleSpriteRend.color = thisColor;
		bubbleSpriteRend.enabled = false;
		isShielded = false;
	}


	// if the player had not picked up a powerup this zone, set the counter to -1. Also, if check if player earned triple power-up achievement
	void CheckPowerupStatus()
	{
		zoneCountPowerups = -1;

		if (isShielded && isRecentlyHealed && isPenetrating) StatManager.UnlockAchievement(6);
	}


	// player picked up a mine powerup
	public IEnumerator PlayerGetMine(Transform powerUpTrans)
	{
		if (mineCount < 3)
		{
			powerUpTrans.parent = playerTrans;
			yield return null;
			
			powerUpTrans.localScale = new Vector3(0.58f, 0.58f, 1);

			if (mineCount == 0) powerUpTrans.localPosition = new Vector3(0,9,-6);
			else if (mineCount == 1) powerUpTrans.localPosition = new Vector3(0,8,-8);
			else if (mineCount == 2) powerUpTrans.localPosition = new Vector3(0,6,-9);

			powerUpTrans.GetComponent<SpriteRenderer>().enabled = true;

			mineCount++;

			mineArr[mineCount - 1] = powerUpTrans.gameObject;
		}

		else
		{
			Destroy (powerUpTrans.gameObject);
		}
	}


	// used to detect collisions with obstacles
	void OnTriggerEnter(Collider other)
	{
		if (other.collider.gameObject.tag == "Box") // (colliderCooldown <= 0 && 
		{
			BoxScript thisBoxScript = other.collider.gameObject.GetComponent<BoxScript>();
			thisBoxScript.GetHit(gameObject, false);
			
			if (thisBoxScript.boxType <= 0 && playerHP > 0)
			{
				PlayerTakeDamage(1, "Box");
			}
		}
	}

	// Collision detects when player is rubbing against a collider and should likely take damage over time
	void OnCollisionEnter(Collision other)
	{
		if (other.collider.gameObject.tag == "Tree")
		{
			treeCollisionPoints++;

			if (treeCollisionPoints == 1)
			{
				StopCoroutine("TakeDamageOverTime");

				// position the particle emitter appropriately
				foreach(ContactPoint contact in other.contacts)
				{
					if (contact.otherCollider.tag == "Tree")
					{
						collisionSys.transform.localPosition = (contact.point - playerTrans.position) * 2f;
						float thisXDeg = Mathf.Rad2Deg *  contact.normal.x;

						if  (thisXDeg < 0) thisXDeg += 360;
						else if (thisXDeg >= 360) thisXDeg -= 360;

						collisionSys.transform.rotation = Quaternion.Euler( thisXDeg, 90, 90);
						break;
					}
				}

				StartCoroutine("TakeDamageOverTime");
			}
		}
	}

	void OnCollisionExit(Collision other)
	{
		if (other.collider.gameObject.tag == "Tree")
		{
			treeCollisionPoints--;

			if (treeCollisionPoints < 0) treeCollisionPoints = 0;
		}
	}



	// method to deal damage to the player (if not shielded) and play audio based on the source
	public void PlayerTakeDamage(int amt, string sourceStr)
	{
		if (!testMode)
		{
			if (!isShielded)
			{
				// damage taken counter set to -1
				zoneCountDamage = -1;

				gameCam.DoShakeEffect();

				Color barColor = hpBar[0].color;
				barColor.a = 0.35f;

				int oldHP = playerHP;
				playerHP -= amt;

				for (int i = Mathf.Max(0, playerHP) ; i < oldHP ; i++)
				{	
					hpBar[i].color = barColor;
				}			
				
				if (playerHP <= 0)
				{
					if (sourceStr == "Mine")
						AudioSource.PlayClipAtPoint(mineHitAudio,playerTrans.position);
					else if (sourceStr == "Box" || sourceStr == "Tree")
						AudioSource.PlayClipAtPoint(bigHitAudio,playerTrans.position);
					
					PlayerDeath();
				}
				else
				{
					if (sourceStr == "Mine")
						AudioSource.PlayClipAtPoint(mineHitAudio,playerTrans.position);
					else if (sourceStr == "Box")
						AudioSource.PlayClipAtPoint(hitAudio,playerTrans.position);	
				}
			}
			
			else
			{
				AudioSource.PlayClipAtPoint(shieldedHitAudio,playerTrans.position);

				boxShieldedCount++;
				if (boxShieldedCount > 11) StatManager.UnlockAchievement(7);
			}
		}
	}


	// deal damage over time while player is in contact with at least 1 collider
	IEnumerator TakeDamageOverTime()
	{
		if (!testMode)
		{
			// quickly fade in the scraping audio and particles
			scrapeAudio.Play();
			collisionSys.enableEmission = true;

			collisionSys.emissionRate = 80;

			for (float t = 0 ; t < 0.2f ; t += Time.deltaTime)
			{
				scrapeAudio.volume = 5 * t;
				yield return null;
			}

			scrapeAudio.volume = 1;

			while (treeCollisionPoints > 0 && playerHP > 0)
				yield return null;

			collisionSys.emissionRate = 0;

			// quickly fade out the scraping audio and particles once the collision points drops below 1
			for (float t = 0.2f ; t > 0 ; t -= Time.deltaTime)
			{
				scrapeAudio.volume = 5 * t;
				yield return null;
			}
			
			scrapeAudio.volume = 0;
			scrapeAudio.Pause();

			collisionSys.enableEmission = false;
		}
	}


	// method to heal the player X ticks of hp
	public void PlayerGetHealed(int healInt)
	{
		StopCoroutine("RecentlyHealed");

		isRecentlyHealed = true;

		// check if the player has all 3 power-ups
		CheckPowerupStatus();

		StartCoroutine("RecentlyHealed");		

		for (int i = 0 ; i < healInt ; i++)
		{
			if (playerHP >= 7)
				break;
			else
			{
				playerHP += 1;
			
				Color barColor = hpBar[playerHP - 1].color;
				barColor.a = 1f;
				hpBar[playerHP - 1].color = barColor;
			}
		}
	}


	// player is classified as recently healed for a moment following picking up a healing crate
	IEnumerator RecentlyHealed()
	{
		float t = 0;
		while( t < 2 )
		{
			if (!LevelController.isPaused) t += Time.deltaTime;
			yield return null;
		}

		isRecentlyHealed = false;
	}


	// method to kill the player
	void PlayerDeath()
	{
		canMove = false;
		collider.enabled = false;
		StopCoroutine("RechargeAmmo");
		playerTrans.parent = null;;

		playerThrusters[shipIndex].emissionRate = 0;

		AudioSource.PlayClipAtPoint(hitAudio,Vector3.zero);

		levelController.PlayerDied(inputMode);
	}


	// method called by the level controller upon crossing from one zone into another, and used to check for zone specific achievements
	public void CrossedIntoZone()
	{
		// Check if player is eligibile for each zone achievement based on counters. If counters are negative they were failed and are reset to 0

		// first check whether the player took damage
		if (zoneCountDamage < 0) zoneCountDamage = 0;
		else 
		{
			zoneCountDamage++;
			if (zoneCountDamage > 1) StatManager.UnlockAchievement(9);
		}

		// next check whether the player fired a shot
		if (zoneCountShots < 0) zoneCountShots = 0;
		else
		{
			zoneCountShots++;
			if (zoneCountDamage > 1 && zoneCountShots > 1) StatManager.UnlockAchievement(10);
		}

		// finally check whether the player collected a powerup
		if (zoneCountPowerups < 0) zoneCountPowerups = 0;
		else
		{
			zoneCountPowerups++;
			if (zoneCountDamage > 1 && zoneCountPowerups > 1) StatManager.UnlockAchievement(11);
			if (zoneCountDamage > 1 && zoneCountShots > 1 && zoneCountPowerups > 1) StatManager.UnlockAchievement(12);
		}
	}


	/*
	// method to drop a mine
	void UseMine()
	{
		mineCount--;
		Vector3 thisDropPos = new Vector3(playerTrans.position.x, playerTrans.position.y + weaponDropOffset, playerTrans.position.z);
		GameObject newMine = (GameObject) Instantiate(minePrefab[inputMode - 1], thisDropPos, Quaternion.identity);
		newMine.GetComponent<MineScript>().playerOwner = playerTrans;

		Destroy(mineArr[mineCount]);
		mineArr[mineCount] = null;
	}*/
}
