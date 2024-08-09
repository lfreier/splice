using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class WeaponPhysics : MonoBehaviour
{
	private WeaponInterface _weapon;
	private WeaponScriptable _weaponScriptable;

	private float currentSpeed;

	private Vector3 _throwMove;
	private Vector3 _lastThrowMove;

	public Actor throwingActor;

	public Rigidbody2D weaponBody;
	public Collider2D throwCollider;
	public Collider2D pickupCollider;

	private GameManager gameManager;

	void Start()
	{
		currentSpeed = 0;
		gameManager = GameManager.Instance;
	}

	void Update()
	{
		/* if currently being thrown... */
		if (_lastThrowMove.magnitude > 0)
		{
			/* if the throw should damage */
			if (currentSpeed <= 0)
			{
				weaponBody.bodyType = RigidbodyType2D.Kinematic;
				gameObject.layer = LayerMask.NameToLayer(GameManager.OBJECT_LAYER);
				throwCollider.enabled = false;
				pickupCollider.enabled = true;

				_lastThrowMove = new Vector3(0, 0, 0);
				currentSpeed = 0;
			}
		}
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

			weaponBody.MovePosition(weaponBody.transform.position + new Vector3(_lastThrowMove.x * currentSpeed * Time.deltaTime, _lastThrowMove.y * currentSpeed * Time.deltaTime));
			this.transform.Rotate(new Vector3(0, 0, WeaponDefs.THROW_ROTATE_MID * (currentSpeed / _weaponScriptable.throwSpeed) * _weaponScriptable.throwWeight * Time.deltaTime));
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
	public void linkInterface(WeaponInterface toLink)
	{
		_weapon = toLink;
		_weaponScriptable = _weapon.getScriptable();
	}

	public void startThrow(Vector3 target, Actor throwingActor)
	{
		_throwMove = target;
		weaponBody.bodyType = RigidbodyType2D.Dynamic;
		gameObject.layer = LayerMask.NameToLayer(GameManager.COLLISION_ACTOR_LAYER);
		this.throwingActor = throwingActor;
		throwCollider.enabled = true;
		pickupCollider.enabled = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
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
				actorHit.takeDamage(_weaponScriptable.throwDamage);
				_weapon.reduceDurability(_weaponScriptable.throwDurabilityDamage);
				EffectDefs.effectStun(actorHit, GameManager.EFCT_SCRIP_ID_STUN1);
			}

			currentSpeed /= 2;
			_throwMove = _lastThrowMove - (transform.up * (1 + (1/_weaponScriptable.throwWeight)));
		}
	}
}