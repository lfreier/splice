using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSound", menuName = "ScriptableObjects/SoundScriptable")]
public class SoundScriptable : ScriptableObject
{
	[field: SerializeField]
	public float length { get; private set; } = 0.5F;

	[field: SerializeField]
	public float radius { get; private set; } = 1F;

	[field: SerializeField]
	public SoundDefs.SoundType type{ get; private set; } = SoundDefs.SoundType.TAP;

	[field: SerializeField]
	/* Speed at which the object must be moving to make sound (not always used) */
	public float triggerSpeed { get; private set; } = 6F;

	[field: SerializeField]
	/* Threshold value which must be reached to trigger the sound (NOT a common unit, threhsold value is case by case) */
	public float threshold { get; private set; } = 2F;

	[field: SerializeField]
	public float volume { get; private set; } = 0.1F;
}