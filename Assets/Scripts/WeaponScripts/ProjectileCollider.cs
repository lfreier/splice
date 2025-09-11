using System.Collections;
using UnityEngine;

public class ProjectileCollider : MonoBehaviour
{
	public Projectile attachedProjectile;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		attachedProjectile.triggerCollision(collision);
	}
}