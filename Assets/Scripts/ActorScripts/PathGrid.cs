using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGrid
{	
	public Vector2 gridWorldSize;
	private float nodeRadius;
	PathNode[,] nodeGrid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	public List<PathNode> path = null;

	private GameManager gameManager;

	public void init(Vector2 gridSize, float _nodeRadius)
	{
		gameManager = GameManager.Instance;
		if (gameManager == null)
		{
			Debug.Log("Game manager is null when it shouldn't be: PathGrid.cs");
			return;
		}

		gridWorldSize = gridSize;
		nodeRadius = _nodeRadius;

		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		createGrid();
	}


	void createGrid()
	{
		nodeGrid = new PathNode[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = gameManager.levelManager.gridPosition - (Vector3.right * gridWorldSize.x / 2) - (Vector3.up * gridWorldSize.y / 2);
		worldBottomLeft.x -= nodeRadius;
		worldBottomLeft.y -= nodeRadius;

		for (int x = 0; x < gridSizeX; x ++)
		{
			for (int y = 0; y < gridSizeY; y ++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics2D.CircleCast(worldPoint, nodeRadius, Vector3.forward, 1, gameManager.unwalkableLayers));
				nodeGrid[x, y] = new PathNode(walkable, worldPoint, x, y);
			}
		}
	}

	public List<PathNode> getNeighbors(PathNode node)
	{
		List<PathNode> neighborList = new List<PathNode>();

		for (int x = -1; x <= 1; x ++)
		{
			for (int y = -1; y <= 1; y ++)
			{
				int newX = node.gridX + x;
				int newY = node.gridY + y;

				if (x == 0 && y == 0
				|| (newX < 0 || newX >= gridSizeX) 
				|| (newY < 0 || newY >= gridSizeY)) continue;

				nodeGrid[newX, newY].walkable = !(Physics2D.CircleCast(nodeGrid[newX, newY].position, nodeRadius, Vector3.forward, 1, gameManager.unwalkableLayers));
				neighborList.Add(nodeGrid[newX, newY]);
			}
		}

		return neighborList;
	}

	public PathNode nodeFromWorldPosition(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;

		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

		/* I'm kinda dumb */
		if (worldPosition.x > 0)
			x++;
		if (worldPosition.y > 0)
			y++;

		return nodeGrid[x, y];
	}
}