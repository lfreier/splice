using System.Collections;
using UnityEngine;

/* this is dumb hacky i think but w/e */
public class BasicWeaponCollider : MonoBehaviour
{
	public BasicWeapon primaryWeapon;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		primaryWeapon.weaponHit(collision);
	}
}