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
	public float damage { get; private set; } = 2F;

	public int durability = 2;

	/* Throwing fields */  
	[field: SerializeField]
	public float throwDamage { get; private set; } = 1F;

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
	public WeaponDefs weaponType { get; private set; }  = WeaponDefs.SWING;

}