using NUnit.Framework;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMutation", menuName = "ScriptableObjects/MutationScriptable")]

public class MutationScriptable : ScriptableObject
{
	[field: SerializeField]
	public float damage { get; private set; } = 1F;

	[field: SerializeField]
	public mutationTrigger trigger { get; private set; } = mutationTrigger.IS_WEAPON;

	[field: SerializeField]
	public Sprite[] tutorialSprites { get; private set; } = null;

	[field: SerializeField]
	public EffectScriptable effectScriptable { get; private set; } = null;

	[field: SerializeField]
	public int mutCost { get; private set; } = 100;

	[field: SerializeField]
	public Vector2 startingPosition { get; private set; } = new Vector2(-0.621F, 0.28F);

	[field: SerializeField]
	public float startingRotation { get; private set; } = 7F;

	[field: SerializeField]
	public float[] values { get; private set; } = new float[10];
}