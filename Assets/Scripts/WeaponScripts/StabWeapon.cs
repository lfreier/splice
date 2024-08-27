using UnityEditor;
using UnityEngine;

public class StabWeapon : MonoBehaviour, WeaponInterface
{
	[SerializeField] public Animator anim;
	
	public ActionInterface secondaryAction;

	string id;

	public Controller2D controller;
	public CircleCollider2D arc;
	public BoxCollider2D hitbox;
	public BoxCollider2D throwBox;

	public SpriteRenderer sprite;
	public Sprite damagedSprite;

	public Actor actorWielder;

	public WeaponScriptable _weaponScriptable;
	public WeaponPhysics _weaponPhysics;

	private float durability;

	void Start()
	{
		durability = _weaponScriptable.durability;
		id = this.gameObject.name;
		secondaryAction = GetComponent<ActionInterface>();
		_weaponPhysics.linkInterface(this);
	}

	void FixedUpdate()
	{
		// was just thrown, so give it initial speed
		_weaponPhysics.calculateThrow();
	}

	public bool attack(LayerMask targetLayer)
	{
		throw new System.NotImplementedException();
	}

	public void attackSecondary()
	{
		if (secondaryAction != null)
		{
			secondaryAction.action();
		}
	}

	public bool canBeDropped()
	{
		if (_weaponScriptable.weaponType == WeaponType.UNARMED || !_weaponScriptable.canBeDropped)
		{
			return false;
		}

		return true;
	}

	public void cancelAttack()
	{
		anim.StopPlayback();
		hitbox.enabled = false;
	}

	public WeaponScriptable getScriptable()
	{
		return _weaponScriptable;
	}
	public Actor getActorWielder()
	{
		return actorWielder;
	}

	public float getSpeed()
	{
		return _weaponScriptable.atkSpeed;
	}

	public WeaponType getType()
	{
		return _weaponScriptable.weaponType;
	}

	public bool inRange(Vector3 target)
	{
		return Vector3.Distance(transform.position, target) <= _weaponScriptable.npcAttackRange;
	}

	public bool isActive()
	{
		return (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Idle") || _weaponPhysics.isBeingThrown());
	}

	public void reduceDurability(float reduction)
	{

	}

	public void setActorToHold(Actor actor)
	{
		actorWielder = actor;
	}

	public void setHitbox(bool toggle)
	{
		hitbox.enabled = toggle;
	}

	public void setStartingPosition()
	{
		transform.parent.SetLocalPositionAndRotation(new Vector3(_weaponScriptable.equipPosX, _weaponScriptable.equipPosY, 0), Quaternion.Euler(0, 0, _weaponScriptable.equipRotZ));
	}
	public void slowWielder(float percentage)
	{
		if (actorWielder != null)
		{
			float slowedSpeed = actorWielder.actorData.maxSpeed * percentage;
			actorWielder.setSpeed(slowedSpeed);
		}
	}
	
	public void throwWeapon(Vector3 target)
	{
		_weaponPhysics.startThrow(target, actorWielder);
	}

	public bool toggleCollider()
	{
		return hitbox.enabled = !hitbox.enabled;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//ho boy here we go

	}
}