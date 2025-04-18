using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class WeaponPhysics : MonoBehaviour
{
	private BasicWeapon _weapon;
	private WeaponScriptable _weaponScriptable;

	private float currentSpeed;

	private Vector3 _throwMove;
	private Vector3 _lastThrowMove;

	public Actor throwingActor;

	public Rigidbody2D weaponBody;
	public Collider2D throwCollider;
	public Collider2D pickupCollider;

	public BasicWeapon attachedWeapon;
	public GameObject topObject;

	private GameManager gameManager;

	void Start()
	{
		currentSpeed = 0;
		gameManager = GameManager.Instance;
	}

	void FixedUpdate()
	{
		calculateThrow();
	}

	public void calculateThrow()
	{
		/* first frame of throw */
		if (_throwMove.magnitude > 0)
		{
			if (_lastThrowMove.magnitude <= 0)
			{
				_lastThrowMove = new Vector3(_throwMove.x, _throwMove.y, 0);
				_throwMove = new Vector3(0, 0, 0);
				currentSpeed = _weaponScriptable.throwSpeed;

				/* check for if throw is starting in a collision */
				ContactFilter2D filter = new ContactFilter2D();
				List<RaycastHit2D> wallList = new List<RaycastHit2D>();
				filter.SetLayerMask(gameManager.unwalkableLayers);
				throwCollider.Cast(throwCollider.transform.position, throwCollider.gameObject.transform.rotation.eulerAngles.z, Vector2.left, filter, wallList, 0, true);
				/* move object forward  */
				if (wallList != null && wallList.Count > 0)
				{
					Vector3 shift = (_lastThrowMove * currentSpeed * Time.deltaTime);
					Vector2 perpShift = Vector2.Perpendicular(shift);
					wallList = new List<RaycastHit2D>();
					/* now, check for lateral movement */
					throwCollider.Cast((Vector2)throwCollider.transform.position + perpShift, throwCollider.gameObject.transform.rotation.eulerAngles.z, Vector2.left, filter, wallList, 0, true);
					if (wallList != null && wallList.Count > 0)
					{
						wallList = new List<RaycastHit2D>();
						throwCollider.Cast((Vector2)throwCollider.transform.position - perpShift, throwCollider.gameObject.transform.rotation.eulerAngles.z, Vector2.left, filter, wallList, 0, true);
						if (wallList == null || wallList.Count <= 0)
						{
							shift = -perpShift;
						}
						else
						{
							shift = shift * 0.5F;
						}
					}
					else
					{
						shift = perpShift;
					}

					topObject.transform.SetPositionAndRotation(topObject.transform.position + shift, Quaternion.Euler(0, 0, topObject.transform.rotation.eulerAngles.z));
					return;
				}
			}
			/* bouncing  off of an object*/
			else
			{
				_lastThrowMove = new Vector3(_throwMove.x, _throwMove.y, 0);
				_throwMove = new Vector3(0, 0, 0);
			}
		}

		if (_lastThrowMove.magnitude > 0)
		{
			currentSpeed -= _weaponScriptable.throwWeight;

			Vector3 translation = _lastThrowMove * currentSpeed * Time.deltaTime;
			float rotationAdd = WeaponDefs.THROW_ROTATE_MID * (currentSpeed / _weaponScriptable.throwSpeed) * _weaponScriptable.throwWeight * Time.deltaTime;

			if (attachedWeapon != null && attachedWeapon.currentSide == false)
			{
				rotationAdd = -rotationAdd;
			}

			if (currentSpeed > 45)
			{
				ContactFilter2D filter = new ContactFilter2D();
				List<RaycastHit2D> wallList = new List<RaycastHit2D>();
				filter.SetLayerMask(gameManager.unwalkableLayers);
				throwCollider.Cast(throwCollider.transform.position, throwCollider.gameObject.transform.rotation.eulerAngles.z + rotationAdd, translation, filter, wallList, translation.magnitude, true);
				/* move object back */
				if (wallList != null && wallList.Count > 0)
				{
					topObject.transform.SetPositionAndRotation(topObject.transform.position - (0.5F * translation), Quaternion.Euler(0, 0, topObject.transform.rotation.eulerAngles.z + rotationAdd));
				}
				else
				{
					topObject.transform.SetPositionAndRotation(topObject.transform.position + translation, Quaternion.Euler(0, 0, topObject.transform.rotation.eulerAngles.z + rotationAdd));
				}
			}
			else
			{
				topObject.transform.SetPositionAndRotation(topObject.transform.position + translation, Quaternion.Euler(0, 0, topObject.transform.rotation.eulerAngles.z + rotationAdd));
			}

			//float throwForce = 500;
			//weaponBody.AddForce(new Vector2(throwForce * _lastThrowMove.x * currentSpeed * Time.deltaTime, throwForce * _lastThrowMove.y * currentSpeed * Time.deltaTime));
			//weaponBody.rotation += WeaponDefs.THROW_ROTATE_MID * (currentSpeed / _weaponScriptable.throwSpeed) * _weaponScriptable.throwWeight * Time.deltaTime; 

			if (currentSpeed <= 0)
			{
				weaponBody.bodyType = RigidbodyType2D.Kinematic;
				gameObject.layer = LayerMask.NameToLayer(GameManager.OBJECT_LAYER);
				throwCollider.enabled = false;
				pickupCollider.enabled = true;
				WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_GROUND, topObject);

				_lastThrowMove = new Vector3(0, 0, 0);
				currentSpeed = 0;
			}
		}
		else
		{
			throwingActor = null;
		}
	}

	public bool isBeingThrown()
	{
		return _lastThrowMove.magnitude > 0;
	}

	/* THIS MUST BE CALLED IN EACH WEAPON'S START FUNCTION */
	public void linkInterface(BasicWeapon toLink)
	{
		_weapon = toLink;
		_weaponScriptable = _weapon.getScriptable();
		attachedWeapon = toLink;
	}

	public void startThrow(Vector3 target, Actor throwingActor)
	{
		_throwMove = target;
		weaponBody.bodyType = RigidbodyType2D.Dynamic;
		gameObject.layer = LayerMask.NameToLayer(GameManager.THROWN_WEAPON_LAYER);
		this.throwingActor = throwingActor;
		throwCollider.enabled = true;
		pickupCollider.enabled = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		bool bouncedOffActor = false;
		if (_lastThrowMove.magnitude > 0)
		{
			Actor actorHit = collision.gameObject.GetComponent<Actor>();
			if (actorHit != null && currentSpeed > _weaponScriptable.throwHurtSpeed)
			{
				if (actorHit.name == throwingActor.name)
				{
					Debug.Log("Stop hitting yourself (throw)");
					return;
				}
				Debug.Log("Throwing " + this.gameObject.name + " hit " + actorHit.name + " for " + _weaponScriptable.throwDamage + " damage");
				if (actorHit.takeDamage(_weaponScriptable.throwDamage, _weapon.getActorWielder()) > 0)
				{	
					actorHit.drop();
					_weapon.reduceDurability(_weaponScriptable.throwDurabilityDamage);

					if (_weaponScriptable.soundActorHit != null)
					{
						AudioClip toPlay;
						bouncedOffActor = true;
						gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundActorHit.name, out toPlay);
						if (toPlay != null)
						{
							_weapon.weaponAudioPlayer.PlayOneShot(toPlay);
						}
					}
				}

				EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stunThrow);
			}

			if (!bouncedOffActor)
			{
				if (_weaponScriptable.soundObstacleHit != null)
				{
					AudioClip toPlay;
					gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundObstacleHit.name, out toPlay);
					if (toPlay != null)
					{
						_weapon.weaponAudioPlayer.PlayOneShot(toPlay);
					}
				}
			}
			currentSpeed /= 2;
			_throwMove = _lastThrowMove * (1 + (1/_weaponScriptable.throwWeight)) * -1;
		}
	}
}