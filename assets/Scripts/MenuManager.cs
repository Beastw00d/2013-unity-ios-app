using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour 
{
	// main class in charge of menu behavior and input

	public SceneFader sceneFader;

	public int currentHasAds, currentGameVersion;

	public int[] versionInts = new int[]{0,0,0};
	public SpriteRenderer[] versionRendArr;
	public Sprite[] numSprites;

	public AudioClip clickUnmuteAudio, clickOnAudio, clickOffAudio, advanceSceneAudio;

	public Sprite audioMuteSprite, audioUnmuteSprite;
	public SpriteRenderer buttonAudioRend;

	// gameobjects for social buttons and visual script for audio button
	public GameObject buttonTwitter, buttonFacebook, howToPlayObj;
	public VisualEffects scriptVisualFacebook, scriptVisualAudio, scriptVisualExit;

	private bool lockButtons = false;


	// ---------------------------------


	void Start()
	{
		InitializeGamecenter();
		SetGameVersion();
		SetVolume();
		
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
					
					if (buttonName.Equals("MiniButtonMute"))
					{
						if (GameSettings.globalVolume != 0)
						{
							AudioListener.volume = 0;
							GameSettings.globalVolume = 0;
							buttonAudioRend.sprite = audioMuteSprite;
						}
						else
						{
							AudioSource.PlayClipAtPoint(clickUnmuteAudio, Vector3.zero);
							AudioListener.volume = 1;
							GameSettings.globalVolume = 1;
							buttonAudioRend.sprite = audioUnmuteSprite;
						}

						PlayerPrefs.SetInt("Volume", GameSettings.globalVolume);
					}

					else if (buttonName.Equals("MiniButtonTwitter"))
					{
						AudioSource.PlayClipAtPoint(clickOnAudio, Vector3.zero);
						GameSettings.SendSocialPost(1);
					}

					else if (buttonName.Equals("MiniButtonFacebook"))
					{
						AudioSource.PlayClipAtPoint(clickOnAudio, Vector3.zero);
						GameSettings.SendSocialPost(2);
					}

					else if (buttonName.Equals("MiniButtonExit"))
					{
						lockButtons = true;
						Application.Quit();
					}

					else if (buttonName.Equals("ButtonHowToPlay"))
					{
						AudioSource.PlayClipAtPoint(clickOnAudio, Vector3.zero);
						lockButtons = true;
						howToPlayObj.SetActive(true);

						StartCoroutine("ShowingHowToPlay");
					}

					else if (buttonName.Equals("ButtonLeaderboard"))
					{
						AudioSource.PlayClipAtPoint(clickOnAudio, Vector3.zero);
						GameSettings.ViewLeaderboard();
					}
					
					else if (buttonName.Equals("ButtonStart"))
					{
						AudioSource.PlayClipAtPoint(advanceSceneAudio, Vector3.zero);
						
						lockButtons = true;
						sceneFader.FadeToScene(1);
					}
				}
			}
		}
	}


	// make an intital connection to gamecenter and allow/disallow certain options based on iOS version and capabilities
	void InitializeGamecenter()
	{
		// if this device does not support social posting, disable the social minibuttons and center the audio minibutton
		if (GameSettings.ConnectToGameCenter() == false)
		{
			buttonTwitter.SetActive(false);
			buttonFacebook.SetActive(false);
			scriptVisualExit.defPos.x = scriptVisualAudio.defPos.x;
			scriptVisualAudio.defPos.x = scriptVisualFacebook.defPos.x;
		}
	}

	// method to set the game volume based upon the stored player volume preference (if available)
	void SetVolume()
	{
		if (GameSettings.globalVolume == -1) 
		{
			int storedVolume = PlayerPrefs.GetInt("Volume", 1);
			AudioListener.volume = GameSettings.globalVolume = storedVolume;
		}
		
		else if (GameSettings.globalVolume == 0)
		{
			AudioListener.volume = 0;
			buttonAudioRend.sprite = audioMuteSprite;
		}
		
		else
		{
			AudioListener.volume = GameSettings.globalVolume = 1;
			buttonAudioRend.sprite = audioUnmuteSprite;
		}
	}


	// method to set the game version static variable and display the game version in the main menu scene
	void SetGameVersion()
	{
		GameSettings.hasAds = currentHasAds;
		GameSettings.gameVersion = currentGameVersion;
		
		// for loop updates the game version sprite renderer sprites
		for (int i = 0 ; i < 3 ; i++)
		{
			versionRendArr[i].sprite = numSprites[versionInts[i]];
		}
	}


	// routine that runs while user has the instructions open and ends upon recieving user input
	IEnumerator ShowingHowToPlay()
	{
		bool hasTouched = false;

		while(!hasTouched)
		{
			int i = 0;
			while (i < Input.touchCount)
			{
				if (Input.GetTouch(i).phase == TouchPhase.Began)
					hasTouched = true;
			}
			
			yield return null;
		}

		AudioSource.PlayClipAtPoint(clickOffAudio, Vector3.zero);
		lockButtons = false;
		howToPlayObj.SetActive(false);
	}
}
