using System.Collections;
using UnityEngine;

public static class SoundDefs
{
	public enum SoundType
	{
		TAP = 0,
		CLINK = 1,
		THUD = 2,
		CLANG = 3,
		YELL = 4
	}

	public static string SOUND_LAYER_NAME = "Sound";

	public static string SOUND_FOOTSTEP = "footstep";
	public static string SOUND_WEAPON_THUD = "weaponThud";
	public static string SOUND_WEAPON_CLANG = "weaponClang";

	public static string TAG_WALL_METAL = "WallMetal";

	static public void createSound(Vector2 worldPosition, SoundScriptable scriptable)
	{
		if (scriptable == null)
		{
			Debug.Log("No sound defined");
			return;
		}
		GameObject newSound = new GameObject(SOUND_LAYER_NAME);
		Sound script = newSound.AddComponent<Sound>();

		newSound.transform.position = worldPosition;
		newSound.layer = LayerMask.NameToLayer(SOUND_LAYER_NAME);

		CircleCollider2D collider = newSound.AddComponent<CircleCollider2D>();

		collider.radius = scriptable.radius;
		script.start(scriptable);
	}
}