using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour 
{
	// script attached to each bullet in order to control collisions

	public SceneFader sceneFader;
	public PlayerControls playerControls;
	public EnemyManager enemyManager;

	public float speed;
	public AudioClip playerHitAudio;

	// RRR
	public GameObject explosionPrefab, healthyExplosionPrefab;
	private Color defaultStartColor;

	// ------------------------------


	// RRR
	void Start()
	{
		defaultStartColor = particleSystem.startColor;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		//-- will move the bullet up unless the speed has not been set or is basically 0
		if(speed > 0.1f)
		{
			this.transform.Translate(Vector3.up * Time.deltaTime * speed);
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

	//-- once the bullet collider has been hit
	void OnTriggerEnter2D(Collider2D other) 
	{
		//-- if the object hit is an enemy destorys the enemy and moves the bullet off screen
		if(other.gameObject.tag == "Enemy") 
		{
			// RRR
			enemyManager.KilledEnemy();

			collider2D.enabled = false;
			StartCoroutine("BulletHitEnemy", other.transform);
			StartCoroutine("MoveBulletToHome");
		}

		//-- if the object hit is the boundary then moves the bullet off screen
		else if(other.gameObject.tag == "Boundary")
		{
			if (PlayerStats.isAlive)
			{
				AudioSource.PlayClipAtPoint(playerHitAudio, Vector3.zero);
				playerControls.ShakeCam();

				if (PlayerStats.LoseHealth() <= 0) sceneFader.StartCoroutine("FadeToGameMenu");
				else sceneFader.DoFadeDamage();
			}

			// RRR
			collider2D.enabled = false;
			StartCoroutine("MoveBulletToHome");
		}

		//-- if the object hit is a healthy cell, destroy the healthy cell and send this bullet home
		if(other.gameObject.tag == "Healthy") 
		{
			collider2D.enabled = false;
			other.collider2D.enabled = false;
			StartCoroutine("BulletHitHealthyCell", other.transform);
			StartCoroutine("MoveBulletToHome");
		}
	}

	
	// RRR method to visually handle a bullet hitting an enemy unit over time
	IEnumerator BulletHitEnemy(Transform enemyTrans)
	{		
		GameObject explosionObj = Instantiate (explosionPrefab, transform.position, Quaternion.identity) as GameObject;
		
		enemyTrans.GetComponent<Enemy>().speed = 0;		
		enemyTrans.position = EnemyManager.enemyHome;
		
		yield return new WaitForSeconds(1);
		
		Destroy(explosionObj);
	}


	// RRR method to handle visual of bullet hitting a healthy cell
	IEnumerator BulletHitHealthyCell(Transform cellTrans)
	{
		GameObject explosionObj = Instantiate (healthyExplosionPrefab, transform.position, Quaternion.identity) as GameObject;
		cellTrans.position = new Vector3(0,-10,0);

		yield return new WaitForSeconds(1);
		
		Destroy(explosionObj);
	}
	
	
	// RRR method to move a bullet to the pool home location after waiting a given time
	IEnumerator MoveBulletToHome ()
	{		
		particleSystem.emissionRate = 0;
		speed = 0;
		
		yield return new WaitForSeconds(1);
		
		particleSystem.Clear();
		transform.position = PlayerControls.bulletHome;
		collider2D.enabled = true;
		particleSystem.startColor = defaultStartColor;
	}
}
