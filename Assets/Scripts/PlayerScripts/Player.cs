using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Player : MonoBehaviour
{
	public const string OBJECT_WEAPON_TAG = "ObjectWeapon";
	public const string EQUIPPED_WEAPON_TAG = "EquippedWeapon";

	private float moveCamInput;
	private Vector2 moveInput;
	private Vector2 oldMoveInput;
	private float currentSpeed;

	private GameObject equippedWeapon;
	private WeaponInterface equippedWeaponInt;

	private float attackInput;
	private float speedCheck;

	private ActorScriptable playerData;
	private Vector2 pointerPos;

	private float interactInput;
	private float lastInteractInput;

	private float throwInput;
	private float lastThrowInput;

	public CameraHandler camHandler;
	public Actor actor;

	public LayerMask enemyLayer;
	public LayerMask weaponLayer;

	[SerializeField]
	private InputActionReference move, moveCam, pointer, attack, interact, throwAction;

	private void Start()
	{
		playerData = actor.actorData;
		currentSpeed = 0;
		speedCheck = 0;
		lastInteractInput = 0;
	}

	private void Update()
	{
		attackInputs();
		moveInputs();
		cameraInputs();
	}

	private void FixedUpdate()
	{
		// move
		if (moveInput.magnitude > 0)
		{
			oldMoveInput = moveInput;
			currentSpeed += playerData.acceleration * playerData.moveSpeed;
		}
		else
		{
			currentSpeed -= playerData.deceleration * playerData.moveSpeed;
		}

		currentSpeed = Mathf.Clamp(currentSpeed, 0, playerData.maxSpeed);
		actor.Move(new Vector3(oldMoveInput.x * currentSpeed * Time.deltaTime, oldMoveInput.y * currentSpeed * Time.deltaTime));
	}

	public void moveInputs()
	{
		Vector2 mousePos = pointer.action.ReadValue<Vector2>();
		pointerPos = Camera.main.ScreenToWorldPoint(mousePos);
		moveInput = move.action.ReadValue<Vector2>();
		moveCamInput = moveCam.action.ReadValue<float>();
	}

	public void cameraInputs()
	{
		// camera logic for 'look' input
		if (moveCamInput > 0)
		{
			camHandler.setCamFollowPlayer(false);
		}
		else
		{
			camHandler.setCamFollowPlayer(true);
		}

		// aim at pointer
		Vector2 aimDir = pointerPos - actor.actorBody.position;
		float aimAngle = (Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg) - 90F;
		actor.actorBody.rotation = aimAngle;
	}

	private void attackInputs()
	{
		lastInteractInput = interactInput;
		attackInput = attack.action.ReadValue<float>();
		interactInput = interact.action.ReadValue<float>();
		throwInput = throwAction.action.ReadValue<float>();

		if (speedCheck > 0)
		{
			speedCheck -= Time.deltaTime;
		}

		/* Don't give an input while an animation is playing */
		if (attackInput > 0 && speedCheck <= 0 && (equippedWeaponInt == null || !equippedWeaponInt.isActive()))
		{
			playerAttack();
		}

		if (interactInput > 0 && lastInteractInput == 0)
		{
			Collider2D[] hitTargets = Physics2D.OverlapCircleAll(this.transform.position, 1F, weaponLayer);

			foreach (Collider2D target in hitTargets)
			{
				if (target.tag.StartsWith("Object"))
				{
					if (target.tag.Equals(OBJECT_WEAPON_TAG))
					{
						Debug.Log("Picking up: " + target.name);
						if (this.equip(target.gameObject.transform.GetChild(0).gameObject))
						{
							//Make sure to only pick up one weapon
							break;
						}
					}
				}
			}
		}

		if (throwInput > 0 && lastThrowInput == 0)
		{
			throwWeapon();
		}

		lastThrowInput = throwInput;
	}


	/* 
	 * @param weaponToEquip needs to be a GameObject with BoxCollider2d and WeaponInterface script (swing/stab/shoot/etc.)
	 */
	bool equip(GameObject weaponToEquip)
	{
		WeaponInterface tempWeapInt = weaponToEquip.GetComponent<WeaponInterface>();
		if (tempWeapInt != null)
		{
			dropWeapon();
			equippedWeapon = weaponToEquip;
			equippedWeaponInt = tempWeapInt;
			weaponToEquip.transform.parent.SetParent(this.transform, true);
			equippedWeaponInt.setStartingPosition();
			equippedWeapon.tag = EQUIPPED_WEAPON_TAG;

			return true;
		}

		return false;
	}

	void dropWeapon()
	{
		if (equippedWeaponInt == null || equippedWeaponInt.canBeDropped()) { return; }

		/* Literally just getting positive/negatives */
		Vector3 aimDir = new Vector3(pointerPos.x, pointerPos.y, 0) - this.transform.position;
		Vector3 translate = new Vector3(Random.Range(0.2F * Mathf.Sign(aimDir.x), 0.6F * Mathf.Sign(aimDir.x)), Random.Range(0.2F * Mathf.Sign(aimDir.y), 0.6F * Mathf.Sign(aimDir.y)), 0);

		equippedWeapon.transform.parent.Translate(translate, Space.World);
		equippedWeapon.transform.parent.Rotate(new Vector3(0, 0, Random.Range(-45, 45)), Space.Self);
		equippedWeapon.transform.parent.SetParent(null, true);
		equippedWeapon.tag = OBJECT_WEAPON_TAG;


		equippedWeapon = null;
		equippedWeaponInt = null;
	}

	void throwWeapon()
	{
		Vector3 aimDir = new Vector3(pointerPos.x, pointerPos.y, 0) - this.transform.position;

		equippedWeaponInt.throwWeapon(Vector3.ClampMagnitude(aimDir, 1F));

		equippedWeapon.transform.parent.SetParent(null, true);
		equippedWeapon.tag = OBJECT_WEAPON_TAG;

		equippedWeapon = null;
		equippedWeaponInt = null;
	}

	void playerAttack()
	{
		equippedWeaponInt.attack(enemyLayer);
		speedCheck = equippedWeaponInt.getSpeed();
	}
}