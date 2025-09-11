using System.Collections;
using UnityEngine;

public class SecondWeapon : MonoBehaviour
{
	public BasicWeapon primaryWeapon;

	public Collider2D hitbox;
	public SpriteRenderer trailSprite;

	public bool toggleCollider(int enable)
	{
		if (hitbox == null)
		{
			return false;
		}
		if (trailSprite != null)
		{
			trailSprite.enabled = enable > 0;
		}
		return hitbox.enabled = enable > 0;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (primaryWeapon != null)
		{
			primaryWeapon.weaponHit(collision);
		}
	}
}