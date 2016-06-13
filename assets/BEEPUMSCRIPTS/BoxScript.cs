using UnityEngine;
using System.Collections;

public class BoxScript : MonoBehaviour 
{
	// 0 is standand, 1 is health, 2 is penetration, 3 is shield
	public int boxType;
	public Transform powerUpTrans;
	public SpriteRenderer powerUpRend;

	public AudioClip breakAudio;
	public Sprite spriteBoxBroken;
	private int hasHitInt = 0;

	// need to use input int to make sure only 1 instance of this effect plays when hit multiple times in the same frame
	public void GetHit(GameObject thisPlayer, bool playBreakAudio)
	{
		hasHitInt ++;
		
		if (hasHitInt == 1)
		{
			StartCoroutine(DoGetHitEffect(thisPlayer, playBreakAudio));
		}
	}
	
	// method to "destroy" the box and if it is a powerup crate, give the powerup to the appropriate player
	IEnumerator DoGetHitEffect(GameObject thisPlayer, bool playBreakAudio)
	{
		SpriteRenderer thisRend = gameObject.GetComponent<SpriteRenderer>();
		Sprite startSprite = thisRend.sprite;
		Color startColor = thisRend.color;
		Color thisColor = startColor;
		
		gameObject.collider.enabled = false;
		thisRend.sprite = spriteBoxBroken;

		if (playBreakAudio || boxType > 0)
			AudioSource.PlayClipAtPoint(breakAudio, gameObject.transform.position);

		if (powerUpRend != null)
			powerUpRend.enabled = false;

		// check for the box type in order to call the right method
		if (boxType == 1) StartCoroutine("DoPowerupHP", thisPlayer);
		else if (boxType == 2) StartCoroutine("DoPowerupPen", thisPlayer);
		else if (boxType == 3) StartCoroutine("DoPowerupShield", thisPlayer);
		else if (boxType == 4) StartCoroutine("DoPowerupMine", thisPlayer);

		for (float t=0 ; t < 0.6f ; t+= Time.deltaTime)
		{
			thisColor.a = Mathf.Max(0,0.5f - t);
			thisRend.color = thisColor;
			
			yield return null;
		}
		
		thisRend.enabled = false;
		
		yield return null;
		
		thisRend.color = startColor;
		thisRend.sprite = startSprite;
		hasHitInt = 0;
	}


	// if this renderer is the active one for the player (player has not refreshed), set the player's curPowerArr slot to null, then destroy crate and powerup
	void CleanUp(GameObject thisPlayer)
	{
		if (boxType != 4 && thisPlayer != null)
		{
			if (thisPlayer.GetComponent<PlayerControl>().curPowerupArr[boxType - 1] == powerUpRend )
				thisPlayer.GetComponent<PlayerControl>().curPowerupArr[boxType - 1] = null;
			
			if (powerUpTrans != null)
				Destroy(powerUpTrans.gameObject);
		}
		
		Destroy(gameObject);
	}


	// do the health pickup visual and healing effect over time
	IEnumerator DoPowerupHP(GameObject thisPlayer)
	{
		PlayerControl thisPlayerControl = thisPlayer.GetComponent<PlayerControl>();
	
		// if the player already has a heal visual, destroy the old visual and stop the coroutine before starting new one
		if (thisPlayerControl.curPowerupArr[0] != null)
			thisPlayerControl.curPowerupArr[0].enabled = false;
		
		thisPlayerControl.curPowerupArr[0] = powerUpRend;
		thisPlayerControl.PlayerGetHealed(2);

		// begin visual effects
		Color thisColor = powerUpRend.color;
		thisColor.a = 0;
		powerUpRend.color = thisColor;
		
		powerUpTrans.parent = thisPlayer.transform;
		yield return null;

		powerUpTrans.localScale = new Vector3(0.4f, 0.4f, 1);
		powerUpTrans.localPosition = new Vector3(0,-3.5f,13.5f);
		powerUpRend.enabled = true;

		float t=0;
		while (t < 0.2f)
		{
			if (!LevelController.isPaused)
			{
				t += Time.deltaTime;
				thisColor.a = t * 4;
				powerUpRend.color = thisColor;
			}
			yield return null;
		}
		
		thisColor.a = 0.8f;
		powerUpRend.color = thisColor;

		t = 1.6f;
		while (t > 0)
		{
			if (powerUpRend != null)
			{
				if (!LevelController.isPaused)
				{
					t -= Time.deltaTime;					
					thisColor.a = t / 2;
					powerUpRend.color = thisColor;
				}

				yield return null;
			}
			else
				break;
		}

		CleanUp(thisPlayer);
	}


