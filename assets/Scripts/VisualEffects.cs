using UnityEngine;
using System.Collections;

public class VisualEffects : MonoBehaviour 
{
	// Script that is capable of producing various visual effects based on the public bools that are checked

	public bool isPulsing, isMoving, isFlickering;

	private Transform trans;
	public Vector3 defPos;
	private float defScale, randScale, randX, randY;

	private SpriteRenderer rend;
	private Color rendColor;


	// -----------------------------------


	// get the necessary variables based on the checked bools
	void Start ()
	{
		trans = transform;

		if (isPulsing)
		{
			defScale = trans.localScale.x;
			randScale = Random.Range(4f, 6f);
		}

		if (isMoving)
		{
			defPos = trans.position;
			randX = Random.Range(1.5f, 2.5f);
			randY = (3.75f / randX);
		}

		if (isFlickering)
		{
			rend = GetComponent<SpriteRenderer>();
			rendColor = rend.color;
		}
	}


	// do the assigned visual effects in each update loop
	void Update () 
	{		
		// pulsing size
		if (isPulsing)
		{
			float mod = defScale + Mathf.Lerp (-0.01f, 0.01f, 0.5f + Mathf.Sin(Time.time * randScale) / 2);
			trans.localScale = new Vector3(mod, mod, 1);
		}

		// randomized movement
		if (isMoving)
		{
			float xPos = Mathf.Sin(Time.time * randX) / 75;
			float yPos = Mathf.Sin(Time.time * randY) / 75;
			trans.position = new Vector3(defPos.x + xPos, defPos.y + yPos, defPos.z);
		}


		// light flickering effect for title
		if (isFlickering)
		{
			if (Random.Range(0,4) == 0)
			{
				rendColor.a = 0.95f + Mathf.Sin(Time.time * 40) / 20;
				rend.color = rendColor;
			}
		}
	}
}
