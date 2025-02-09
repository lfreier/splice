using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static GameManager;


public class PlayerHUD : MonoBehaviour
{
	private Actor player;

	public Canvas[] hudCanvas;

	public GameObject healthHudObject;

	public TextMeshProUGUI cellText;
	public Image mutationFill;
	public Canvas mutationFillCanvas;
	public Canvas mutationBGFillCanvas;
	public Image mutationOutline;

	public Image activeItemIcon;

	public GameObject heartPrefab;
	public GameObject shieldPrefab;

	public Sprite heartSprite;
	public Sprite halfHeartSprite;
		
	public Sprite shieldSprite;
	public Sprite halfShieldSprite;

	public Canvas shadowCanvas;
	public RectTransform textTransform;
	public int pixelSpacing;

	public AudioSource musicPlayer;
	private float musicVolume;

	private List<GameObject> heartList;
	private List<GameObject> shieldList;

	private GameObject healthListHead;
	private GameObject shieldListHead;

	private int headIndex;
	public float hudHealth = 0;

	private int shieldHeadIndex;
	public float shieldHealth = 0;

	private Vector3 hudStartScale;
	private Vector3 shadowStartScale;
	private Vector3 textStartScale;

	private static float pixelSize = 0.0625F;
	private GameManager gameManager;

	private bool fadeInMusic = false;
	private float targetVolume;

	public AudioResource[] bgm;

	private void Start()
	{
		gameManager = GameManager.Instance;
		gameManager.initHudEvent += init;
		gameManager.startMusicEvent += startNewMusic;
		gameManager.updateHealthEvent += updateHealth;
		gameManager.updateShieldEvent += updateShield;
		gameManager.muteEvent += mute;
		gameManager.updateCellCount += updateCells;
		foreach (Canvas canvas in hudCanvas)
		{
			canvas.gameObject.SetActive(false);
		}
		hudStartScale = transform.localScale;
		shadowStartScale = shadowCanvas.transform.localScale;
		textStartScale = textTransform.localScale;
		fadeInMusic = false;
	}

	private void OnDestroy()
	{
		gameManager.initHudEvent -= init;
		gameManager.muteEvent -= mute;
		gameManager.startMusicEvent -= startNewMusic;
		gameManager.updateHealthEvent -= updateHealth;
		gameManager.updateShieldEvent -= updateShield;
		gameManager.updateCellCount -= updateCells;
	}

	public void init()
	{
		if (heartList != null)
		{
			resetHeartHUD();
		}

		foreach (Actor actor in FindObjectsByType<Actor>(FindObjectsSortMode.None))
		{
			if (actor.tag == ActorDefs.playerTag)
			{
				player = actor;
				gameManager.playerStats.playerHUD = this;
				if (gameManager.playerStats.player == null)
				{
					gameManager.playerStats.player = actor;
				}
				Camera main = Camera.main;
				foreach (Canvas canvas in hudCanvas)
				{
					canvas.worldCamera = main;
					canvas.sortingLayerName = GameManager.UI_LAYER;
				}

				this.transform.localScale = hudStartScale * Screen.width / (1080 * 1.5F);
				shadowCanvas.transform.localScale = shadowStartScale * Screen.width / (1080 * 1.5F);
				textTransform.localScale = textStartScale * Screen.width / (1080 * 1.5F);

				/* */
				if (player.mutationHolder.transform.childCount > 0)
				{
					gameManager.playerStats.showMutationBar();
				}

				if (musicPlayer.enabled && !musicPlayer.isPlaying)
				{
					if (bgm != null && bgm.Length > 0)
					{
						int bgmIndex = Random.Range(0, bgm.Length);
						musicPlayer.resource = bgm[bgmIndex];
					}
					else if (gameManager.levelManager.currLevelData != null)
					{
						musicPlayer.resource = gameManager.levelManager.currLevelData.sceneMusic.audioClip;
						musicPlayer.volume = gameManager.levelManager.currLevelData.sceneMusic.volume;
					}
				}

				break;
			}
		}

		if (player == null)
		{
			return;
		}

		heartList = new List<GameObject>();
		shieldList = new List<GameObject>();

		float i;
		float playerMaxHealth = player.gameManager.playerStats.getPlayerMaxHealth();
		float currHealth = player.gameManager.playerStats.getPlayerSavedHealth();
		//TODO: This is fucking dumb but w/e
		player.actorData.health = currHealth;
		player.actorData.maxHealth = playerMaxHealth;
		for (i = 0; i < playerMaxHealth - 0.5F; i ++)
		{
			addHeart();
			if (i < currHealth - 0.5F)
			{
				refillHeart(heartSprite);
			}
		}

		/* add an extra half heart */
		if (currHealth % 1 != 0)
		{
			refillHeart(halfHeartSprite);
		}

		foreach (Canvas canvas in hudCanvas)
		{
			canvas.gameObject.SetActive(true);
		}
		/*
		if (musicPlayer != null && musicPlayer.enabled && !musicPlayer.isPlaying)
		{
			musicPlayer.Play();
		}
		*/
	}

