using System.Collections;
using UnityEngine;

public class SpawnSprite : MonoBehaviour
{
	public Vector2 spawnLocation;
	public float spawnRotation;

	void Start()
	{
		this.gameObject.transform.SetPositionAndRotation(spawnLocation, Quaternion.identity);
		this.gameObject.transform.Rotate(0, 0, spawnRotation, Space.Self);
	}
}