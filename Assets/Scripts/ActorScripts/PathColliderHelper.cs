using System.Collections;
using UnityEngine;

public class PathColliderHelper : MonoBehaviour
{
	public Pathfinding pathfinder;

	public void fixLivelock()
	{

	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		return;
		//turnOnPathfinding(collision);
		
	}

	void turnOnPathfinding(Collider2D collision)
	{
		if (pathfinder.pathingTimer > 0)
		{
			return;
		}

		int actorLayer = LayerMask.NameToLayer(GameManager.ACTOR_LAYER);
		int collLayer = LayerMask.NameToLayer(GameManager.WALL_COLLISION_LAYER);
		int objectMidLayer = LayerMask.NameToLayer(GameManager.OBJECT_MID_LAYER);
		int objectHighLayer = LayerMask.NameToLayer(GameManager.OBJECT_HIGH_LAYER);

		if (actorLayer == collision.gameObject.layer)
		{
			/* randomly choose one to get a new path */
		}
		else if (collLayer == collision.gameObject.layer)
		{
			/* start pathfinding */
		}
		else if (objectMidLayer == collision.gameObject.layer || objectHighLayer == collision.gameObject.layer)
		{
			/* start pathfinding - maybe deal with velocity? */
		}
		else
		{
			return;
		}

		pathfinder.addUnwalkable(collision.transform.position);
		pathfinder.startPathfinding = true;
	}
}