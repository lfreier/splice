using UnityEditor;
using UnityEngine;

public interface WeaponInterface
{
	public bool attack(LayerMask targetLayer);

	public float getSpeed();

	public bool isActive();

	public void setStartingPosition();

	public void throwWeapon(Vector3 target);

	public bool canBeDropped();

	public bool toggleCollider();
}