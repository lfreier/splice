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

	public void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actor = collision.transform.GetComponent<Actor>();
		if (actor != null && actor.tag == ActorDefs.playerTag && affectedEnemies != null)
		{
			for (int i = 0; i < affectedEnemies.Length; i++)
			{
				/*
				if (newMoveTargets != null && i < newMoveTargets.Length)
				{
					affectedEnemies[i].moveTarget = newMoveTargets[i];
				}
				*/
				if (weaponsToEquip != null && i < weaponsToEquip.Length)
				{
					GameObject spawnedWeapon = Instantiate(weaponsToEquip[i]);
					affectedEnemies[i].actor.equip(spawnedWeapon);
				}
				affectedEnemies[i]._detection = ActorDefs.detectMode.forced;
			}
			Destroy(this);
		}
	}
}