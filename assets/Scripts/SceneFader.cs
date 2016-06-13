using UnityEngine;
using System.Collections;

public class SceneFader : MonoBehaviour 
{
	// class attached to the cover camera and in charge of fading in and out of scenes. This gameObject will also have the screen side blockers parented to it

	private SpriteRenderer cover;
	public GameObject gameMenuObj;
	public SpriteRenderer gameCover;
	public static bool isFading;

	public AudioClip buttonAudio;

	public float adPlayTime;
	public SpriteRenderer adRenderer;

	// ---------------------------------


	void Start()
	{
		isFading = true;

		PlayerStats.isAlive = true;
		PlayerStats.health = 3;

		cover = GetComponent<SpriteRenderer>();

		// if this version of the game has ads, if we are not in the menu scene, and if the player's most recent player lasted more than 20 seconds, show an ad
		if (GameSettings.hasAds == 1 && Application.loadedLevel != 0) StartCoroutine("FadeAndShowAd", adPlayTime);
		else StartCoroutine("FadeInOverTime");
	}


	// fade in the current scene with an add of specified play duration
	IEnumerator FadeAndShowAd(float adPlayTime)
	{
		while (cover == null)
			yield return null;

		Color gameCoverColor = gameCover.color;
		Color coverColor = cover.color;
		coverColor.a = 1;
		cover.color = coverColor;

		// TODO: this will be replaced with actual ads in future		
		adRenderer.enabled = true;
		gameCover.color = Color.black;

		// fade out the cover to show the ad
		for( float t = 0.5f ; t > 0 ; t -= Time.deltaTime)
		{
			coverColor.a =  2 * t;
			cover.color = coverColor;
			yield return null;
		}

		coverColor.a = 0;
		cover.color = coverColor;

		// wait for the given ad display/play time
		yield return new WaitForSeconds(adPlayTime);

		// fade the cover back in
		for (float t = 0 ; t < 0.5f ; t += Time.deltaTime)
		{
			coverColor.a =  2 * t;
			cover.color = coverColor;
			yield return null;
		}

		coverColor.a =  1;
		cover.color = coverColor;

		// disable the ad visual, then fade out to the actual game
		adRenderer.enabled = false;
		gameCover.color = gameCoverColor; 

		for (float t = 0.5f ; t > 0 ; t -= Time.deltaTime)
		{
			coverColor.a =  2 * t;
			cover.color = coverColor;
			yield return null;
		}

		cover.enabled = false;
		yield return null;
		
		isFading = false;
	}


	// fade in the current scene
	IEnumerator FadeInOverTime()
	{
		while (cover == null)
			yield return null;

		Color coverColor = cover.color;
		coverColor.a = 1;
		cover.color = coverColor;

		yield return new WaitForSeconds(0.2f);

		for (float t = 0.5f ; t > 0 ; t -= Time.deltaTime)
		{
			coverColor.a =  2 * t;
			cover.color = coverColor;
			yield return null;
		}

		cover.enabled = false;
		yield return null;

		isFading = false;
	}


	public void FadeToScene(int scene)
	{
		if (!isFading)
		{
			isFading = true;

			StopCoroutine("FadeInOverTime");
			StartCoroutine("FadeToSceneOverTime", scene);
		}
	}


	IEnumerator FadeToSceneOverTime(int scene)
	{
		Color coverColor = cover.color;
		coverColor.a = 0;
		cover.color = coverColor;
		cover.enabled = true;

		for (float t = 0 ; t < 0.5f ; t += Time.deltaTime)
		{
			coverColor.a =  2 * t;
			cover.color = coverColor;
			yield return null;
		}

		coverColor.a = 1;
		cover.color = coverColor;

		yield return null;

		Application.LoadLevel(scene);
	}


	// method called by the playerControl script upon the player dying
	public IEnumerator FadeToGameMenu()
	{
		GameSettings.SendHighScore(EnemyManager.playerScore);

		Color coverColor = gameCover.color;
		float startA = coverColor.a;
		float startR = coverColor.r;

		gameMenuObj.SetActive(true);
		gameMenuObj.transform.localScale = Vector3.zero;

		// fade in the game cover
		for (float t = 0 ; t < 0.5f ; t += Time.deltaTime)
		{
			gameMenuObj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, 2 * t);
			coverColor.a = Mathf.Lerp (startA, 0.6f, 2 * t);
			coverColor.r = Mathf.Lerp (startR, 0, 2 * t);
			gameCover.color = coverColor;

			yield return null;
		}

		// while loop to check for the user's input
		while (true)
		{
			if (Input.GetMouseButtonDown(0))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 50f))
				{
					if (hit.collider.gameObject.name.Equals("ButtonRetry"))
					{
						AudioSource.PlayClipAtPoint(buttonAudio, Vector3.zero);
						gameMenuObj.SetActive(false);
						FadeToScene(Application.loadedLevel);
						break;
					}

					else if (hit.collider.gameObject.name.Equals("ButtonMenu"))
					{
						AudioSource.PlayClipAtPoint(buttonAudio, Vector3.zero);
						gameMenuObj.SetActive(false);
						FadeToScene(0);
						break;
					}
				}
			}

			yield return null;
		}
	}


	// public method to start the fade damage coroutine, after stopping any already running fade damage routines
	public void DoFadeDamage()
	{
		StopCoroutine("FadeDamage");
		StartCoroutine("FadeDamage");
	}


	// upon taking damage, the screen briefly fades in and out a visual effect
	IEnumerator FadeDamage()
	{
		Color coverColor = gameCover.color;
		coverColor.a = 0.3f;
		gameCover.color = coverColor;
		
		// fade out the game cover quickly
		for (float t = 0.6f ; t > 0f ; t -= Time.deltaTime)
		{
			coverColor.a = t / 2;
			gameCover.color = coverColor;
			yield return null;
		}

		coverColor.a = 0;
		gameCover.color = coverColor;
	}
}
