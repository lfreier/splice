using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHUD : MonoBehaviour
{
	private Actor player;

	public TextMeshProUGUI cellText;

	public GameObject heartPrefab;

	public Sprite heartSprite;
	public Sprite halfHeartSprite;

	public Canvas healthCanvas;
	public Canvas shadowCanvas;
	public RectTransform textTransform;

	public AudioSource musicPlayer;
	private float musicVolume;

	private List<GameObject> heartList;

	public int pixelSpacing;

	private GameObject healthListHead;
	private int headIndex;
	private float hudHealth = 0;

	private PlayerStats stats;

	private static float pixelSize = 0.0625F;

	// Use this for initialization
	void Start()
	{
		foreach (Actor actor in FindObjectsByType<Actor>(FindObjectsSortMode.None))
		{
			if (actor.tag == ActorDefs.playerTag)
			{
				player = actor;
				player.hud = this;
				PlayerInteract interactScript = player.gameObject.GetComponent<PlayerInteract>();
				interactScript.stats.hud = this;
				Camera main = player.getCamera();

				healthCanvas.worldCamera = main;
				shadowCanvas.worldCamera = main;

				healthCanvas.transform.SetParent(main.transform, false);

				this.transform.localScale *= Screen.width / (1080 * 1.5F);
				shadowCanvas.transform.localScale *= Screen.width / (1080 * 1.5F);
				textTransform.localScale *= Screen.width / (1080 * 1.5F);

				healthCanvas.sortingLayerName = GameManager.UI_LAYER;
				shadowCanvas.sortingLayerName = GameManager.UI_LAYER;

				musicPlayer.Play();

				break;
			}
		}

		heartList = new List<GameObject>();

		float i;
		for (i = 0; i < player.actorData.health - 0.5F; i ++)
		{
			addHeart();
			refillHeart(heartSprite);
		}

		/* add an extra half heart */
		if (player.actorData.health % 1 != 0)
		{
			addHeart();
			refillHeart(halfHeartSprite);
		}
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void updateCells(int count)
	{
		cellText.SetText("" + count);
	}

	private void addHeart()
	{
		GameObject newHeart = Instantiate(heartPrefab, this.transform);
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
	private void removeHeart()
	{

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