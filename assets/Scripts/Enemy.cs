using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	// RRR
	public SceneFader sceneFader;
	public PlayerControls playerControls;
	public AudioClip playerHitAudio;
	public float speed;
	private Transform trans;
	private float myRand;
	
	// -----------------------------

	// RRR
	void Start()
	{
		trans = transform;
		myRand = Random.Range(0.75f, 0.85f);
	}

	// Update is called once per frame
	void Update () 
	{
		//-- will move the enemy up unless the speed has not been set or is basically 0
		if(speed > 0.1f)
		{
			trans.position += Vector3.down * Time.deltaTime * speed;

			// RRR
			float scaleMod = myRand +  Mathf.Sin(10 * Time.time *  (0.2f + myRand)) / 40;
			trans.localScale = new Vector3(scaleMod, scaleMod, 1);
		}
	}
	
	public float GetSpeed() 
	{
		return speed;
	}
	
	public void SetSpeed(float speedSet)
	{
		speed = speedSet;
	}
	
	//-- once the enemy collider has been hit
	void OnTriggerEnter2D(Collider2D other) 
	{
		//-- if the object hit is the Player puts the enemy back to off screen 
		if(other.gameObject.tag == "Player") 
		{
			this.gameObject.transform.position = EnemyManager.enemyHome;
			speed = 0;

			if (PlayerStats.isAlive)
			{
				AudioSource.PlayClipAtPoint(playerHitAudio, Vector3.zero);
				playerControls.ShakeCam();

				if (PlayerStats.LoseHealth() <= 0) sceneFader.StartCoroutine("FadeToGameMenu");
				else sceneFader.DoFadeDamage();
			}
		}	
	}
}