	// Update is called once per frame
	void Update()
	{
		if (fadeInMusic)
		{
			musicPlayer.volume += (targetVolume / 128);
			if (musicPlayer.volume >= targetVolume)
			{
				musicPlayer.Play();
				fadeInMusic = false;
			}
		}
	}

	public void startNewMusic(MusicScriptable music)
	{
		if (!musicPlayer.resource.name.Equals(music.audioClip.name))
		{
			musicPlayer.Stop();
			musicPlayer.enabled = true;
			musicPlayer.resource = music.audioClip;
			musicPlayer.volume = 0;
			targetVolume = music.volume;
			fadeInMusic = true;
		}
	}

	public void updateCells(int count)
	{
		cellText.SetText("" + count);
		mutationFill.fillAmount = ((float)gameManager.playerStats.getMutationBar()) / ((float)gameManager.playerStats.getMaxMutationBar());
	}

	public void changeActiveItemIcon(Sprite icon)
	{
		activeItemIcon.enabled = true;
		activeItemIcon.sprite = icon;
	}

	private void addHeart()
	{
		//TODO: move shield when adding hearts
		GameObject newHeart = Instantiate(heartPrefab, healthHudObject.transform);
		newHeart.transform.SetLocalPositionAndRotation(new Vector2(heartList.Count * pixelSpacing * pixelSize, 0), Quaternion.identity);
		heartList.Add(newHeart);

		if (hudHealth == 0)
		{
			healthListHead = heartList[0];
		}
	}

	private void addShieldHeart()
	{
		GameObject newHeart = Instantiate(shieldPrefab, healthHudObject.transform);
		newHeart.transform.SetLocalPositionAndRotation(new Vector2((heartList.Count + shieldList.Count) * pixelSpacing * pixelSize, 0), Quaternion.identity);
		shieldList.Add(newHeart);

		if (shieldHealth == 0)
		{
			shieldListHead = shieldList[0];
		}
	}

	private void clearShields()
	{
		foreach (GameObject shield in shieldList)
		{
			Destroy(shield);
		}

		shieldHeadIndex = 0;
		shieldHealth = 0;
		player.actorData.shield = 0;
		shieldList.Clear();
	}

	private void damageShield()
	{
		changeHeart(null, ref shieldHealth, shieldListHead);
	}

	private void refillShield(Sprite sprite)
	{
		if (shieldHealth % 1 == 0)
		{
			/* don't do anything if trying to refill past max */
			if (shieldHealth >= shieldList.Count)
			{
				return;
			}
			else
			{
				shieldListHead = shieldList[(int)shieldHealth];
			}
		}

		changeHeart(sprite, ref shieldHealth, shieldListHead);
	}

	private void updateShield(float newShield)
	{
		/* add shield, or remove shield
		 * damage is handled normally
		 */
		float damage = shieldHealth - newShield;

		/* make new shield */
		if (shieldHealth <= 0)
		{
			int i;
			for (i = 0; i < newShield - 0.5F; i++)
			{
				addShieldHeart();
				refillShield(shieldSprite);
			}

			/* add an extra half heart */
			if (newShield % 1 != 0)
			{
				addShieldHeart();
				refillShield(halfShieldSprite);
				if (shieldHealth % 1 != 0)
				{
					refillShield(shieldSprite);
				}
			}
		}
		/* damage shield */
		else if (shieldHealth > newShield)
		{
			if (damage > 0)
			{
				float i;

				/* has a half heart right now */
				if (shieldHealth % 1 != 0)
				{
					damageShield();
					damage -= 0.5F;
				}

				for (i = 1; i <= damage; i++)
				{
					damageShield();
				}

				if (shieldHealth != newShield)
				{
					damageShield();
					refillShield(halfShieldSprite);
				}

				/* remove shield hearts from HUD when depleted */
				if (shieldList.Count - shieldHealth >= 1)
				{
					Destroy(shieldList[shieldList.Count - 1]);
					shieldList.RemoveAt(shieldList.Count - 1);
					updateListHead();
				}
			}
		}
		/* refill shield */
		else
		{
			float i;
			for (i = damage; i <= -1; i = shieldHealth - newShield)
			{
				refillShield(shieldSprite);
			}

			if (i < 0)
			{
				refillShield(halfShieldSprite);
			}
		}

		if (shieldHealth <= 0)
		{
			clearShields();
		}
	}

