using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour 
{
	public GameObject playerOwner;
	public float shotSpeed;
	public bool isPenetrating;
	private int boxHitCount = 0;

	// need to inherit the level speed on awake in addition to the bullet's own shot speed. Also give the bullet a maximum life span until it is destroyed
	void Awake()
	{
		shotSpeed += LevelController.levelSpeed;
		StartCoroutine("DestroyInTime", 2f);
	}

	// movement over time based on the given shot speed
	void FixedUpdate()
	{
		if (!LevelController.isPaused)
		{
			rigidbody.MovePosition( new Vector3(rigidbody.position.x, rigidbody.position.y + shotSpeed * Time.fixedDeltaTime, rigidbody.position.z));

			// extra effect for penetrating bullets
			if (isPenetrating)
				transform.Rotate( Vector3.up, 360 * Time.fixedDeltaTime);
		}
	}

	// on trigger enter used to detect hits with obstacles and "destroy" them
	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Box")
		{
			other.gameObject.GetComponent<BoxScript>().GetHit(playerOwner, true);

			if (!isPenetrating)
				Destroy(gameObject);
			else
			{
				boxHitCount++;

				if (boxHitCount > 7) StatManager.UnlockAchievement(8);
			}
		}
	}


	// upon colliding with a tree, destroy the shot
	void OnCollisionEnter(Collision other)
	{
		if (other.collider.tag == "Tree")
		{
			Destroy(gameObject);
		}
	}


	// method to destroy this shot after the given time if it has not hit anything
	IEnumerator DestroyInTime(float waitTime)
	{
		float t = 0;
		while (t < waitTime)
		{
			if (!LevelController.isPaused) t += Time.deltaTime;
			yield return null;
		}

		Destroy(gameObject);
	}
}
