using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHUD : MonoBehaviour
{
	private Actor player;

	public GameObject heartPrefab;

	public Sprite heartSprite;
	public Sprite halfHeartSprite;

	public Canvas healthCanvas;
	public Canvas shadowCanvas;

	private List<GameObject> heartList;

	public int pixelSpacing;

	private GameObject healthListHead;
	private int headIndex;
	private float hudHealth = 0;

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
				Camera main = player.getCamera();
				Vector3 startPos = this.transform.position;
				startPos.z = 1;

				healthCanvas.worldCamera = main;
				shadowCanvas.worldCamera = main;

				healthCanvas.transform.SetParent(main.transform, false);
				healthCanvas.transform.SetLocalPositionAndRotation(startPos, Quaternion.identity);

				float newX = (healthCanvas.pixelRect.width / -2) + (this.transform.localScale.x / 2);
				float newY = (healthCanvas.pixelRect.height / 2) - this.transform.localScale.y;
				this.transform.SetLocalPositionAndRotation(new Vector3(newX, newY), Quaternion.identity);

				healthCanvas.sortingLayerName = GameManager.UI_LAYER;
				shadowCanvas.sortingLayerName = GameManager.UI_LAYER;

				break;
			}
		}

		heartList = new List<GameObject>();

		float i;
		for (i = 1; i <= player.actorData.health; i ++)
		{
			addHeart(heartSprite);
		}

		if (i - player.actorData.health > 0)
		{
			addHeart(halfHeartSprite);
		}
	}

	// Update is called once per frame
	void Update()
	{
	}

	private void addHeart(Sprite heart)
	{
		GameObject newHeart = Instantiate(heartPrefab, this.transform);
		newHeart.transform.SetLocalPositionAndRotation(new Vector2(heartList.Count * pixelSpacing * pixelSize, 0), Quaternion.identity);
		heartList.Add(newHeart);
		refillHeart(heart);
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

			/* move the head to the empty heart if necessary */
			updateListHead();
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
			/* remove hearts */
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

			if (i - damage > 0)
			{
				damageHeart();
				refillHeart(halfHeartSprite);
			}
		}
		else
		{
			/* refill hearts */
			float i;
			for (i = damage; i <= -1; i ++)
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
		if (hudHealth % 1 != 0)
		{
			headIndex++;
		}

		/* don't do anything if trying to refill past max */
		if (headIndex >= heartList.Count)
		{
			headIndex = heartList.Count - 1;
		}
		healthListHead = heartList[headIndex];
	}
}