	/* Helper function to make refilling/damaging hearts their own function for readiability
	 */
	private void changeHeart(Sprite newHeart, ref float healthSrc, GameObject listHead)
	{
		if (listHead == null || listHead.transform.childCount < 1)
		{
			Debug.Log("error when changing HUD heart");
			return;
		}
		GameObject heartSpriteObj = listHead.transform.GetChild(0).gameObject;
		SpriteRenderer toChange = heartSpriteObj.GetComponent<SpriteRenderer>();

		if (toChange != null)
		{
			if (newHeart == null)
			{
				/* TOOD: fix hacky shit but w/e it works*/
				if (toChange.sprite.name.Contains("half"))
				{
					healthSrc -= 0.5F;
				}
				else
				{
					healthSrc -= 1;
				}
			}
			else if (newHeart.name.Contains("half"))
			{
				healthSrc += 0.5F;
			}
			/* filling a half heart from a half heart */
			else if (toChange.sprite != null && toChange.sprite.name.Contains("half"))
			{
				healthSrc += 0.5F;
			}
			else
			{
				healthSrc += 1;
			}

			toChange.sprite = newHeart;
			toChange.size = new Vector2(1, 1);
		}
		
		updateListHead();
	}

	private void damageHeart()
	{
		changeHeart(null, ref hudHealth, healthListHead);
	}

	private void refillHeart(Sprite heart)
	{
		if (hudHealth % 1 == 0)
		{
			/* don't do anything if trying to refill past max */
			if (hudHealth >= heartList.Count)
			{
				return;
			}
			else
			{
				healthListHead = heartList[(int)hudHealth];
			}
		}

		changeHeart(heart, ref hudHealth, healthListHead);
	}

	/* Removing max health, should not be done often
	 */
	private void reduceHearts(float heartsToRemove)
	{

	}

	private void resetHeartHUD()
	{
		foreach (GameObject heart in heartList)
		{
			Destroy(heart);
		}
		hudHealth = 0;
	}

	public void updateHealth(float newHealth)
	{
		float damage = hudHealth - newHealth;

		if (damage > 0)
		{
			float i;

			/* has a half heart right now */
			if (hudHealth % 1 != 0)
			{
				damageHeart();
				damage -= 0.5F;
			}

			for (i = 1; i <= damage; i++)
			{
				damageHeart();
			}

			if (hudHealth != newHealth)
			{
				damageHeart();
				refillHeart(halfHeartSprite);
			}
		}
		else
		{
			/* refill hearts */
			float i;
			for (i = damage; i <= -1F; i = hudHealth - newHealth)
			{
				refillHeart(heartSprite);
			}

			if (i < 0)
			{
				if (hudHealth % 1 != 0)
				{
					refillHeart(heartSprite);
				}
				else
				{
					refillHeart(halfHeartSprite);
				}
			}
		}
	}

	private void updateListHead()
	{
		headIndex = (int)hudHealth;

		if (hudHealth % 1 == 0 && headIndex > 0)
		{
			headIndex--;
		}

		/* don't do anything if trying to refill past max */
		if (headIndex >= heartList.Count)
		{
			headIndex = heartList.Count - 1;
		}
		healthListHead = heartList[headIndex];

		shieldHeadIndex = (int)shieldHealth;

		if (shieldHealth % 1 == 0 && shieldHeadIndex > 0)
		{
			shieldHeadIndex--;
		}

		/* don't do anything if trying to refill past max */
		if (shieldHeadIndex >= shieldList.Count)
		{
			shieldHeadIndex = shieldList.Count - 1;
		}

		if (shieldHeadIndex >= 0 && shieldList.Count > 0)
		{
			shieldListHead = shieldList[shieldHeadIndex];
		}
		else
		{
			shieldListHead = null;
		}
	}

	public void mute()
	{
		musicPlayer.mute = !musicPlayer.mute;
		if (musicPlayer.mute == false)
		{
			musicPlayer.Play();
		}
		Debug.Log("Setting mute to " + musicPlayer.mute);
	}
}