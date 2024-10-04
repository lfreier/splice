using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewActor", menuName = "ScriptableObjects/ActorScriptable")]

public class ActorScriptable : ScriptableObject
{

	[field: SerializeField]
	public float health { get; private set; } = 2F;

	[field: SerializeField]
	public float attackTimer { get; private set; } = 0.2F;

	[field: SerializeField]
	public float maxSpeed { get; private set; } = 25F;

	[field: SerializeField]
	public float moveSpeed { get; private set; } = 5F;

	[field: SerializeField]
	public float acceleration { get; private set; } = 0.5F;

	[field: SerializeField]
	public float deceleration { get; private set; } = 0.8F;

	[field: SerializeField]
	public float hearingRange { get; private set; } = 20F;

	[field: SerializeField]
	public float sightRange { get; private set; } = 10F;

	[field: SerializeField]
	/* In degrees - must be a multiple of 10 */
	public float sightAngle { get; private set; } = 110F;

	[field: SerializeField]
	public float frightenedDistance { get; private set; } = 3F;

	[field: SerializeField]
	/*  */
	public float knockbackResist { get; private set; } = 5F;
}