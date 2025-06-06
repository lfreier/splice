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

	private static int MINE_INDEX = 0;
	private static float SPORE_CLOUD_RADIUS = 0.4375F;
	private static float SUMM_CMD_FORCE_TIME = 0.75F;

	private float bufferTimer = 0F;
	private GameManager gameManager;

	private void OnDestroy()
	{
		gameManager.playerAbilityEvent -= abilityInputPressed;
		gameManager.playerAbilitySecondaryEvent -= abilityInputSecondaryPressed;
		gameManager.playerSecondaryEvent -= abilitySecondaryPressed;
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

	/* summoning zombies */
	private void abilityInputPressed()
	{
		Vector2 placingLoc = actorWielder.transform.position + actorWielder.transform.up * 0.5625F;
		if (bufferTimer <= 0 && sporeCloudPrefab != null)
		{
			Instantiate(sporeCloudPrefab, placingLoc, actorWielder.transform.rotation, null);
		}

		//TODO: play animation
		RaycastHit2D[] corpses = Physics2D.CircleCastAll(placingLoc, SPORE_CLOUD_RADIUS, Vector2.zero, ActorDefs.GLOBAL_PICKUP_RADIUS, LayerMask.GetMask(ActorDefs.corpseLayer));
		if (corpses == null)
		{
			bufferTimer = MutationDefs.ABILITY_BUFF_TIMER;
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
		if (mutationScriptable.values[MINE_INDEX] <= actorWielder.gameManager.playerStats.getMutationBar() && bufferTimer <= 0)
		{
			actorWielder.gameManager.playerStats.changeMutationBar(Mathf.RoundToInt(-mutationScriptable.values[MINE_INDEX]));
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

			move._detection = ActorDefs.detectMode.forced;
			move.forcedTimer = SUMM_CMD_FORCE_TIME;
			move.idlePath = new Vector2[] { pointerLoc };
			move.idlePathPauseTime = new float[] { 10000F };
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