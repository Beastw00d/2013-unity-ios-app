using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControls : MonoBehaviour 
{
	// script in charge of taking player input and performing the appropriate actions

	// array of all bullets (and their scripts) in the bullet pool, index of the bullet that will be used next shot 
	public Transform[] bulletArr;
	public Bullet[] bulletScriptArr;
	private int bulletIndex = 0;

	// speed with which bullets are fired, cooldown time of the gun, and audio clip for shooting a gun
	public float bulletSpeed, cooldownTime;
	public AudioClip shootAudio;

	// bullet pool location for inactive bullets
	public static Vector3 bulletHome = new Vector3(20, 20, 0);

	// layer mask used to check where the user is touching in relation to the row colliders
	public LayerMask rowMask;

	// array of row sprite renderers and colliders
	public SpriteRenderer[] rowRendArr;
	public Collider[] rowColArr; 

	// bounds used for determining which part of the screen a touch is in
	public float guiYBound;
	//public float[] rowXBound;

	// array of row gun locations and gun sprites for animation
	public Transform[] gunArr;

	// bit array that returns true for gun is on cooldown and false for not on cooldown (aka can be fired)
	private BitArray gunCooldowns = new BitArray(4, false);

	// the default color and highlight color for rows
	public Color rowColorDefault, rowColorHighlight;

	public SpriteRenderer[] heartsTemp;
	public static SpriteRenderer[] hearts;

	public static Color healthFullColor;

	
	// ------------------------------------


	void Start()
	{
		healthFullColor = heartsTemp[0].color;
		hearts = heartsTemp;
	}


	void Update()
	{
		//GetMouseClick();
		GetTouches();
	}


	// a substitute method for get touches that can be used with a mouse
	void GetMouseClick()
	{
		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit, 100f, rowMask))
			{
				for (int i = 0 ; i < 4 ; i++)
				{
					if (hit.collider.Equals(rowColArr[i]))
					{
						FireGun(i);
						break;
					}
				}
			}
		}
	}
	
	
	// method to check for user touch input each frame, and call for a shot to be fired or a gui menu to open depending on where the user touches
	void GetTouches()
	{
		// while loop checks all possible touches
		int touchIndex = 0;
		while (touchIndex < Input.touchCount)
		{
			if (Input.GetTouch(touchIndex).phase == TouchPhase.Began)
			{
				//Vector2 inputPos = Camera.main.ScreenToViewportPoint(Input.GetTouch(touchIndex).position );
				Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(touchIndex).position);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 100f, rowMask))
				{
					for (int i = 0 ; i < 4 ; i++)
					{
						if (hit.collider.Equals(rowColArr[i]))
						{
							FireGun(i);
							break;
						}
					}
				}
			}

			touchIndex++;
		}
	}

	/* // if the player touches above this range on the Y axis, then handle GUI input
	if (inputPos.y > guiYBound)
		Debug.Log ("GUI PLACEHOLDER LOG");

	// else if the player touched in the actual game area, o
	else if (PlayerStats.health <= 0)
	{
		Debug.Log ("CANNOT SHOOT: player is dead.");
	}

	// else, the player is capable of shooting, so find out which row the user pressed in order to fire a bullet in that row
	else
	{
		for (int i = 0 ; i < rowXBound.Length ; i++)
		{
			if (inputPos.x < rowXBound[i])
			{
				FireGun(i);
				break;
			}
		}
	}*/


	// method to fire the gun associated with the given index
	void FireGun(int gunIndex)
	{
		// as long as this gun index is not on cooldown, set it on cooldown, fire it, and start the coroutine for cooldown
		if (gunCooldowns[gunIndex] == false)
		{
			gunCooldowns[gunIndex] = true;

			StartCoroutine("FireGunOverTime", gunIndex);
			StartCoroutine("GunCooldownOverTime", gunIndex);
		}
	}


	// coroutine to fire and then cool down a gun (plus visual) over time and then set the bool for its cooldown to false
	IEnumerator FireGunOverTime(int gunIndex)
	{
		Transform thisTrans = gunArr[gunIndex];
		
		ParticleSystem bulletSys = bulletArr[bulletIndex].particleSystem;
		Bullet thisBulletScript = bulletScriptArr[bulletIndex];
		bulletArr[bulletIndex].position = thisTrans.position;

		// advance bullet int
		bulletIndex++;
		if (bulletIndex >= bulletArr.Length) bulletIndex = 0;
		
		AudioSource.PlayClipAtPoint(shootAudio, thisTrans.position);

		yield return new WaitForFixedUpdate();

		// enable bullet to move and emit particles
		bulletSys.emissionRate = 20;
		thisBulletScript.speed = bulletSpeed;
	}


	// coroutine to visually fade the row color for user feedback on shot and then set the gun cooldown to false once the set cooldown time has passed
	IEnumerator GunCooldownOverTime(int gunIndex)
	{
		for (float t = 0 ; t < 1 ; t += Time.deltaTime / cooldownTime)
		{
			rowRendArr[gunIndex].color = Color.Lerp(rowColorHighlight, rowColorDefault, t / 2);
			yield return null;
		}
		
		rowRendArr[gunIndex].color = rowColorDefault;
		gunCooldowns[gunIndex] = false;
	}


	// RRR method for visual feedback upon player taking damage
	public void ShakeCam()
	{
		StopCoroutine("ShakeCamOverTime");
		StartCoroutine("ShakeCamOverTime");
	}

	IEnumerator ShakeCamOverTime()
	{
		Transform camTrans = Camera.main.transform;
		
		for (float t = 0.6f ; t > 0 ; t -= Time.deltaTime)
		{
			camTrans.position = new Vector3( t * (Mathf.Sin(Time.time * 20) / 15), 0, -10);
			yield return null;
		}
		
		camTrans.position = new Vector3(0, 0, -10);
	}	
}
