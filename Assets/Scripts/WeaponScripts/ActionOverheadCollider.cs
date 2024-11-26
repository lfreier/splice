using System.Collections;
using UnityEngine;

public class ActionOverheadCollider : MonoBehaviour
{
	public ActionOverhead attachedOverhead;
	public bool isSweetspot;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//if sweet spot is hit, do more damage and stun for less durability
		bool stopDash = false;
		if (attachedOverhead != null)
		{
			BasicWeapon weap = attachedOverhead.weapon;
			if (isSweetspot && !attachedOverhead.hitSourspot)
			{
				stopDash = weap.durability <= 0.5F;
				Actor actorHit = weap.weaponHit(collision, weap._weaponScriptable.secondaryDamage, 0.5F);
				if (actorHit != null)
				{
					EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stun1);
				}
				else
				{
					stopDash = false;
				}
			}
			else if (!isSweetspot)
			{
				//otherwise, do normal damage and then stun
				stopDash = weap.durability <= 1F;
				Actor actorHit = weap.weaponHit(collision);
				if (actorHit != null)
				{
					attachedOverhead.hitSourspot = true;
					EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stunHalf);
				}
				else
				{
					stopDash = false;
				}
			}

			if (stopDash)
			{
				GameManager gameManager = GameManager.Instance;
				gameManager.signalMovementUnlocked();
				gameManager.signalRotationUnlocked();
			}
		}
	}
}