	// do the pen pickup visual and healing effect over time
	IEnumerator DoPowerupPen(GameObject thisPlayer)
	{
		PlayerControl thisPlayerControl = thisPlayer.GetComponent<PlayerControl>();
		
		if (thisPlayerControl.curPowerupArr[1] != null)
		{
			thisPlayerControl.StopCoroutine("PlayerGetPenetrating");
			thisPlayerControl.curPowerupArr[1].enabled = false;
		}
		
		thisPlayerControl.curPowerupArr[1] = powerUpRend;
		thisPlayerControl.StartCoroutine("PlayerGetPenetrating", 8);
		
		// begin visual effects
		Color thisColor = powerUpRend.color;
		thisColor.a = 0;
		powerUpRend.color = thisColor;
		
		powerUpTrans.parent = thisPlayer.transform;
		yield return null;
		
		powerUpTrans.localScale = new Vector3(0.58f, 0.58f, 1);
		powerUpTrans.localPosition = new Vector3(0,-9,8);
		powerUpRend.enabled = true;

		float t=0;
		while (t < 0.2f)
		{
			if (!LevelController.isPaused)
			{
				t += Time.deltaTime;
				thisColor.a = t * 4;
				powerUpRend.color = thisColor;
			}
			yield return null;
		}
		
		thisColor.a = 0.8f;
		powerUpRend.color = thisColor;
		
		t=0;
		while (t < 7.6f)
		{
			if (!LevelController.isPaused) t += Time.deltaTime;
			yield return null;
		}
		
		t = 0.2f;
		while(t > 0)
		{
			if (powerUpRend != null)
			{
				if (!LevelController.isPaused)
				{ 
					t -= Time.deltaTime;
					
					thisColor.a = t * 4;
					powerUpRend.color = thisColor;
				}

				yield return null;
			}
			else
				break;
		}
		
		CleanUp(thisPlayer);
	}


	// do the shield pickup visual and healing effect over time
	IEnumerator DoPowerupShield(GameObject thisPlayer)
	{
		PlayerControl thisPlayerControl = thisPlayer.GetComponent<PlayerControl>();
		
		// if the player already has a penetrating shot powerup, destroy the old visual and stop the coroutine before starting new one
		if (thisPlayerControl.curPowerupArr[2] != null)
		{
			thisPlayerControl.StopCoroutine("PlayerGetShield");
			thisPlayerControl.curPowerupArr[2].enabled = false;
		}
		
		thisPlayerControl.curPowerupArr[2] = powerUpRend;
		thisPlayerControl.StartCoroutine("PlayerGetShield", 5f);
		
		// begin visual effects
		Color thisColor = powerUpRend.color;
		thisColor.a = 0;
		powerUpRend.color = thisColor;
		
		powerUpTrans.parent = thisPlayer.transform;
		yield return null;
		
		powerUpTrans.localScale = new Vector3(0.58f, 0.58f, 1);
		powerUpTrans.localPosition = new Vector3(0,9,8);
		powerUpRend.enabled = true;
		
		float t=0;
		while (t < 0.2f)
		{
			if (!LevelController.isPaused)
			{
				t += Time.deltaTime;
				thisColor.a = t * 4;
				powerUpRend.color = thisColor;
			}
			yield return null;
		}
		
		thisColor.a = 0.8f;
		powerUpRend.color = thisColor;
		
		t=0;
		while (t < 4.6f)
		{
			if (!LevelController.isPaused) t += Time.deltaTime;
			yield return null;
		}

		t = 0.2f;
		while(t > 0)
		{
			if (powerUpRend != null)
			{
				if (!LevelController.isPaused)
				{ 
					t -= Time.deltaTime;
				
					thisColor.a = t * 4;
					powerUpRend.color = thisColor;
				}
				yield return null;
			}
			else
				break;
		}
		
		CleanUp(thisPlayer);
	}


	// do the mine pickup visual and healing effect over time
	IEnumerator DoPowerupMine(GameObject thisPlayer)
	{
		PlayerControl thisPlayerControl = thisPlayer.GetComponent<PlayerControl>();
		
		// based on the player's mine count, place this mine visual in the appropriate slot
		thisPlayerControl.StartCoroutine("PlayerGetMine", powerUpTrans);

		if (powerUpRend != null)
		{
			// begin visual effects
			Color thisColor = powerUpRend.color;
			thisColor.a = 0;
			powerUpRend.color = thisColor;

			yield return null;

			float t=0;

			while (t < 0.2f)
			{
				if (!LevelController.isPaused)
				{
					t += Time.deltaTime;
					thisColor.a = t * 4;
					powerUpRend.color = thisColor;
				}
				yield return null;
			}
		}
		
		CleanUp(thisPlayer);
	}
}
