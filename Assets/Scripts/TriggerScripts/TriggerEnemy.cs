using System.Collections;
using UnityEngine;

public class TriggerEnemy : MonoBehaviour
{
	public EnemyMove[] affectedEnemies;
	//public Vector2[] newMoveTargets;
	public GameObject[] weaponsToEquip;

	private GameManager gm;

	private void Start()
	{
		gm = GameManager.Instance;
	}

	private void FixedUpdate()
	{
		if (affectedEnemies != null)
		{
			for (int i = 0; i < affectedEnemies.Length; i++)
			{
				if (affectedEnemies[i] == null)
				{
					continue;
				}
				if (affectedEnemies[i]._detection != ActorDefs.detectMode.idle && affectedEnemies[i]._detection != ActorDefs.detectMode.forced && affectedEnemies[i]._detection != ActorDefs.detectMode.nul)
				{
					equipNewWeapon(i);
				}
			}
		}
	}

	private void equipNewWeapon(int i)
	{
		if (weaponsToEquip != null && i < weaponsToEquip.Length && weaponsToEquip[i] != null)
		{
			GameObject spawnedWeapon = Instantiate(weaponsToEquip[i]);
			affectedEnemies[i].actor.equip(spawnedWeapon);
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actor = collision.transform.GetComponent<Actor>();
		if (actor != null && actor.tag == ActorDefs.playerTag && affectedEnemies != null)
		{
			for (int i = 0; i < affectedEnemies.Length; i++)
			{
				if (affectedEnemies[i] == null)
				{
					continue;
				}
				/*
				if (newMoveTargets != null && i < newMoveTargets.Length)
				{
					affectedEnemies[i].moveTarget = newMoveTargets[i];
				}
				*/
				affectedEnemies[i]._detection = ActorDefs.detectMode.forced;
				equipNewWeapon(i);
			}
			Destroy(this.gameObject);
		}
	}
}