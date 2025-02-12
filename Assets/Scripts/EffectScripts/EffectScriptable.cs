﻿using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEffect", menuName = "ScriptableObjects/EffectScriptable")]

public class EffectScriptable : ScriptableObject
{
	[field: SerializeField]
	public EffectDefs.constantType constantEffectType { get; private set; } = EffectDefs.constantType.NONE;

	[field: SerializeField]
	/* Effect length in seconds */
	public float effectLength { get; private set; } = 1F;

	[field: SerializeField]
	public GameObject effectPrefab;

	[field: SerializeField]
	public Sprite effectSprite;

	[field: SerializeField]
	public float effectStrength { get; private set; } = 1F;

	[field: SerializeField]
	/* Length until effect is applied in seconds */
	public float tickLength { get; private set; } = 1F;
}