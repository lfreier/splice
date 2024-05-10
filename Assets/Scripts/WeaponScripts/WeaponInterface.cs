using UnityEditor;
using UnityEngine;

public interface WeaponInterface
{
	public bool attack(LayerMask targetLayer);

	public bool canBeDropped();

	public WeaponScriptable getScriptable();

	public float getSpeed();
	public WeaponType getType();

	public bool inRange(Vector3 target);

	public bool isActive();

	public void physicsMove(Vector3 velocity);

	public void setActorToHold(Actor actor);

	public void setHitbox(bool toggle);

	public void setStartingPosition();

	public void throwWeapon(Vector3 target);

	public bool toggleCollider();
}