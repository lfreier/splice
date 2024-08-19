using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewObstacle", menuName = "ScriptableObjects/ObstacleScriptable")]
public class ObstacleScriptable : ScriptableObject
{
	[field: SerializeField]
	public float collisionDamageThreshold { get; private set; } = 2F;

	[field: SerializeField]
	public float collisionDamage { get; private set; } = 1F;

	[field: SerializeField]
	public float pushbackForce { get; private set; } = 1F;
}