using System.Collections;
using UnityEngine;

public class OpenDoorsOnDeath : MonoBehaviour
{
	public AutoDoor[] toUnlock;

	public EnemyMove[] actorsToPath;
	public Vector2[] paths;

	private void Start()
	{
		if (toUnlock != null && toUnlock.Length > 0)
		{
			foreach (AutoDoor door in toUnlock)
			{
				if (door == null)
				{
					continue;
				}

				if (door._doorType == AutoDoor.doorType.AUTO)
				{
					door.doorUnlock();
				}
				else if (!door.isOpen())
				{
					door.doorToggle(true);
				}
			}
		}

		if (actorsToPath != null && actorsToPath.Length > 0
			&& paths != null && paths.Length > 0
			&& actorsToPath.Length == paths.Length)
		{
			int i = -1;
			foreach (EnemyMove move in actorsToPath)
			{
				i++;
				if (move == null)
				{
					continue;
				}

				move._detection = ActorDefs.detectMode.idle;
				move.forcedTimer = 10F;
				move.idlePath = new Vector2[] { paths[i] };
				move.idlePathPauseTime = new float[] { 4F };
				move.moveTarget = paths[i];
				move.actor.actorBody.rotation = move.actor.aimAngle(paths[i]);
				move.pathIndex = 0;
			}
		}

		Destroy(gameObject);
	}
}