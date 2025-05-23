using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class MutationDefs
{
	public static string MUTATION_SELECT_TAG	= "MutationSelect";

	public static string TRIGGER_BEAST			= "TriggerBeast";
	public static string TRIGGER_BLADE_WING		= "TriggerBladeWing";

	public static string TRIGGER_BEAST_ATK2		= "AttackBeast2";
	public static string TRIGGER_BEAST_ATK3		= "AttackBeast3";
	public static string TRIGGER_STOP_TRANSFORM = "StopTransform";

	public static float ABILITY_BUFF_TIMER		= 0.5F;

	public static string NAME_BEAST = "BEAST";
	public static string NAME_LIMB  = "LIMB";
	public static string NAME_SPORE = "SPORE";
	public static string NAME_WINGS = "WINGS";

	public static short MAX_SLOTS = 2;

	[System.Serializable]
	public struct MutationData
	{
		public int mutationBar;
		public int maxMutationBar;
	};

	public static bool isMutationSelect(GameObject target)
	{
		return target.tag.StartsWith("Mutation") && target.tag.Equals(MutationDefs.MUTATION_SELECT_TAG);
	}
}

public enum mutationType
{
	mBeast = 0,
	mLimb = 1,
	mWing = 2,
	mSpore = 3
};

public enum mutationTrigger
{
	IS_WEAPON,
	PRIMARY_ATTACK,
	SECONDARY_ATTACK,
	DAMAGE_TAKEN,
	DAMAGE_GIVEN,
	ACTIVE_SLOT
};