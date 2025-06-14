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
	public GameObject sporeCloudPrefab;

	public Sprite abilityIcon1;
	public Sprite abilityIcon2;

	private static int MINE_COST_INDEX = 0;
	private static float SPORE_CLOUD_RADIUS = 0.4375F;
	private static float SUMM_CMD_FORCE_TIME = 0.5F;

	private float bufferTimer = 0F;
	private GameManager gameManager;

	private void OnDestroy()
	{
		gameManager.playerAbilityEvent -= abilityInputPressed;
		gameManager.playerAbilitySecondaryEvent -= abilityInputSecondaryPressed;
		gameManager.playerSecondaryEvent -= abilitySecondaryPressed;
		gameManager.updateCellCount -= updateCells;
	}

	void Start()
	{
		summons = new List<EnemyMove>();
	}

	void FixedUpdate()
	{
		if (bufferTimer != 0)
		{
			bufferTimer -= Time.deltaTime;
			if (bufferTimer <= 0)
			{
				bufferTimer = 0;
			}
		}
	}

	private void updateCells(int amount)
	{
		gameManager.playerStats.playerHUD.setMutAbilityFill(mutationScriptable.mutCost, mutationScriptable.values[MINE_COST_INDEX]);
	}

	/* summoning zombies */
	private void abilityInputPressed()
	{
		Vector2 placingLoc = actorWielder.transform.position + actorWielder.transform.up * 0.5625F;
		if (bufferTimer <= 0 && sporeCloudPrefab != null)
		{
			Instantiate(sporeCloudPrefab, placingLoc, actorWielder.transform.rotation, null);
			bufferTimer = MutationDefs.ABILITY_BUFF_TIMER;
		}

		//TODO: play animation
		RaycastHit2D[] corpses = Physics2D.CircleCastAll(placingLoc, SPORE_CLOUD_RADIUS, Vector2.zero, ActorDefs.GLOBAL_PICKUP_RADIUS, LayerMask.GetMask(ActorDefs.corpseLayer));
		if (corpses == null || corpses.Length <= 0)
		{
			return;
		}
		foreach (RaycastHit2D rayHit in corpses)
		{
			Corpse checkCorpse = rayHit.collider.gameObject.GetComponent<Corpse>();
			if (checkCorpse != null && bufferTimer <= 0
				&& mutationScriptable.mutCost <= actorWielder.gameManager.playerStats.getMutationBar())
			{
				actorWielder.gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
				raiseZombie(checkCorpse);
				break;
			}
		}
		bufferTimer = MutationDefs.ABILITY_BUFF_TIMER;
	}

	/* placing mines */
	private void abilityInputSecondaryPressed()
	{
		if (mutationScriptable.values[MINE_COST_INDEX] <= actorWielder.gameManager.playerStats.getMutationBar() && bufferTimer <= 0)
		{
			actorWielder.gameManager.playerStats.changeMutationBar(Mathf.RoundToInt(-mutationScriptable.values[MINE_COST_INDEX]));
			Vector2 placingLoc = actorWielder.transform.position + actorWielder.transform.up * 0.75F;
			RaycastHit2D[] hits = Physics2D.RaycastAll(actorWielder.transform.position, placingLoc - (Vector2)actorWielder.transform.position, (placingLoc - (Vector2)actorWielder.transform.position).magnitude + 0.5F, gameManager.unwalkableLayers.value);
			if (hits != null && hits.Length > 0)
			{
				placingLoc = actorWielder.transform.position;
			}
			Instantiate(sporeMinePrefab, placingLoc, actorWielder.transform.rotation, null);
			bufferTimer = MutationDefs.ABILITY_BUFF_TIMER;
		}
	}

	/* commanding summons */
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

			if (move._detection == ActorDefs.detectMode.forced)
			{
				move._nextForcedDetection = ActorDefs.detectMode.idle;
			}
			else
			{
				move._nextForcedDetection = move._detection;
			}
			move._detection = ActorDefs.detectMode.forced;
			move.forcedTimer = SUMM_CMD_FORCE_TIME;
			move.idlePath = new Vector2[] { pointerLoc };
			move.idlePathPauseTime = new float[] { 10000F };
			move.moveTarget = pointerLoc;
			move.actor.actorBody.rotation = move.actor.aimAngle(pointerLoc);
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
		gameManager.updateCellCount += updateCells;
		pInputs = wielder.gameObject.GetComponent<PlayerInputs>();
		pInteract = wielder.gameObject.GetComponentInChildren<PlayerInteract>();

		if (abilityIcon1 != null && abilityIcon2 != null)
		{
			gameManager.playerStats.playerHUD.abilityIconImage1.sprite = abilityIcon1;
			gameManager.playerStats.playerHUD.abilityIconImage2.sprite = abilityIcon2;
		}
		else
		{
			gameManager.playerStats.playerHUD.abilityIconImage1.sprite = null;
			gameManager.playerStats.playerHUD.abilityIconImage2.sprite = null;
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
		this.actorWielder = wielder;
	}

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return mutationScriptable.tutorialSprites;
	}
}