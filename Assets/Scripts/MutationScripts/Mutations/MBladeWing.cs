using System.Collections;
using UnityEngine;

public class MBladeWing : MonoBehaviour, MutationInterface
{
	public Sprite icon;
	public Actor actorWielder;

	public float dashSpeed = 1800F;
	public float dashWidth = 1F;

	private bool dashActive = false;

	private Vector3 dashStart;
	private Vector3 dashTarget;

	[SerializeField] public Animator anim;

	[SerializeField] public Collider2D hitbox;

	[SerializeField] public MutationScriptable mutationScriptable;

	[SerializeField] public SoundScriptable soundScriptable;

	private float buffTimer;

	private GameManager gameManager;

	private void OnDestroy()
	{
		GameManager.Instance.playerAbilityEvent -= abilityInputPressed;
	}
	private void FixedUpdate()
	{
		if (dashActive)
		{
			//every frame, move player towards
			actorWielder.Move(dashTarget * dashSpeed * Time.deltaTime);
			SoundDefs.createSound(actorWielder.transform.position, soundScriptable);
		}

		if (buffTimer != 0)
		{
			buffTimer -= Time.deltaTime;
			if (buffTimer <= 0)
			{
				buffTimer = 0;
			}
		}
	}

	private void abilityInputPressed()
	{
		float currMutEnergy = actorWielder.gameManager.playerStats.getMutationBar();
		if (buffTimer <= 0 && currMutEnergy >= mutationScriptable.mutCost)
		{
			actorWielder.gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
			anim.SetTrigger(MutationDefs.TRIGGER_BLADE_WING);
			buffTimer = MutationDefs.ABILITY_BUFF_TIMER;
		}
	}

	public void bladeWingDash()
	{
		actorWielder.gameManager.signalMovementLocked();
		actorWielder.gameManager.signalRotationLocked();

		dashActive = true;

		PlayerInputs playerIn = actorWielder.GetComponentInChildren<PlayerInputs>();
		if (playerIn != null)
		{
			dashStart = actorWielder.transform.position;
			dashTarget = Vector2.ClampMagnitude(playerIn.pointerPos() - (Vector2)actorWielder.transform.position, 1);
		}

		actorWielder.setActorCollision(false, new string[] { GameManager.OBJECT_MID_LAYER, GameManager.ACTOR_LAYER });
	}

	public string getDisplayName()
	{
		return MutationDefs.NAME_WINGS;
	}

	public Sprite getIcon()
	{
		return icon;
	}
	public string getId()
	{
		return "MBladeWing";
	}

	private void init(Actor wielder)
	{
		gameManager = GameManager.Instance;
		this.actorWielder = wielder;
		gameManager.playerAbilityEvent += abilityInputPressed;
		buffTimer = 0;
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.ACTIVE_SLOT;
	}

	/* for an active slot mutation, this does nothing */
	public void trigger(Actor actorTarget)
	{
		return;
	}

	public MutationInterface mEquip(Actor actor)
	{
		actorWielder = actor;

		init(actor);
		
		return this;
	}
	public void setStartingPosition()
	{
		this.transform.SetLocalPositionAndRotation(mutationScriptable.startingPosition, Quaternion.Euler(0, 0, mutationScriptable.startingRotation));
	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
	}

	public void stopDash()
	{
		dashActive = false;
		actorWielder.setActorCollision(true, null);

		//now, draw a cast for other collisions and do damage and such
		Vector3 startPosition = dashStart + (Vector3.ClampMagnitude(actorWielder.transform.position - dashStart, 1) * (dashWidth / 2));
		Vector3 endPosition = actorWielder.transform.position + (Vector3.ClampMagnitude(dashStart - actorWielder.transform.position, 1) * (dashWidth / 2));
		RaycastHit2D[] hits = Physics2D.CircleCastAll(startPosition,
			dashWidth,
			endPosition - startPosition,

			(endPosition - startPosition).magnitude, 
			LayerMask.GetMask(new string[] { GameManager.OBJECT_MID_LAYER, GameManager.ACTOR_LAYER }));

		foreach (RaycastHit2D collision in hits)
		{
			Actor actorHit = collision.transform.GetComponentInChildren<Actor>();
			if (actorHit != null && actorWielder.isTargetHostile(actorHit))
			{
				actorHit.takeDamage(mutationScriptable.damage, actorWielder);
				continue;
			}

			Obstacle obstacleHit = collision.transform.GetComponentInChildren<Obstacle>();
			if (obstacleHit != null)
			{
				obstacleHit.obstacleBody.AddForce(Vector3.ClampMagnitude(startPosition -  endPosition, 1) * obstacleHit._obstacleScriptable.maxObstacleForce);
				continue;
			}
		}

		dashStart = Vector3.zero;
		dashTarget = Vector3.zero;

		actorWielder.gameManager.signalMovementUnlocked();
		actorWielder.gameManager.signalRotationUnlocked();
	}

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return mutationScriptable.tutorialSprites;
	}

	public void updateCells(int amount)
	{

	}
}