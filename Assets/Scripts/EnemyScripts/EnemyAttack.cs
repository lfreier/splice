using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
	public Actor actor;
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
			if (attackTimer <= 0)
			{
				actor.attack();
			}
			attackTimer -= Time.deltaTime;
		}
		else
		{
			attackTimer = attackTimerLength;
		}
	}
}