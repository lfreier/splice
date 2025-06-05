using System.Collections;
using UnityEngine;

public class MSporeMineExplosion : MonoBehaviour
{
	public MSporeMine mine;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actorHit = collision.GetComponentInChildren<Actor>();
		if (actorHit != null)
		{
			EnemyMove enemyMove = actorHit.GetComponentInChildren<EnemyMove>();
			if (enemyMove != null)
			{
				enemyMove.setStunResponse(actorHit.transform.position);
				EffectDefs.effectApply(actorHit, GameManager.Instance.effectManager.stun1);
				EffectDefs.effectApply(actorHit, GameManager.Instance.effectManager.bleed2Half);
			}
			else if (actorHit.tag == ActorDefs.playerTag)
			{
				EffectDefs.effectApply(actorHit, GameManager.Instance.effectManager.bleed1Half);
			}
		}

		//TODO: maybe if something other than actor is hit?
	}
}