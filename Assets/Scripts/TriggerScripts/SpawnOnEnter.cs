using System.Collections;
using UnityEngine;

public class SpawnOnEnter : MonoBehaviour
{
	public GameObject[] toEnable;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actorHit = collision.GetComponentInChildren<Actor>();
		if (actorHit != null && actorHit.tag == ActorDefs.playerTag)
		{
			foreach (GameObject enable in toEnable)
			{
				if (enable == null)
				{
					continue;
				}
				enable.SetActive(true);
			}
			Destroy(this.gameObject);
		}
	}
}