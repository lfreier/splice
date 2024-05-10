using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class MutationDefs
{
	public static string MUTATION_SELECT_TAG = "MutationSelect";

	public static string BLEED_EFFECT_1 = "";

	public static bool isMutationSelect(GameObject target)
	{
		return target.tag.StartsWith("Mutation") && target.tag.Equals(MutationDefs.MUTATION_SELECT_TAG);
	}
}

public enum mutationTrigger
{
	IS_WEAPON,
	PRIMARY_ATTACK,
	SECONDARY_ATTACK,
	DAMAGE_TAKEN,
	DAMAGE_GIVEN
};