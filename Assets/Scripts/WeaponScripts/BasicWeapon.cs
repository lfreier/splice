using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class BasicWeapon : MonoBehaviour, WeaponInterface
{
	[SerializeField] public Animator anim;

	public ActionInterface secondaryAction;

	public Collider2D hitbox;
	public BoxCollider2D throwBox;

	public SpriteRenderer sprite;
	public Sprite damagedSprite;
	public SpriteRenderer trailSprite;

	public Actor actorWielder;

	public WeaponScriptable _weaponScriptable;
	public WeaponPhysics _weaponPhysics;

	public AudioSource weaponAudioPlayer;

	public SoundScriptable actorHitSound;
	public SoundScriptable wallHitSound;

	public bool currentSide;

	public bool attackOnlyRight;

	private GameManager gameManager;

	public float durability;

	public bool isInit = false;

	private bool soundMade = false;

	void Start()
	{
		gameManager = GameManager.Instance;
		if (durability == 0)
		{
			durability = _weaponScriptable.durability;
		}
		secondaryAction = GetComponent<ActionInterface>();
		if (secondaryAction == null)
		{
			secondaryAction = GetComponentInChildren<ActionInterface>();
		}
		if (_weaponPhysics != null)
		{
			_weaponPhysics.linkInterface(this);
		}
		PickupDefs.setLayer(gameObject);
		init();

		isInit = true;
	}

	virtual public bool attack(LayerMask targetLayer)
	{
		soundMade = false;
		anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
		//lastTargetLayer = targetLayer;
		actorWielder.invincible = false;
		if (_weaponScriptable.soundSwing != null)
		{
			AudioClip toPlay;
			gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundSwing.name, out toPlay);
			if (toPlay != null)
			{
				weaponAudioPlayer.PlayOneShot(toPlay);
			}
		}

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
		if (anim != null)
		{
			anim.StopPlayback();
		}
		if (hitbox != null)
		{
			hitbox.enabled = false;
		}
	}

	public WeaponScriptable getScriptable()
	{
		return _weaponScriptable;
	}

	public Actor getActorWielder()
	{
		return actorWielder;
	}

	public GameObject getGameObject()
	{
		return gameObject;
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

	virtual public bool inRange(Vector3 target)
	{
		return Vector3.Distance(transform.position, target) <= _weaponScriptable.npcAttackRange;
	}

	virtual public bool isActive()
	{
		bool throwActive = false;
		if (_weaponPhysics != null)
		{
			throwActive = _weaponPhysics.isBeingThrown();
		}

		return !anim.GetCurrentAnimatorStateInfo(0).IsTag("Idle") || throwActive;
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
			secondaryAction.setDamagedSprite();
		}

		if (durability <= 0)
		{
			if (_weaponScriptable.soundBreak != null)
			{
				AudioClip toPlay;
				gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundBreak.name, out toPlay);
				if (toPlay != null)
				{
					actorWielder.actorAudioSource.PlayOneShot(toPlay);
				}
			}
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
		if (attackOnlyRight)
		{
			actorWielder.setAttackOnly(WeaponDefs.ANIM_BOOL_ONLY_RIGHT, false);
		}
		else
		{
			actorWielder.setAttackOnly("", false);
		}
	}

	public void setHitbox(bool toggle)
	{
		if (hitbox != null && !(attackOnlyRight & !currentSide))
		{
			hitbox.enabled = toggle;
		}
	}

	public void setSide(int side)
	{
		/* 1 == right side, 0 == left side */
		currentSide = (side > 0);
	}

	public void setStartingPosition(bool side)
	{
		if (!side)
		{
			transform.SetLocalPositionAndRotation(new Vector3(_weaponScriptable.equipOtherPosX, _weaponScriptable.equipOtherPosY, 0), Quaternion.Euler(0, 0, _weaponScriptable.equipOtherRotZ));
		}
		else
		{
			transform.SetLocalPositionAndRotation(new Vector3(_weaponScriptable.equipPosX, _weaponScriptable.equipPosY, 0), Quaternion.Euler(0, 0, _weaponScriptable.equipRotZ));
		}
		transform.parent.SetLocalPositionAndRotation(new Vector3(_weaponScriptable.equipPosX, _weaponScriptable.equipPosY, 0), Quaternion.Euler(0, 0, _weaponScriptable.equipRotZ));

		anim.enabled = true;
		currentSide = side;
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
		if (_weaponPhysics == null)
		{
			return;
		}

		anim.enabled = false;
		transform.parent.SetParent(null);

		_weaponPhysics.startThrow(target, actorWielder);
		actorWielder = null;
	}

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

	public Actor weaponHit(Collider2D collision)
	{
		return weaponHit(collision, getWeaponDamage(), 1F);
	}

	public Actor weaponHit(Collider2D collision, float damage, float durabilityDamage)
	{
		float knockbackMult = 1;
		float maxForce = 10000;

		if (this.actorWielder != null && collision.name == actorWielder.name)
		{
			Debug.Log("Stop hitting yourself");
			return null;
		}

		if (_weaponPhysics != null && _weaponPhysics.throwingActor != null && collision.name == _weaponPhysics.throwingActor.name)
		{
			Debug.Log("Stop hitting yourself");
			return null;
		}

		Actor actorHit = collision.GetComponent<Actor>();
		if (actorWielder == null)
		{
			return null;
		}

		if (actorHit != null && actorWielder.isTargetHostile(actorHit) && !actorHit.invincible)
		{
			actorWielder.triggerDamageEffects(actorHit);
			if (actorHit.takeDamage(damage, actorWielder) > 0)
			{
				reduceDurability(durabilityDamage);
			}
			knockbackMult = (1 - actorHit._actorScriptable.knockbackResist) * WeaponDefs.KNOCKBACK_MULT_ACTOR;
			SoundDefs.createSound(actorHit.transform.position, actorHitSound);
			if (_weaponScriptable.soundActorHit != null)
			{
				AudioClip toPlay;
				gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundActorHit.name, out toPlay);
				if (toPlay != null)
				{
					weaponAudioPlayer.PlayOneShot(toPlay);
				}
			}

			maxForce = ActorDefs.MAX_HIT_FORCE;
			if (actorWielder.isStunned())
			{
				maxForce = ActorDefs.MAX_PARRY_FORCE;
			}

			KnockbackTimer knockbackTimer = actorHit.AddComponent<KnockbackTimer>();
			knockbackTimer.init(actorHit._actorScriptable.knockbackResist);

			Debug.Log("Hit: " + collision.name + " for " + damage + " damage");
		}
		else
		{
			if (collision.tag == SoundDefs.TAG_WALL_METAL)
			{
				SoundDefs.createSound(actorWielder.transform.position, wallHitSound);
				if (_weaponScriptable.soundWallHit != null)
				{
					AudioClip toPlay;
					gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundWallHit.name, out toPlay);
					if (toPlay != null)
					{
						weaponAudioPlayer.PlayOneShot(toPlay);
					}
				}
			}
			Debug.Log("Hit: " + collision.name + " for no damage");

			Obstacle obstacle = collision.GetComponent<Obstacle>();
			if (obstacle != null)
			{
				reduceDurability(obstacle._obstacleScriptable.weaponDurabilityDamage);
				knockbackMult = obstacle._obstacleScriptable.weaponHitMult * WeaponDefs.KNOCKBACK_MULT_OBSTACLE;
				maxForce = obstacle._obstacleScriptable.maxObstacleForce;
				obstacle.knockOver();

				if (_weaponScriptable.soundObstacleHit != null && !soundMade && knockbackMult != 0)
				{
					AudioClip toPlay;
					soundMade = true;
					gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundObstacleHit.name, out toPlay);
					if (toPlay != null)
					{
						weaponAudioPlayer.PlayOneShot(toPlay);
					}
				}
			}
		}

		/* knockback */
		Rigidbody2D hitBody = collision.attachedRigidbody;
		if (hitBody != null)
		{
			Vector3 force = Vector3.ClampMagnitude(hitBody.transform.position - actorWielder.transform.position, 1);
			float forceMult = Mathf.Min(_weaponScriptable.knockbackDamage * knockbackMult, maxForce);
			Debug.Log("Hit force on " + hitBody.name + ": " + forceMult);
			hitBody.AddForce(force * forceMult);
		}

		if (durability <= 0)
		{
			actorWielder.drop();
		}

		return actorHit;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		weaponHit(collision);
	}
}