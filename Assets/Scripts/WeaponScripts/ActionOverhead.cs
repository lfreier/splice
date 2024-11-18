using System.Collections;
using UnityEngine;

public class ActionOverhead : MonoBehaviour, ActionInterface
{
	public Animator anim;
	public Collider2D overheadHitbox;
	public Collider2D sweetSpotHitbox;

	public BasicWeapon weapon;

	public SpriteRenderer spriteRend;

	public Sprite overheadDamagedSprite;

	public float dashSpeed = 900F;
	private Vector3 dashTarget;

	private bool weaponStartSide;

	private bool attackActive;
	private bool animationActive;
	private Actor actorWielder;

	public void FixedUpdate()
	{
		if (attackActive)
		{
			//every frame, move player towards dash target
			actorWielder.Move(dashTarget * dashSpeed * Time.deltaTime);
		}
	}

	public void action()
	{
		if (!animationActive)
		{
			attackActive = false;
			animationActive = true;
			spriteRend.enabled = true;
			weapon.sprite.enabled = false;
			anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK_SEC);
		}
	}

	public void setActorToHold(Actor actor)
	{
		actorWielder = actor;
		animationActive = false;
	}

	public void setDamagedSprite()
	{
		spriteRend.sprite = overheadDamagedSprite;
	}

	public void startOverheadFrames()
	{
		actorWielder.gameManager.signalMovementLocked();
		actorWielder.gameManager.signalRotationLocked();

		sweetSpotHitbox.enabled = true;
		overheadHitbox.enabled = true;

		weaponStartSide = weapon.currentSide;

		PlayerInputs playerIn = actorWielder.GetComponentInChildren<PlayerInputs>();
		if (playerIn != null)
		{
			dashTarget = Vector2.ClampMagnitude(playerIn.pointerPos() - (Vector2)actorWielder.transform.position, 1);
			attackActive = true;
		}
	}

	public void stopOverhead()
	{
		//need to set startingPosition
		weapon.setStartingPosition(weaponStartSide);

		spriteRend.enabled = false;
		weapon.sprite.enabled = true;

		animationActive = false;

		dashTarget = Vector2.zero;
	}

	public bool toggleHitbox()
	{
		if (overheadHitbox.enabled)
		{
			//disable the dash
			actorWielder.gameManager.signalMovementUnlocked();
			actorWielder.gameManager.signalRotationUnlocked();

			sweetSpotHitbox.enabled = false;
			overheadHitbox.enabled = false;

			attackActive = false;
		}
		else
		{
			//Set starting position of the overhead
			weapon._weaponPhysics.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		}

		return true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//if sweet spot, do more damage and stun

		//otherwise do normal damage and stun

		//then decrease durability
	}
}