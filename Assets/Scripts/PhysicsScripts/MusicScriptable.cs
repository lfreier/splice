using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMusic", menuName = "ScriptableObjects/MusicScriptable")]
public class MusicScriptable : ScriptableObject
{
	[field: SerializeField]
	public AudioClip audioClip { get; private set; } = null;

	[field: SerializeField]
	public float volume { get; private set; } = 0.1F;
}