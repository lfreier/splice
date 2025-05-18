using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSpawn", menuName = "ScriptableObjects/PlayerSpawnScriptable")]

public class PlayerSpawnScriptable : ScriptableObject
{
	[field: SerializeField]
	public Vector2 spawnPosition;

	[field: SerializeField]
	public float spawnRotation;

	[field: SerializeField]
	public LevelManager.elevatorIndex elevatorIndex;

	[field: SerializeField]
	public string floorName;

	[field: SerializeField]
	public SceneDefs.SCENE sceneIndex;

	[field: SerializeField]
	public LevelManager.levelSpawnIndex spawnIndex;
}