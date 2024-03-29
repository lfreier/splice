using UnityEditor;
using UnityEngine;

public class MutationScriptable : ScriptableObject
{
	[field: SerializeField]
	public mutationTrigger trigger { get; private set; } = mutationTrigger.NEW_ATTACK;
}