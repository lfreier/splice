using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SwingWeapon : MonoBehaviour, WeaponInterface
{
	[SerializeField] public Animator anim;

	LayerMask lastTargetLayer;
	string id;

	private const float equipPosX = 0.38F;
	private const float equipPosY = 0.15F;
	private const float equipRotZ = -67.5F;

	public Controller2D controller;
	public CircleCollider2D arc;
	public BoxCollider2D hitbox;

	private Vector3 throwMovement;
	private Vector3 lastThrowMovement;

	private float currentSpeed;

	public WeaponScriptable weaponScriptable;

	void Start()
	{
		id = this.gameObject.name;
		currentSpeed = 0;
	}

	void Update()
	{
		/* if currently being thrown... */
		if (lastThrowMovement.magnitude > 0)
		{
			/* if the throw should damage */ 
			if (currentSpeed <= weaponScriptable.throwHurtSpeed)
			{
				hitbox.enabled = false;
			}
			if (currentSpeed <= 0)
			{
				lastThrowMovement = new Vector3(0, 0, 0);
				currentSpeed = 0;
			}
		}
	}

	void FixedUpdate()
	{
		// was just thrown, so give it initial speed
		if (throwMovement.magnitude > 0)
		{
			lastThrowMovement = new Vector3(throwMovement.x, throwMovement.y, 0);
			throwMovement = new Vector3(0, 0, 0);
			currentSpeed = weaponScriptable.throwSpeed;
		}
		
		if (lastThrowMovement.magnitude > 0)
		{
			currentSpeed -= weaponScriptable.throwWeight;
			this.transform.parent.Rotate(new Vector3(0, 0, 100 * (currentSpeed / weaponScriptable.throwSpeed) * weaponScriptable.throwWeight * Time.deltaTime));
			controller.MoveRect(new Vector3(lastThrowMovement.x * currentSpeed * Time.deltaTime, lastThrowMovement.y * currentSpeed * Time.deltaTime));
		}
	}

	public bool attack(LayerMask targetLayer)
	{
		anim.SetTrigger("Attack");
		lastTargetLayer = targetLayer;
		
		return true;
	}
	public float getSpeed()
	{
		return weaponScriptable.atkSpeed;
	}

	public bool isActive()
	{
		return (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Idle") || lastThrowMovement.magnitude > 0);
	}


	public void setStartingPosition()
	{
		Vector3 startPos;
		Quaternion startRot;
		startPos = new Vector3(equipPosX, equipPosY);
		startRot = Quaternion.Euler(0, 0, equipRotZ);
		this.gameObject.transform.parent.SetLocalPositionAndRotation(startPos, startRot);
		//currParent.SetPositionAndRotation(currParent.TransformPoint(currParent.position), currParent.rotation);
	}

	/* Only deal with the movement of the throw */
	public void throwWeapon(Vector3 target)
	{
		throwMovement = target;
		hitbox.enabled = true;
	}

	public bool canBeDropped()
	{
		if (id == "Unarmed")
		{
			return false;
		}

		return true;
	}

	public bool toggleCollider()
	{
		return arc.enabled = !arc.enabled;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//TODO: deal with layermasks in a way that actually makes sense later
		Transform currParent = this.gameObject.transform;
		while(currParent != null)
		{
			if (collision.name == currParent.name)
			{
				Debug.Log("Stop hitting yourself");
				return;
			}
			currParent = currParent.transform.parent;
		}

		Actor actorHit = collision.GetComponent<Actor>();
		if (actorHit != null)
		{
			actorHit.takeDamage(weaponScriptable.damage);
			Debug.Log("Hit: " + collision.name + " for " + weaponScriptable.damage + " damage");
		}
		else
		{
			Debug.Log("Hit: " + collision.name);
		}
	}
}