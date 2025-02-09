using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSpawn", menuName = "ScriptableObjects/PlayerSpawnScriptable")]

public class PlayerSpawnScriptable : ScriptableObject
{
	[field: SerializeField]
	public Vector2 spawnPosition;

	[field: SerializeField]
	public float spawnRotation;
}