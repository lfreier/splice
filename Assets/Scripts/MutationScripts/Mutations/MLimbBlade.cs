using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MLimb;

public class MLimbBlade : MonoBehaviour
{
	public MLimb attachedLimb;

	public AudioSource bladeAudioPlayer;
	public AudioClip bladeActorHitSound;

	public List<Collider2D> collisions = new List<Collider2D>();

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision != null)
		{
			Actor actorHit = collision.GetComponentInChildren<Actor>();
			if (actorHit != null && attachedLimb != null)
			{
				if (collisions.Contains(collision))
				{
					return;
				}
				GameManager.Instance.playSound(bladeAudioPlayer, bladeActorHitSound.name, 1F);

				EffectDefs.effectApply(actorHit, attachedLimb.actorWielder.gameManager.effectManager.stunHalf);
				actorHit.takeDamage(attachedLimb.mutationScriptable.damage);
				
				EnemyMove enemyMove = actorHit.GetComponentInChildren<EnemyMove>();
				if (enemyMove != null)
				{
					enemyMove.setStunResponse(attachedLimb.actorWielder);
				}
				collisions.Add(collision);
				return;
			}

			Obstacle obstacle = collision.GetComponentInChildren<Obstacle>();
			if (obstacle != null)
			{
				return;
			}

			attachedLimb.anim.SetTrigger(MutationDefs.TRIGGER_LIMB_BLADE_RETRACT);
		}
	}
}