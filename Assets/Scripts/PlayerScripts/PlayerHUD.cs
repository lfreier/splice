using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHUD : MonoBehaviour
{
	private Actor player;

	public Canvas[] hudCanvas;

	public GameObject healthHudObject;

	public TextMeshProUGUI cellText;
	public Image mutationFill;

	public GameObject heartPrefab;

	public Sprite heartSprite;
	public Sprite halfHeartSprite;

	public Canvas shadowCanvas;
	public RectTransform textTransform;
	public int pixelSpacing;

	public AudioSource musicPlayer;
	private float musicVolume;

	private List<GameObject> heartList;

	private GameObject healthListHead;
	private int headIndex;
	private float hudHealth = 0;

	private Vector3 hudStartScale;
	private Vector3 shadowStartScale;
	private Vector3 textStartScale;

	private static float pixelSize = 0.0625F;
	private GameManager gameManager;

	private void Start()
	{
		gameManager = GameManager.Instance;
		gameManager.initHudEvent += init;
		gameManager.updateHealthEvent += updateHealth;
		gameManager.muteEvent += mute;
		gameManager.updateCellCount += updateCells;
		foreach (Canvas canvas in hudCanvas)
		{
			canvas.gameObject.SetActive(false);
		}
		hudStartScale = transform.localScale;
		shadowStartScale = shadowCanvas.transform.localScale;
		textStartScale = textTransform.localScale;
	}

	private void OnDestroy()
	{
		gameManager.initHudEvent -= init;
		gameManager.muteEvent -= mute;
		gameManager.updateHealthEvent -= updateHealth;
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
				Camera main = Camera.main;
				foreach (Canvas canvas in hudCanvas)
				{
					canvas.worldCamera = main;
					canvas.sortingLayerName = GameManager.UI_LAYER;
				}

				this.transform.localScale = hudStartScale * Screen.width / (1080 * 1.5F);
				shadowCanvas.transform.localScale = shadowStartScale * Screen.width / (1080 * 1.5F);
				textTransform.localScale = textStartScale * Screen.width / (1080 * 1.5F);

				if (musicPlayer.enabled && !musicPlayer.isPlaying)
				{
					musicPlayer.Play();
				}

				break;
			}
		}

		if (player == null)
		{
			return;
		}

		heartList = new List<GameObject>();

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
		if (playerMaxHealth % 1 != 0)
		{
			addHeart();
			refillHeart(halfHeartSprite);
			if (currHealth % 1 != 0)
			{
				refillHeart(halfHeartSprite);
			}
		}

		foreach (Canvas canvas in hudCanvas)
		{
			canvas.gameObject.SetActive(true);
		}
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void updateCells(int count)
	{
		cellText.SetText("" + count);
		mutationFill.fillAmount = gameManager.playerStats.getMutationBar() / gameManager.playerStats.maxMutationBar;
	}

	private void addHeart()
	{
		GameObject newHeart = Instantiate(heartPrefab, healthHudObject.transform);
		newHeart.transform.SetLocalPositionAndRotation(new Vector2(heartList.Count * pixelSpacing * pixelSize, 0), Quaternion.identity);
		heartList.Add(newHeart);

		if (hudHealth == 0)
		{
			healthListHead = heartList[0];
		}
	}

	/* Helper function to make refilling/damaging hearts their own function for readiability
	 */
	private void changeHeart(Sprite heart)
	{
		if (healthListHead.transform.childCount < 1)
		{
			//error
			return;
		}
		GameObject heartSpriteObj = healthListHead.transform.GetChild(0).gameObject;
		SpriteRenderer toChange = heartSpriteObj.GetComponent<SpriteRenderer>();

		if (toChange != null)
		{
			if (heart == null)
			{
				if (toChange.sprite == halfHeartSprite)
				{
					hudHealth -= 0.5F;
				}
				else
				{
					hudHealth -= 1;
				}
			}
			else if (heart == halfHeartSprite)
			{
				hudHealth += 0.5F;
			}
			/* filling a half heart from a half heart */
			else if (toChange.sprite == halfHeartSprite)
			{
				hudHealth += 0.5F;
			}
			else
			{
				hudHealth += 1;
			}

			toChange.sprite = heart;
			toChange.size = new Vector2(1, 1);
		}
		
		updateListHead();
	}

	private void damageHeart()
	{
		changeHeart(null);
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
		changeHeart(heart);
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
			for (i = damage; i <= -1; i = hudHealth - newHealth)
			{
				refillHeart(heartSprite);
			}

			if (i < 0)
			{
				refillHeart(halfHeartSprite);
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
	}

	public void mute()
	{
		musicPlayer.mute = !musicPlayer.mute;
		Debug.Log("Setting mute to " + musicPlayer.mute);
	}
}