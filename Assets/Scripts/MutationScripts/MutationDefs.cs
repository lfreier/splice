using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class MutationDefs
{
	public static string MUTATION_SELECT_TAG = "MutationSelect";

	public static short MAX_SLOTS = 2;

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
	DAMAGE_GIVEN,
	ACTIVE_SLOT
};