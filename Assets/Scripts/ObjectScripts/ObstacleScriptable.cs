using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewObstacle", menuName = "ScriptableObjects/ObstacleScriptable")]
public class ObstacleScriptable : ScriptableObject
{
	[field: SerializeField]
	public float actorPushForce { get; private set; } = 10F;

	[field: SerializeField]
	public float collisionDamageThreshold { get; private set; } = 2F;

	[field: SerializeField]
	public float collisionDamage { get; private set; } = 1F;

	[field: SerializeField]
	public float durability { get; private set; } = 3F;

	[field: SerializeField]
	public float heldDamageBonus { get; private set; } = 1F;

	[field: SerializeField]
	public float maxObstacleForce { get; private set; } = 8000F;

	[field: SerializeField]
	public ObstacleDefs.type type { get; private set; } = ObstacleDefs.type.NORMAL;

	[field: SerializeField]
	public float weaponDurabilityDamage { get; private set; } = 0.1F;

	[field: SerializeField]
	public float weaponHitMult { get; private set; } = 2F;

	[field: SerializeField]
	/* Internal weight 'class' - determines if an obstacle can be moved in certain situations (like being grabbed by the limb) */
	public weightClass weight { get; private set; } = weightClass.MID;
}