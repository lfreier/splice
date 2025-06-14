using System.Collections;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MGrabber : MonoBehaviour
{
	public Actor wielder;

	public float grabOffset;

	public int mutCost1;
	public int mutCost2;

	public Transform[] grabbingTransforms;
	public Transform holdingTransform;

	public GameObject grabbedObject;
	public Rigidbody2D heldRigidbody;
	public Obstacle heldObstacle;
	public Collider2D heldObstacleCollider;

	public GameObject actorObstaclePrefab;

	private GameManager gameManager;

	private void Start()
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}
	}

	public void addObjectToGrabber(Transform objectTransform)
	{
		grabbedObject = objectTransform.gameObject;

		Rigidbody2D objBody = grabbedObject.GetComponentInChildren<Rigidbody2D>();
		if (objBody == null)
		{
			objBody = grabbedObject.GetComponentInParent<Rigidbody2D>();
		}
		if (objBody != null)
		{
			heldRigidbody = objBody;
			heldRigidbody.simulated = false;

			Obstacle obstacle = grabbedObject.GetComponentInChildren<Obstacle>();
			if (obstacle == null)
			{
				obstacle = grabbedObject.GetComponentInParent<Obstacle>();
			}
			if (obstacle != null)
			{
				heldObstacle = obstacle;
				Collider2D obstacleCollider = obstacle.GetComponent<Collider2D>();
				if (obstacleCollider != null)
				{
					heldObstacleCollider = obstacleCollider;
					obstacleCollider.isTrigger = true;
				}
				WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_CHARS, heldObstacle.gameObject);

				Vector3 grabPos = holdingTransform.position + (holdingTransform.up * grabOffset);
				grabbedObject.transform.SetParent(holdingTransform);
				grabbedObject.transform.SetPositionAndRotation(grabPos, grabbedObject.transform.rotation);
			}
		}
	}

	private bool checkGrabObject(Transform currChild, Actor grabbedActor, int cost)
	{
		while (currChild != null)
		{
			if (currChild.parent == null)
			{
				/* only spend mut energy for certain objects */
				float currMutEnergy = gameManager.playerStats.getMutationBar();
				if (currMutEnergy >= cost)
				{
					grabbedObject = currChild.gameObject;

					Vector3 grabPos = holdingTransform.position + (holdingTransform.up * grabOffset);
					grabbedObject.transform.SetParent(holdingTransform);
					grabbedObject.transform.SetPositionAndRotation(grabPos, grabbedObject.transform.rotation);

					if (grabbedActor != null)
					{
						EffectDefs.effectApply(grabbedActor, grabbedActor.gameManager.effectManager.stunPermanent);
						if (cost > 0)
						{
							gameManager.playerStats.changeMutationBar(-mutCost2);
						}
					}
					else if (cost > 0)
					{
						gameManager.playerStats.changeMutationBar(-mutCost1);
					}
					return true;
				}
				else
				{
					heldRigidbody = null;
				}
			}
			currChild = currChild.parent;
		}

		return false;
	}
	public bool grabObjects(LayerMask grabLayers, float grabRadius, weightClass maxWeightClass, bool grabPickups)
	{
		foreach (Transform grab in grabbingTransforms)
		{
			if (grab == null)
			{
				continue;
			}

			Vector3 grabPos = grab.position + (grab.up * grabOffset);

			Collider2D[] hitTargets = Physics2D.OverlapCircleAll(grabPos, grabRadius, grabLayers);
			foreach (Collider2D target in hitTargets)
			{
				if (grabTargetCollider(target, maxWeightClass, grabPickups))
				{
					/* grabbed object found */
					return true;
				}
				else
				{
					continue;
				}
			}
		}

		return false;
	}

	/* returns true if the target collider is grabbed
	 * a.k.a true to break, false to continue
	 */
	public bool grabTargetCollider(Collider2D target, weightClass maxWeight, bool grabPickups)
	{
		/* weapon has to only check for parent, because of equipped weapons */
		WeaponInterface weapon = target.GetComponentInParent<WeaponInterface>();

		PickupBox box = target.GetComponentInChildren<PickupBox>();
		Obstacle obstacle = target.GetComponentInChildren<Obstacle>();
		Actor actor = target.GetComponentInChildren<Actor>();
		PickupInterface pickup = target.GetComponentInChildren<PickupInterface>();

		if (weapon != null && grabPickups)
		{
			if (WeaponDefs.canWeaponBePickedUp(target.gameObject))
			{
				if (checkGrabObject(target.gameObject.transform, null, 0))
				{
					return true;
				}
			}
			return false;
		}

		if (obstacle == null)
		{
			obstacle = target.GetComponentInParent<Obstacle>();
		}
		if (obstacle != null && obstacle._obstacleScriptable.weight <= maxWeight)
		{
			int cost = mutCost1;
			/*
			if (obstacle._obstacleScriptable.weight == maxWeight)
			{
				cost = mutCost2;
			}
			*/
			Rigidbody2D objBody = target.GetComponentInChildren<Rigidbody2D>();
			if (objBody == null)
			{
				objBody = target.GetComponentInParent<Rigidbody2D>();
			}
			if (objBody != null)
			{
				/* Now hold the object and wait to release */
				if (checkGrabObject(target.gameObject.transform, null, cost))
				{
					heldRigidbody = objBody;
					heldRigidbody.simulated = false;
					Collider2D obstacleCollider = obstacle.GetComponent<Collider2D>();
					if (obstacleCollider != null)
					{
						heldObstacleCollider = obstacleCollider;
						obstacleCollider.isTrigger = true;
					}
					heldObstacle = obstacle;
					WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_CHARS, heldObstacle.gameObject);
					return true;
				}
				return false;
			}
		}

		if (actor == null)
		{
			actor = target.GetComponentInParent<Actor>();
		}
		if (actor != null)
		{
			Rigidbody2D objBody = target.GetComponentInChildren<Rigidbody2D>();
			if (objBody == null)
			{
				objBody = target.GetComponentInParent<Rigidbody2D>();
			}
			if (objBody != null && actor.name != wielder.name)
			{
				/* Now hold the object and wait to release */
				if (checkGrabObject(target.gameObject.transform, actor, mutCost1))
				{
					heldRigidbody = objBody;
					heldRigidbody.simulated = false;
					return true;
				}
				return false;
			}
		}

		if (box == null)
		{
			box = target.GetComponentInParent<PickupBox>();
		}
		/* if trying to grab a pickup box, pick up the whole thing */
		if (box != null && obstacle == null)
		{
			GameObject boxPickup = box.getPickup();
			if (boxPickup != null)
			{
				if (checkGrabObject(boxPickup.transform, null, 0))
				{
					return true;
				}
				return false;
			}
		}

		if (pickup == null)
		{
			pickup = target.GetComponentInParent<PickupInterface>();
		}
		if (pickup != null && grabPickups)
		{
			if (checkGrabObject(target.gameObject.transform, null, 0))
			{
				return true;
			}
		}

		return false;
	}

	public void releaseHeldObject(float objectReleaseForce)
	{
		if (heldRigidbody == null)
		{
			return;
		}

		float releaseForce = objectReleaseForce;
		heldRigidbody.simulated = true;
		heldRigidbody.bodyType = RigidbodyType2D.Dynamic;
		heldRigidbody.transform.SetParent(null, true);

		Vector3 releaseTarget = wielder.transform.position + (wielder.transform.up * 2);
		CollisionExclusion exclusion = heldRigidbody.AddComponent<CollisionExclusion>();

		if (heldObstacle != null)
		{
			heldObstacle.beingHeld = false;
			heldObstacleCollider.isTrigger = false;
			heldObstacle.enablePhysics();
			releaseForce = Mathf.Min(objectReleaseForce, heldObstacle._obstacleScriptable.maxObstacleForce);
			WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_GROUND, heldObstacle.gameObject);
		}

		Obstacle obs = null;
		Actor grabbedActor = heldRigidbody.GetComponentInChildren<Actor>();
		if (grabbedActor != null)
		{
			EffectDefs.effectApply(grabbedActor, grabbedActor.gameManager.effectManager.stun1);
			releaseForce = objectReleaseForce / 4.5F;
			GameObject actorObs = Instantiate(actorObstaclePrefab, grabbedActor.transform);
			if (actorObs != null)
			{
				obs = actorObs.GetComponentInChildren<Obstacle>();
				if (obs != null)
				{
					obs.obstacleBody = heldRigidbody;
				}
			}
		}

		/* Disable collisions for short amount of time to avoid player collision */
		Collider2D[] colliders1 = new Collider2D[wielder.actorBody.attachedColliderCount];
		Collider2D[] colliders2 = new Collider2D[heldRigidbody.attachedColliderCount]; ;
		/*if (obs == null)
		{
			colliders2 = new Collider2D[heldRigidbody.attachedColliderCount + 1];
		}
		else
		{
			colliders2 = new Collider2D[heldRigidbody.attachedColliderCount];
		}*/
		wielder.actorBody.GetAttachedColliders(colliders1);
		heldRigidbody.GetAttachedColliders(colliders2);

		/*
		if (obs != null)
		{
			colliders2[heldRigidbody.attachedColliderCount] = obs.;
		}
		*/
		exclusion.init(0.1F, colliders1, colliders2);

		heldRigidbody.AddForce(Vector2.ClampMagnitude(releaseTarget - heldRigidbody.transform.position, 1) * releaseForce);
		heldRigidbody = null;
		heldObstacle = null;
		heldObstacleCollider = null;
	}

}