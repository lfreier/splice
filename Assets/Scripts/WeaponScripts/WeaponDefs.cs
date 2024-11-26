using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class WeaponDefs
{
	public static string OBJECT_WEAPON_TAG			= "ObjectWeapon";
	public static string EQUIPPED_WEAPON_TAG		= "EquippedWeapon";
	public static string EQUIPPED_ACTIVE_TAG		= "EquippedActive";

	public static string SORT_LAYER_GROUND			= "Ground";
	public static string SORT_LAYER_CHARS			= "Chars";

	public static string ANIM_BOOL_ONLY_LEFT		= "OnlyLeft";
	public static string ANIM_BOOL_ONLY_RIGHT		= "OnlyRight";
	public static string ANIM_TRIGGER_SWAP_SIDE		= "SwapSide";
	public static string ANIM_TRIGGER_ATTACK		= "Attack";
	public static string ANIM_TRIGGER_ATTACK_SEC	= "AttackSecondary";
	public static string ANIM_TRIGGER_TIMEOUT = "Timeout";

	public static float THROW_ROTATE_LOW = 100.0F;
	public static float THROW_ROTATE_MID = 350.0F;
	public static float THROW_ROTATE_HIGH = 600.0F;

	public static float KNOCKBACK_MULT_PARRY = 1200F;

	public static float DURABILITY_DAMAGED_DIVIDER = 3F;

	public static float KNOCKBACK_MULT_SWING = 1600F;

	public static bool canWeaponBePickedUp(GameObject target)
	{
		return target.tag.StartsWith("Object") && target.tag.Equals(WeaponDefs.OBJECT_WEAPON_TAG);
	}


	/* Sets the GameObject tag of the given weapon
	 * Mainly used when dropping the weapon, since tag is used for knowing if the weapon is equipped or not
	 */
	public static void setWeaponTag(GameObject weapon, string newTag)
	{
		weapon.tag = newTag;
		foreach (Transform child in weapon.transform)
		{
			child.tag = newTag;
			foreach (Transform secChild in child)
			{
				secChild.tag = newTag;
			}
		}
	}
}
public enum WeaponType
{
	UNARMED,
	SWING,
	STAB,
	SHOOT
};