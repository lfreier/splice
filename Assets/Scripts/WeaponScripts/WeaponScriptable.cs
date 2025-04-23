using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/WeaponScriptable")]
public class WeaponScriptable : ScriptableObject
{

	[field: SerializeField]
	/* Arc of swing in degrees for swing weapons 
	 * Or alt fire stab weapons
	 * Bullet spread arc for guns
	 */
	public float arc { get; private set; } = 135F;

	[field: SerializeField]
	public float atkSpeed { get; private set; } = 0.25F;

	[field: SerializeField]
	public bool canBeDropped { get; private set; } = true;

	[field: SerializeField]
	public float damage { get; private set; } = 2F;

	[field: SerializeField]
	public string description { get; private set; } = "";

	[field: SerializeField]
	public float durability { get; private set; } = 2F;

	[field: SerializeField]
	public Sprite icon { get; private set; } = null;

	[field: SerializeField]
	public string displayName { get; private set; } = "";

	[field: SerializeField]
	public float npcAttackRange { get; private set; } = 2F;

	[field: SerializeField]
	public WeaponDefs.prefabIndex prefabIndex { get; private set; } = WeaponDefs.prefabIndex.RULER;

	[field: SerializeField]
	public float secondaryDamage { get; private set; } = 2F;

	[field: SerializeField]
	public AudioClip soundActorHit { get; private set; } = null;

	[field: SerializeField]
	public AudioClip soundBreak { get; private set; } = null;

	[field: SerializeField]
	public AudioClip soundObstacleHit { get; private set; } = null;

	[field: SerializeField]
	public AudioClip soundSwing { get; private set; } = null;

	[field: SerializeField]
	public AudioClip soundWallHit { get; private set; } = null;

	/* Throwing fields */
	[field: SerializeField]
	public float throwDamage { get; private set; } = 1F;

	[field: SerializeField]
	public float throwDurabilityDamage { get; private set; } = 0.5F;

	/* Higher throw speed means more throw range. */
	[field: SerializeField]
	public float throwSpeed { get; private set; } = 50F;

	/* How fast the throw will slow down after the initial speed. */
	[field: SerializeField]
	public float throwWeight { get; private set; } = 1F;

	/* Higher throw speed means more throw range. */
	[field: SerializeField]
	public float throwHurtSpeed { get; private set; } = 15F;


	[field: SerializeField]
	public WeaponType weaponType { get; private set; }  = WeaponType.SWING;

	public float equipPosX = 0.38F;
	public float equipPosY = 0.15F;
	public float equipRotZ = -67.5F;

	public float equipOtherPosX = -0.38F;
	public float equipOtherPosY = -0.625F;
	public float equipOtherRotZ = -115F;

	[field: SerializeField]
	/* In percent */
	public float knockbackDamage { get; private set; } = 1F;
}