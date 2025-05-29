using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MSpore : MonoBehaviour, MutationInterface
{
	public MutationScriptable mutationScriptable;

	public Actor wielder;
	public PlayerInteract pInteract;
	public PlayerInputs pInputs;

	public List<EnemyMove> summons;

	public Sprite icon;

	private void OnDestroy()
	{
		GameManager.Instance.playerAbilityEvent -= abilityInputPressed;
		GameManager.Instance.playerSecondaryEvent -= abilitySecondaryPressed;
	}

	public void Start()
	{
		summons = new List<EnemyMove>();
	}

	private void abilityInputPressed()
	{
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
				&& mutationScriptable.mutCost <= wielder.gameManager.playerStats.getMutationBar())
			{
				wielder.gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
				raiseZombie(checkCorpse);
				break;
			}
		}
	}

	private void abilitySecondaryPressed()
	{
		Vector2 pointerLoc = pInputs.pointerPos();
		GameObject click = Instantiate(wielder.gameManager.prefabManager.clickPrefab, pointerLoc, Quaternion.identity, null);

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
		if (corpse != null && corpse.corpseSprite != null && wielder.gameManager.prefabManager.zombiePrefabs.Length > (int)corpse.type)
		{
			GameObject newZombie = Instantiate(wielder.gameManager.prefabManager.zombiePrefabs[(int)corpse.type], corpse.transform.position, corpse.transform.rotation, null);
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
		setWielder(wielder);
		GameManager.Instance.playerAbilityEvent += abilityInputPressed;
		GameManager.Instance.playerSecondaryEvent += abilitySecondaryPressed;
		pInputs = wielder.gameObject.GetComponent<PlayerInputs>();
		pInteract = wielder.gameObject.GetComponentInChildren<PlayerInteract>();
		if (pInteract != null && pInteract.interactCollider != null)
		{
			pInteract.interactCollider.includeLayers |= (1 << LayerMask.NameToLayer(ActorDefs.corpseLayer));
		}
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
		this.wielder = wielder;
	}

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return mutationScriptable.tutorialSprites;
	}
}