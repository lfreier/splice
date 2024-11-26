using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ActionOverhead : MonoBehaviour, ActionInterface
{
	public Animator anim;
	public Collider2D overheadHitbox;
	public Collider2D sweetSpotHitbox;

	public BasicWeapon weapon;

	public SpriteRenderer spriteRend;

	public Sprite overheadDamagedSprite;

	public bool hitSourspot = false;

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
		/* set animation trigger, switch sprites, and make sure attackActive is false (used for active hitbox frames) */
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

	/* set by BasicWeapon when normal weapon sprite is damaged */
	public void setDamagedSprite()
	{
		spriteRend.sprite = overheadDamagedSprite;
	}

	/* called by the overhead animation */
	public void startOverheadFrames()
	{
		actorWielder.gameManager.signalMovementLocked();
		actorWielder.gameManager.signalRotationLocked();

		hitSourspot = false;

		overheadHitbox.enabled = true;

		weaponStartSide = weapon.currentSide;

		PlayerInputs playerIn = actorWielder.GetComponentInChildren<PlayerInputs>();
		if (playerIn != null)
		{
			dashTarget = Vector2.ClampMagnitude(playerIn.pointerPos() - (Vector2)actorWielder.transform.position, 1);
			attackActive = true;
		}
	}

	/* called by the overhead animation */
	public void startSweetspotFrames()
	{
		sweetSpotHitbox.enabled = true;
	}

	/* called by the overhead animation - resets sprites and data, but NOT hitbox */
	public void stopOverhead()
	{
		//need to set startingPosition
		weapon.setStartingPosition(weaponStartSide);

		spriteRend.enabled = false;
		weapon.sprite.enabled = true;

		animationActive = false;

		dashTarget = Vector2.zero;
	}

	/* Called by the overhead animation - has some attached logic
	 * Not used to enable the hitbox - only disable (sorry)
	 */
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
}