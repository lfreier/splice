using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
	public Actor actor;
	public EnemyMove enemyMove;
	public float attackTimer = 0;

	private float attackTimerLength;

	void Start()
	{
		attackTimerLength = actor._actorScriptable.attackTimer;
	}

	void FixedUpdate()
	{
		Actor target = actor.getAttackTarget();
		if (target != null && actor.inWeaponRange(target.transform.position))
		{
			if (attackTimer <= 0 && enemyMove._detection != ActorDefs.detectMode.forced)
			{
				actor.attack();
				attackTimer = attackTimerLength;
			}
			attackTimer -= Time.deltaTime;
		}
		else
		{
			attackTimer = attackTimerLength;
		}
	}
}