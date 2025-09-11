using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public GameObject toSpawn;
	public BoxCollider2D spawnBox;
	public int amountToSpawn = 1;
	public int maxSpawnAmount = 5;

	private float spawnTimer = 0;
	public float spawnTimerLength = 5F;
	public float initialSpawnTimer = 5F;

	public List<GameObject> spawned;

	private void Start()
	{
		spawnTimer = initialSpawnTimer + Time.deltaTime;
		spawned = new List<GameObject>();
	}

	private void FixedUpdate()
	{
		if (spawnTimer > 0)
		{
			spawnTimer -= Time.deltaTime;
			if (spawnTimer <= 0)
			{
				spawnNew();
				spawnTimer = spawnTimerLength;
			}
		}
	}

	void spawnNew()
	{
		for (int i = 0; i < spawned.Count; i ++)
		{
			GameObject toCheck = spawned[i];
			if (toCheck == null)
			{
				spawned.RemoveAt(i);
				i--;
			}
		}

		int totalToSpawn = Mathf.Min(spawned.Count + amountToSpawn, maxSpawnAmount);
		for (int i = spawned.Count; i < totalToSpawn; i++)
		{
			float randX = Random.Range(spawnBox.bounds.min.x, spawnBox.bounds.max.x);
			float randY = Random.Range(spawnBox.bounds.min.y, spawnBox.bounds.max.y);

			GameObject newObj = Instantiate(toSpawn, new Vector2(randX, randY), Quaternion.identity, null);
			if (newObj != null)
			{
				spawned.Add(newObj);
			}
		}
	}
}