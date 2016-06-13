using UnityEngine;
using System.Collections;

public class GameCam : MonoBehaviour 
{
	// script to control game cam effects/movements

	private Vector3 defaultLocalPos;
	private Transform trans;
	private float defaultX, curX, velocX;
	public float smoothCam, shakeSpeed, shakeAmt;

	// -----------------------------


	void Start()
	{
		trans = transform;
		defaultLocalPos = trans.localPosition;
		defaultX = curX = defaultLocalPos.x;
	}


	public void DoShakeEffect()
	{
		StopCoroutine("ShakeOverTime");
		StartCoroutine("ShakeOverTime");
	}

	// camera shake effect over time
	IEnumerator ShakeOverTime()
	{
		float t = 0.4f;
		while (t > 0)
		{
			if (!LevelController.isPaused)
			{
				t -= Time.deltaTime;
				curX = Mathf.SmoothDamp(curX, defaultX + t * shakeAmt * Mathf.Sin(shakeSpeed * Time.deltaTime), ref velocX, smoothCam * Time.deltaTime);
				trans.localPosition = new Vector3(curX, defaultLocalPos.y, defaultLocalPos.z);
			}

			yield return null;
		}
	}
}
