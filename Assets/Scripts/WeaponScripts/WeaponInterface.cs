using UnityEditor;
using UnityEngine;

public interface WeaponInterface
{
	public bool attack(LayerMask targetLayer);

	public void attackSecondary();

	public bool canBeDropped();

	public void cancelAttack();

	public Actor getActorWielder();

	public GameObject getGameObject();

	public WeaponScriptable getScriptable();

	public float getSpeed();

	public WeaponType getType();

	public bool inRange(Vector3 target);

	public bool isActive();

	public void reduceDurability(float reduction);

	public void setActorToHold(Actor actor);

	public void setHitbox(bool toggle);

	public void setStartingPosition(bool side);

	public void slowWielder(float percentage);
	
	public void throwWeapon(Vector3 target);

	public bool toggleCollider(int enable);
}