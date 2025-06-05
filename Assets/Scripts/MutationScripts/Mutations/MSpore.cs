using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MSpore : MonoBehaviour, MutationInterface
{
	public MutationScriptable mutationScriptable;

	public Actor actorWielder;
	public PlayerInteract pInteract;
	public PlayerInputs pInputs;

	public List<EnemyMove> summons;

	public Sprite icon;

	public GameObject sporeMinePrefab;

	private static int MINE_INDEX = 0;
	private GameManager gameManager;

	private void OnDestroy()
	{
		gameManager.playerAbilityEvent -= abilityInputPressed;
		gameManager.playerAbilitySecondaryEvent -= abilityInputSecondaryPressed;
		gameManager.playerSecondaryEvent -= abilitySecondaryPressed;
	}

	public void Start()
	{
		summons = new List<EnemyMove>();
	}

	private void abilityInputPressed()
	{
		if (pInteract == null)
		{
			return;
		}

		//TODO: play animation
		RaycastHit2D[] corpses = Physics2D.CircleCastAll(pInteract.interactCollider.bounds.center, ActorDefs.GLOBAL_PICKUP_RADIUS, Vector2.zero, ActorDefs.GLOBAL_PICKUP_RADIUS, LayerMask.GetMask(ActorDefs.corpseLayer));
		if (corpses == null)
		{
			return;
		}
		foreach (RaycastHit2D rayHit in corpses)
		{
			Corpse checkCorpse = rayHit.collider.gameObject.GetComponent<Corpse>();
			if (checkCorpse != null
				&& mutationScriptable.mutCost <= actorWielder.gameManager.playerStats.getMutationBar())
			{
				actorWielder.gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
				raiseZombie(checkCorpse);
				break;
			}
		}
	}

	private void abilityInputSecondaryPressed()
	{
		if (mutationScriptable.values[MINE_INDEX] <= actorWielder.gameManager.playerStats.getMutationBar())
		{
			actorWielder.gameManager.playerStats.changeMutationBar(Mathf.RoundToInt(-mutationScriptable.values[MINE_INDEX]));
			Vector2 placingLoc = actorWielder.transform.position + actorWielder.transform.up * 0.75F;
			RaycastHit2D[] hits = Physics2D.RaycastAll(actorWielder.transform.position, placingLoc - (Vector2)actorWielder.transform.position, (placingLoc - (Vector2)actorWielder.transform.position).magnitude + 0.5F, gameManager.unwalkableLayers.value);
			if (hits != null && hits.Length > 0)
			{
				placingLoc = actorWielder.transform.position;
			}
			Instantiate(sporeMinePrefab, placingLoc, actorWielder.transform.rotation, null);
		}
	}

	private void abilitySecondaryPressed()
	{
		if (summons == null || summons.Count <= 0)
		{
			return;
		}

		Vector2 pointerLoc = pInputs.pointerPos();
		GameObject click = Instantiate(actorWielder.gameManager.prefabManager.clickPrefab, pointerLoc, Quaternion.identity, null);

		for (int i = 0; i < summons.Count; i++)
		{
			EnemyMove move = summons[i];
			if (move == null)
			{
				summons.RemoveAt(i);
				i--;
				continue;
			}

			move.usingPathfinding = true;
			move._detection = ActorDefs.detectMode.idle;
			move.idlePath = new Vector2[] { pointerLoc };
			move.pathIndex = 0;
		}
	}

	public bool raiseZombie(Corpse corpse)
	{
		if (corpse != null && corpse.corpseSprite != null && actorWielder.gameManager.prefabManager.zombiePrefabs.Length > (int)corpse.type)
		{
			GameObject newZombie = Instantiate(actorWielder.gameManager.prefabManager.zombiePrefabs[(int)corpse.type], corpse.transform.position, corpse.transform.rotation, null);
			if (newZombie != null)
			{
				Destroy(corpse.corpseSprite);
				Destroy(corpse.pickupCollider);
				Destroy(corpse);
				newZombie.SetActive(true);

				EnemyMove summonMove = newZombie.GetComponent<EnemyMove>();
				if (summonMove != null)
				{
					summons.Add(summonMove);
				}
				return true;
			}
		}

		return false;
	}

	public string getDisplayName()
	{
		return MutationDefs.NAME_SPORE;
	}

	public Sprite getIcon()
	{
		return icon;
	}

	public string getId()
	{
		return "MSpore";
	}

	public void init(Actor wielder)
	{
		gameManager = GameManager.Instance;
		setWielder(wielder);
		gameManager.playerAbilityEvent += abilityInputPressed;
		gameManager.playerSecondaryEvent += abilitySecondaryPressed;
		gameManager.playerAbilitySecondaryEvent += abilityInputSecondaryPressed;
		pInputs = wielder.gameObject.GetComponent<PlayerInputs>();
		pInteract = wielder.gameObject.GetComponentInChildren<PlayerInteract>();
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.ACTIVE_SLOT;
	}

	public void trigger(Actor actorTarget)
	{

	}

	public MutationInterface mEquip(Actor actor)
	{
		init(actor);
		return this;
	}
	public void setStartingPosition()
	{

	}

	public void setWielder(Actor wielder)
	{
		this.actorWielder = wielder;
	}

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return mutationScriptable.tutorialSprites;
	}
}