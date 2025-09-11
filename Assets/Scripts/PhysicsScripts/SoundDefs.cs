using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneDefs;

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

	static public void createSound(Vector2 worldPosition, SoundScriptable scriptable, Actor origin)
	{
		if (scriptable == null)
		{
			Debug.Log("No sound defined");
			return;
		}

		GameManager gm = GameManager.Instance;
		if (gm != null && gm.audioManager.soundPrefab != null)
		{
			GameObject soundObj = GameObject.Instantiate(gm.audioManager.soundPrefab, worldPosition, Quaternion.identity);
			Sound script = soundObj.GetComponent<Sound>();
			if (script != null)
			{
				script.startSound(scriptable, origin);
			}
		}
	}
}