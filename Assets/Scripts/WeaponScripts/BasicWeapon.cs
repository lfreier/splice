using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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
	public AudioSource weaponSwingAudioPlayer;

	public SoundScriptable actorHitSound;
	public SoundScriptable wallHitSound;

	public bool currentSide;

	public bool attackOnlyRight;

	protected GameManager gameManager;

	public float durability;

	public bool isInit = false;
	public bool swingActive = false;

	protected bool soundMade = false;

	void Awake()
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
		swingActive = true;
		soundMade = false;
		anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
		//lastTargetLayer = targetLayer;
		actorWielder.invincible = false;
		if (_weaponScriptable.soundSwing != null)
		{
			playSound(weaponSwingAudioPlayer, _weaponScriptable.soundSwing.name, _weaponScriptable.soundSwingVolume);
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

		return !anim.GetCurrentAnimatorStateInfo(0).IsTag("Idle") || throwActive || swingActive;
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
			if (secondaryAction != null)
			{
				secondaryAction.setDamagedSprite();
			}
		}

		if (durability <= 0)
		{
			if (_weaponScriptable.soundBreak != null)
			{
				AudioClip toPlay;
				gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundBreak.name, out toPlay);
				if (toPlay != null)
				{
					if (actorWielder == null)
					{
						weaponAudioPlayer.PlayOneShot(toPlay, gameManager.effectsVolume);
					}
					else
					{
						actorWielder.actorAudioSource.PlayOneShot(toPlay, gameManager.effectsVolume);
					}
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

	virtual public void setStartingPosition(bool side)
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

	virtual public bool toggleCollider(int enable)
	{
		swingActive = enable > 0;
		if (hitbox == null || (anim.GetBool(WeaponDefs.ANIM_BOOL_ONLY_RIGHT) && !currentSide))
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

	public virtual void playSound(AudioSource player, string soundName, float volume)
	{
		AudioClip toPlay;
		if (gameManager.audioManager.soundHash.TryGetValue(soundName, out toPlay) && toPlay != null)
		{
			player.PlayOneShot(toPlay, volume * gameManager.effectsVolume);
		}
	}

	public Actor weaponHit(Collider2D collision, float damage, float durabilityDamage)
	{
		float knockbackMult = 1;
		float maxForce = 10000;
		Actor actorRef = actorWielder;

		if (actorRef != null && collision.name == actorRef.name)
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
		if (actorRef == null)
		{
			return null;
		}

		if (actorHit != null && actorRef.isTargetHostile(actorHit) && !actorHit.invincible)
		{
			actorRef.triggerDamageEffects(actorHit);
			knockbackMult = (1 - actorHit._actorScriptable.knockbackResist) * WeaponDefs.KNOCKBACK_MULT_ACTOR;
			SoundDefs.createSound(actorHit.transform.position, actorHitSound);
			if (_weaponScriptable.soundActorHit != null)
			{
				playSound(weaponAudioPlayer, _weaponScriptable.soundActorHit.name, _weaponScriptable.soundActorHitVolume);
			}

			if (actorHit.takeDamage(damage, actorRef) > 0)
			{
				reduceDurability(durabilityDamage);
			}

			maxForce = ActorDefs.MAX_HIT_FORCE;
			if (actorRef.isStunned())
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
				SoundDefs.createSound(actorRef.transform.position, wallHitSound);
				if (_weaponScriptable.soundWallHit != null)
				{
					playSound(weaponAudioPlayer, _weaponScriptable.soundWallHit.name, _weaponScriptable.soundWallHitVolume);
				}
			}
			Debug.Log("Hit: " + collision.name + " for no damage");

			Obstacle obstacle = collision.GetComponent<Obstacle>();
			if (obstacle != null)
			{
				knockbackMult = obstacle._obstacleScriptable.weaponHitMult * WeaponDefs.KNOCKBACK_MULT_OBSTACLE;
				maxForce = obstacle._obstacleScriptable.maxObstacleForce;
				obstacle.knockOver();

				if (_weaponScriptable.soundObstacleHit != null && !soundMade && knockbackMult != 0)
				{
					soundMade = true;
					playSound(weaponAudioPlayer, _weaponScriptable.soundObstacleHit.name, _weaponScriptable.soundObstacleHitVolume);
				}

				reduceDurability(obstacle._obstacleScriptable.weaponDurabilityDamage);
			}
		}

		/* knockback */
		Rigidbody2D hitBody = collision.attachedRigidbody;
		if (hitBody != null)
		{
			Vector3 force = Vector3.ClampMagnitude(hitBody.transform.position - actorRef.transform.position, 1);
			float forceMult = Mathf.Min(_weaponScriptable.knockbackDamage * knockbackMult, maxForce);
			Debug.Log("Hit force on " + hitBody.name + ": " + forceMult);
			hitBody.AddForce(force * forceMult);
		}

		if (durability <= 0)
		{
			actorRef.drop();
		}

		return actorHit;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		weaponHit(collision);
	}
}