using UnityEngine;
using System.Collections;

public class HealthyCell : MonoBehaviour 
{
	// script attached to a healthy cell that heals the player as long as it is not destroyed

	public AudioClip healAudio;
	private Transform trans;

	// ---------------------------------


	void Start () 
	{
		trans = transform;
	}


	// method called from the enemyManager script to initiate this cell's movement routine
	public void SpawnCell(Vector3 spawnPos, float moveSpeed)
	{
		StopCoroutine("MoveCell");

		trans.position = spawnPos;
		collider2D.enabled = true;

		StartCoroutine("MoveCell", moveSpeed);
	}


	// method to move the cell over time
	IEnumerator MoveCell(float moveSpeed)
	{

		while (trans.position.y > -10)
		{
			trans.position = new Vector3 (trans.position.x, trans.position.y - (moveSpeed * Time.deltaTime), 0);
			yield return null;
		}

		collider2D.enabled = false;
	}


	// upon entering the boundary trigger, the cell has reached the player healing point and should heal the player
	void OnTriggerEnter2D (Collider2D other)
	{
		if (PlayerStats.isAlive && other.tag == "Player")
		{
			Debug.Log ("Trig");

			AudioSource.PlayClipAtPoint(healAudio, Vector3.zero);
			collider2D.enabled = false;

			if (PlayerStats.health < 3)
			{
				PlayerControls.hearts[PlayerStats.health].color = PlayerControls.healthFullColor;
				PlayerStats.health++;
			}
		}
	}
}
