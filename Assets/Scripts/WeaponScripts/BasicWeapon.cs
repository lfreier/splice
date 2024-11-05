using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class BasicWeapon : MonoBehaviour, WeaponInterface
{
	[SerializeField] public Animator anim;

	public ActionInterface secondaryAction;

	LayerMask lastTargetLayer;
	string id;

	public Collider2D hitbox;
	public BoxCollider2D throwBox;

	public SpriteRenderer sprite;
	public Sprite damagedSprite;
	public SpriteRenderer trailSprite;

	public Actor actorWielder;

	public WeaponScriptable _weaponScriptable;
	public WeaponPhysics _weaponPhysics;

	public SoundScriptable actorHitSound;
	public SoundScriptable wallHitSound;

	public AudioClip weaponBreakSound;
	public AudioClip weaponSwingSound;

	private GameManager gameManager;

	protected float durability;

	void Start()
	{
		gameManager = GameManager.Instance;
		durability = _weaponScriptable.durability;
		id = this.gameObject.name;
		secondaryAction = GetComponent<ActionInterface>();
		if (secondaryAction == null)
		{
			secondaryAction = GetComponentInChildren<ActionInterface>();
		}
		_weaponPhysics.linkInterface(this);

		init();
	}

	void FixedUpdate()
	{
		// was just thrown, so give it initial speed
		_weaponPhysics.calculateThrow();
		if (!isActive())
		{
			hitbox.enabled = false;
		}
	}

	public bool attack(LayerMask targetLayer)
	{
		anim.SetTrigger("Attack");
		lastTargetLayer = targetLayer;
		actorWielder.invincible = false;
		//actorWielder.actorAudioSource.PlayOneShot(weaponSwingSound);

		return true;
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

	public virtual float getWeaponDamage()
	{
		return _weaponScriptable.damage;
	}

	public virtual void init()
	{

	}

	public bool inRange(Vector3 target)
	{
		return Vector3.Distance(transform.position, target) <= _weaponScriptable.npcAttackRange;
	}

	public bool isActive()
	{
		return (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Idle") || _weaponPhysics.isBeingThrown());
	}

	virtual public void reduceDurability(float reduction)
	{
		if (durability < 0)
		{
			return;
		}

		durability -= reduction;

		if (durability <= Mathf.Round(_weaponScriptable.durability / WeaponDefs.DURABILITY_DAMAGED_DIVIDER) && damagedSprite != null)
		{
			//set to damaged sprite
			sprite.sprite = damagedSprite;
		}

		if (durability <= 0)
		{
			this.actorWielder.drop();
			Debug.Log("Weapon broke: " + this.name);
			//actorWielder.actorAudioSource.PlayOneShot(weaponBreakSound);
			//might need to wait for sound to play out
			Destroy(this.transform.parent.gameObject);
		}
	}
	public void setActorToHold(Actor actor)
	{
		actorWielder = actor;
		if (secondaryAction != null || null != (secondaryAction = GetComponentInChildren<ActionInterface>()))
		{
			secondaryAction.setActorToHold(actor);
		}
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

	/* Only deal with the movement of the throw */
	public void throwWeapon(Vector3 target)
	{
		anim.Rebind();
		anim.Update(0f);
		_weaponPhysics.startThrow(target, actorWielder);
	}

	public bool toggleCollider()
	{
		if (trailSprite != null)
		{
			trailSprite.enabled = !trailSprite.enabled;
		}
		return hitbox.enabled = !hitbox.enabled;
	}
	public void toggleIFrames()
	{
		if (actorWielder != null)
		{
			actorWielder.invincible = !actorWielder.invincible;
		}
	}

	public bool toggleSecondaryCollider()
	{
		return secondaryAction.toggleHitbox();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		float knockbackMult = 1;
		float maxForce = 10000;
		//TODO: put this somewhere else that's not specific for the weapon?
		if (this.actorWielder != null && collision.name == actorWielder.name)
		{
			Debug.Log("Stop hitting yourself");
			return;
		}

		if (_weaponPhysics.throwingActor != null && collision.name == _weaponPhysics.throwingActor.name)
		{
			Debug.Log("Stop hitting yourself");
			return;
		}

		Actor actorHit = collision.GetComponent<Actor>();
		if (actorWielder == null)
		{
			return;
		}

		if (actorHit != null && actorWielder.isTargetHostile(actorHit) && !actorHit.invincible)
		{
			actorWielder.triggerDamageEffects(actorHit);
			if (actorHit.takeDamage(getWeaponDamage()) > 0)
			{
				reduceDurability(1);
			}
			knockbackMult = 1 - actorHit._actorScriptable.knockbackResist;
			SoundDefs.createSound(actorHit.transform.position, actorHitSound);

			maxForce = ActorDefs.MAX_HIT_FORCE;
			if (actorWielder.isStunned())
			{
				maxForce = ActorDefs.MAX_PARRY_FORCE;
			}

			Debug.Log("Hit: " + collision.name + " for " + _weaponScriptable.damage + " damage");
		}
		else
		{
			if (collision.tag == SoundDefs.TAG_WALL_METAL)
			{
				SoundDefs.createSound(_weaponPhysics.transform.position, wallHitSound);
			}
			else
			{
				SoundDefs.createSound(_weaponPhysics.transform.position, actorHitSound);
			}
			Debug.Log("Hit: " + collision.name + " for no damage");

			Obstacle obstacle = collision.GetComponent<Obstacle>();
			if (obstacle != null)
			{
				reduceDurability(obstacle._obstacleScriptable.weaponDurabilityDamage);
				knockbackMult = obstacle._obstacleScriptable.weaponHitMult;
				maxForce = obstacle._obstacleScriptable.maxObstacleForce;
			}
		}

		/* knockback */
		Rigidbody2D hitBody = collision.attachedRigidbody;
		if (hitBody != null)
		{
			Vector3 force = Vector3.ClampMagnitude(hitBody.transform.position - actorWielder.transform.position, 1);
			float forceMult = Mathf.Min(WeaponDefs.KNOCKBACK_MULT_SWING * _weaponScriptable.knockbackDamage * knockbackMult, maxForce);
			Debug.Log("Hit force on " + hitBody.name + ": " + forceMult);
			hitBody.AddForce(force * forceMult);
		}
	}
}