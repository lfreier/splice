using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMutation", menuName = "ScriptableObjects/MutationScriptable")]

public class MutationScriptable : ScriptableObject
{
	[field: SerializeField]
	public mutationTrigger trigger { get; private set; } = mutationTrigger.IS_WEAPON;

	[field: SerializeField]
	public EffectScriptable effectScriptable { get; private set; } = new EffectScriptable();
}