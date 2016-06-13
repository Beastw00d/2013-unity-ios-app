using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour 
{
	// script in charge of the menu scene and responsible for setting appropriate initial game conditions

	public bool testMode;

	// version int and int array in X.xx form (ignoring period)
	public int currentGameVersion;
	public bool currentHasAds;
	public int[] versionInts;

	public Color shipColorDull, shipColorOff, controlsButtonColorOn, controlsButtonColorOff;

	private SceneFader sceneFader;

	public AudioClip clickButtonAudio, advanceSceneAudio, clickControlsOnAudio, clickControlsOffAudio, clickUnmuteAudio;

	public SpriteRenderer[] shipLightA, shipLightB, shipLightC, shipNameRend, shipWhiteRends, lockRends, versionRendArr;
	public SpriteRenderer backgroundRend, buttonControlsRend, buttonAudioRend, buttonAchievementsRend, moveTiltRend, moveStickRend;

	public Sprite audioMuteSprite, audioUnmuteSprite;
	public Sprite[] numSprites;

	public Transform selectorTrans;
	public SpriteRenderer[] selectorRend;
	public ParticleSystem selectorSys;
	private Vector3 selectorPos;
	public float bobAmt;

	public Color[] shipColors;
	public Color[] particleColors;

	// bitarray of which ships are locked and unlocked
	private BitArray lockedShips = new BitArray(4, true);

	private int selectedShip = 1;

	private bool lockButtons = false, achievementsShowing = false;

	// ----------------------------------

	void Start()
	{
		// if the statmanager has not retrieve a high score from gamecenter, tell it to do so
		if (StatManager.highScore < 0) StatManager.GetGameData();

		if (testMode == true)
		{
			for (int i = 0 ; i < StatManager.achievements.Length ; i++)
			{
				StatManager.achievements[i] = true;
			}
		}

		// show the appropriate controls based on the stored controlModeInt
		if(GameMode.controlModeInt == 1)
		{
			moveTiltRend.enabled = true;
			moveStickRend.enabled = false;
		}
		else
		{
			moveStickRend.enabled = true;
			moveTiltRend.enabled = false;
		}

		SetGameVersion();
		SetShipLocks();

		if (GameMode.globalVolume == 0)
		{
			AudioListener.volume = 0;
			buttonAudioRend.sprite = audioMuteSprite;
		}
		else
		{
			if (GameMode.globalVolume == -1) GameMode.globalVolume = 1;

			AudioListener.volume = 1;
			buttonAudioRend.sprite = audioUnmuteSprite;
		}

		sceneFader = GameObject.FindWithTag("Cover").GetComponent<SceneFader>();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && !lockButtons)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 100))
			{
				if (hit.collider.tag == "Button")
				{
					string buttonName = hit.collider.gameObject.name;

					if (buttonName.Equals("ButtonMute"))
					{
						if (GameMode.globalVolume != 0)
						{
							AudioListener.volume = 0;
							GameMode.globalVolume = 0;
							buttonAudioRend.sprite = audioMuteSprite;
						}
						else
						{
							AudioSource.PlayClipAtPoint(clickUnmuteAudio, Vector3.zero);
							AudioListener.volume = 1;
							GameMode.globalVolume = 1;
							buttonAudioRend.sprite = audioUnmuteSprite;
						}
					}
					else if (buttonName.Equals("Crimson") && selectedShip != 1)
					{
						selectedShip = 1;
						selectorTrans.position = new Vector3(selectorTrans.position.x, hit.collider.transform.position.y, selectorTrans.position.z);
						selectorRend[0].color = selectorRend[1].color = shipColors[0];
						selectorSys.startColor = particleColors[0];
						HighlightPlayers();
					}
					else if (buttonName.Equals("Aqua") && selectedShip != 2 && lockedShips[1] == false)
					{
						selectedShip = 2;
						selectorTrans.position = new Vector3(selectorTrans.position.x, hit.collider.transform.position.y, selectorTrans.position.z);
						selectorRend[0].color = selectorRend[1].color = shipColors[1];
						selectorSys.startColor = particleColors[1];
						HighlightPlayers();
					}
					else if (buttonName.Equals("Orange") && selectedShip != 3 && lockedShips[2] == false)
					{
						selectedShip = 3;
						selectorTrans.position = new Vector3(selectorTrans.position.x, hit.collider.transform.position.y, selectorTrans.position.z);
						selectorRend[0].color = selectorRend[1].color = shipColors[2];
						selectorSys.startColor = particleColors[2];
						HighlightPlayers();
					}
					else if (buttonName.Equals("Green") && selectedShip != 4 && lockedShips[3] == false)
					{
						selectedShip = 4;
						selectorTrans.position = new Vector3(selectorTrans.position.x, hit.collider.transform.position.y, selectorTrans.position.z);
						selectorRend[0].color = selectorRend[1].color = shipColors[3];
						selectorSys.startColor = particleColors[3];
						HighlightPlayers();
					}

					else if (buttonName.Equals("ButtonControls"))
					{
						/*if (backgroundRend.sortingOrder != -5) 
						{
							AudioSource.PlayClipAtPoint(clickControlsOnAudio, Vector3.zero);
							backgroundRend.sortingOrder = -5;
							buttonControlsRend.color = controlsButtonColorOn;
						}
						else
						{
							AudioSource.PlayClipAtPoint(clickControlsOffAudio, Vector3.zero);
							backgroundRend.sortingOrder = -2;
							buttonControlsRend.color = controlsButtonColorOff;
						}*/

						AudioSource.PlayClipAtPoint(clickControlsOnAudio, Vector3.zero);

						if(GameMode.controlModeInt == 0)
						{
							GameMode.controlModeInt = 1;
							moveTiltRend.enabled = true;
							moveStickRend.enabled = false;
						}
						else
						{
							GameMode.controlModeInt = 0;
							moveStickRend.enabled = true;
							moveTiltRend.enabled = false;
						}
					}

					else if (buttonName.Equals("ButtonAchievements"))
					{
						if (!achievementsShowing)
						{
							achievementsShowing = true;
							AudioSource.PlayClipAtPoint(clickControlsOnAudio, Vector3.zero);
							//buttonAchievementsRend.color = controlsButtonColorOn;

							// TODO: code to show achievements
						}

						else
						{
							achievementsShowing = false;
							AudioSource.PlayClipAtPoint(clickControlsOffAudio, Vector3.zero);
							//buttonAchievementsRend.color = controlsButtonColorOff;

							// TODO: code to hide achievements
						}
					}

					else if (buttonName.Equals("ButtonStart"))
					{
						AudioSource.PlayClipAtPoint(advanceSceneAudio, Vector3.zero);

						lockButtons = true;
						GameMode.selectedShip = selectedShip;
						GameMode.startingLevel = GameMode.currentLevel = 0;
						sceneFader.FadeToScene(1);
					}
				}
			}
		}

		selectorPos = selectorTrans.position;
		selectorPos.x = -37 + Mathf.Sin(8 * Time.time) * bobAmt; 
		selectorTrans.position = selectorPos;
	}


	// method to set the game version static variable and display the game version in the main menu scene
	void SetGameVersion()
	{
		GameMode.hasAds = currentHasAds;
		GameMode.gameVersion = currentGameVersion;

		// for loop updates the game version sprite renderer sprites
		for (int i = versionInts.Length - 1 ; i > -1 ; i--)
		{
			versionInts[i]++;
			
			if (versionInts[i] > 9)
			{
				versionInts[i] -= 10;
				versionRendArr[i].sprite = numSprites[versionInts[i]];
			}
			else
			{
				versionRendArr[i].sprite = numSprites[versionInts[i]];
				break;
			}
		}
	}
	
	
	// method to visually highlight the selected player count and unhighlight nonselected player counts
	void HighlightPlayers()
	{
		AudioSource.PlayClipAtPoint(clickButtonAudio, Vector3.zero);

		for (int i = 0 ; i < 4 ; i++)
		{
			if (i == selectedShip - 1)
			{
				shipLightA[i].color = shipLightB[i].color = shipLightC[i].color = shipNameRend[i].color = shipColors[i];
				shipWhiteRends[i].color = Color.white;
			}
			else
			{
				shipLightA[i].color = shipLightB[i].color = shipLightC[i].color = shipColorOff;
				shipWhiteRends[i].color = shipNameRend[i].color = shipColorDull;
			}
		}
	}


	// make sure the ships that this player has not unlocked are marked as locked and unable to be selected
	void SetShipLocks()
	{
		lockedShips[0] = false;

		for (int i = 1 ; i < 4 ; i++)
		{
			lockedShips[i] = StatManager.GetLocks(i);
			if (lockedShips[i] == false) lockRends[i].enabled = false;
		}
	}
}
