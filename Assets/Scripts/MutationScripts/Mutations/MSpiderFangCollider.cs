using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MSpiderFangCollider : MonoBehaviour
{
	public MSpider attachedSpider;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision && attachedSpider != null)
		{
			Actor actorHit = collision.GetComponentInChildren<Actor>();
			if (actorHit != null)
			{
				attachedSpider.triggerFangAttack(actorHit);
			}
		}
	}
}