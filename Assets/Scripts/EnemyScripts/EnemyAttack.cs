using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
	public Actor actor;

	void Start()
	{

	}

	void Update()
	{
		Actor target = actor.getAttackTarget();
		if (target != null && actor.inWeaponRange(target.transform.position))
		{
			//actor.setMoveSpeed(0);
			actor.attack();
		}
	}
}