using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
	public PathGrid grid;
	public bool startPathfinding;
	public float pathingTimer;

	public void addUnwalkable(Vector3 unwalkablePosition)
	{
		PathNode nodeToChange = grid.nodeFromWorldPosition(unwalkablePosition);
		nodeToChange.walkable = false;
		if (grid.path != null)
		{
			grid.path.Remove(nodeToChange);
		}
	}

	public void findPath(Vector3 start, Vector3 target)
	{
		PathNode startNode = grid.nodeFromWorldPosition(start);
		PathNode targetNode = grid.nodeFromWorldPosition(target);

		List<PathNode> openSet = new List<PathNode>();
		HashSet<PathNode> closedSet = new HashSet<PathNode>();

		openSet.Add(startNode);
		startNode.gCost = 0;

		while (openSet.Count > 0)
		{
			PathNode curr = openSet[0];
			for (int i = 1; i < openSet.Count; i++)
			{
				if (openSet[i].fCost < curr.fCost || openSet[i].fCost == curr.fCost)
				{
					if (openSet[i].hCost < curr.hCost)
						curr = openSet[i];
				}
			}


			openSet.Remove(curr);
			closedSet.Add(curr);

			if (curr == targetNode)
			{
				retracePath(startNode, targetNode);
				return;
			}

			foreach (PathNode neighbor in grid.getNeighbors(curr))
			{
				if (!neighbor.walkable || closedSet.Contains(neighbor))
				{
					continue;
				}

				int newMoveCost = curr.gCost + getNodeDistance(curr, neighbor);
				if (newMoveCost < neighbor.gCost || !openSet.Contains(neighbor))
				{
					neighbor.gCost = newMoveCost;
					neighbor.hCost = getNodeDistance(neighbor, targetNode);
					neighbor.parent = curr;

					if (!openSet.Contains(neighbor))
					{
						openSet.Add(neighbor);
					}
				}
			}
		}
	}

	public Vector3 getNextMove(Vector3 currPosition, float targetError)
	{
		if (grid.path == null || grid.path.Count <= 0)
		{
			return currPosition;
		}
		else if (grid.path.Count == 1)
		{
			return grid.path[0].position;
		}

		Vector3 nextPosition;
		if ((grid.path[0].position - currPosition).magnitude < targetError)
		{
			nextPosition = grid.path[1].position;
			grid.path.RemoveAt(0);
		}
		else
		{
			nextPosition = grid.path[0].position;
		}

		return nextPosition;
	}

	void retracePath (PathNode start, PathNode end)
	{
		List<PathNode> path = new List<PathNode>();
		PathNode curr = end;

		while(curr != start)
		{
			path.Add(curr);
			curr = curr.parent;
		}

		path.Reverse();

		grid.path = path;
	}

	int getNodeDistance(PathNode a, PathNode b)
	{
		int distX = Mathf.Abs(a.gridX - b.gridX);
		int distY = Mathf.Abs(a.gridY - b.gridY);

		if (distX > distY)
		{
			return (4 * distY) + (10 * distX);
		}

		return (10 * distY) + (4 * distX);
	}
}