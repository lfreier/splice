using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	private static GameManager instance = null;

	public GameObject mutPLimb;

	public GameObject weapPBladeArm;
	public GameObject weapPFist;

	public LayerMask actorLayers;
	public LayerMask collisionLayer;
	public LayerMask lineOfSightLayers;
	public LayerMask findWeaponLayers;
	public LayerMask soundLayer;

	public static string DAMAGE_LAYER = "Damage";
	public static string OBJECT_LAYER = "Object";
	public static string OBJECT_MID_LAYER = "ObjectMid";
	public static string COLLISION_ACTOR_LAYER = "CollisionActor";
	public static string UI_LAYER = "UI";

	public static string CHAR_SCRIP_ID_SCIENTIST = "scientist";

	public static string EFCT_SCRIP_ID_BLEED1 = "bleed1";

	public static string EFCT_SCRIP_ID_STUNHALF = "stunHalf";
	public static string EFCT_SCRIP_ID_STUN1 = "stun1";
	public static string EFCT_SCRIP_ID_STUN3 = "stun3";
	public static string EFCT_SCRIP_ID_IFRAME0 = "iframe0";
	public static string EFCT_SCRIP_ID_IFRAME1 = "iframe1";

	public static string WEAP_SCRIP_ID_BLADEARM = "bladeArm";
	public static string WEAP_SCRIP_ID_FISTS = "fists";
	public static string WEAP_SCRIP_ID_RULER = "ruler";

	public static Color COLOR_BLUE = new Color(0.1F, 0.1F, 0.4F, 1F);
	public static Color COLOR_GREEN = new Color(0.15F, 0.4f, 0, 1F);
	public static Color COLOR_RED = new Color(0.4F, 0.1F, 0.1F, 1F);
	public static Color COLOR_IFRAME = new Color(0.9F, 0.3F, 0.3F, 1F);

	public List<Type> actorBehaviors = new List<Type>();

	private Dictionary<string, ActorScriptable> actorScriptables = new Dictionary<string, ActorScriptable>();
	private Dictionary<string, EffectScriptable> effectScriptables = new Dictionary<string, EffectScriptable>();
	private Dictionary<string, MutationScriptable> mutationScriptables = new Dictionary<string, MutationScriptable>();
	private Dictionary<string, SoundScriptable> soundScriptables = new Dictionary<string, SoundScriptable>();
	private Dictionary<string, WeaponScriptable> weaponScriptables = new Dictionary<string, WeaponScriptable>();

	private Dictionary<string, MutationInterface> mutations = new Dictionary<string, MutationInterface>();

	public static GameManager Instance
	{
		get
		{
			return instance;
		}
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			instance = this;
		}

		Resources.LoadAll<ActorScriptable>("");
		var actorScriptableObjects = FindAssetsByType<ActorScriptable>();
		foreach (ActorScriptable actor in actorScriptableObjects)
		{
			actorScriptables.Add(actor.name, actor);
		}

		Resources.LoadAll<EffectScriptable>("");
		var effectScriptableObjects = FindAssetsByType<EffectScriptable>();
		foreach (EffectScriptable effect in effectScriptableObjects)
		{
			effectScriptables.Add(effect.name, effect);
		}

		Resources.LoadAll<MutationScriptable>("");
		var mutationScriptableObjects = FindAssetsByType<MutationScriptable>();
		foreach (MutationScriptable mutation in mutationScriptableObjects)
		{
			mutationScriptables.Add(mutation.name, mutation);
		}

		Resources.LoadAll<SoundScriptable>("");
		var soundScriptableObjects = FindAssetsByType<SoundScriptable>();
		foreach (SoundScriptable sound in soundScriptableObjects)
		{
			soundScriptables.Add(sound.name, sound);
		}

		Resources.LoadAll<WeaponScriptable>("");
		var weaponScriptableObjects = FindAssetsByType<WeaponScriptable>();
		foreach (WeaponScriptable weapon in weaponScriptableObjects)
		{
			weaponScriptables.Add(weapon.name, weapon);
		}

		actorBehaviors.Add(Type.GetType("PlayerAttack"));
		actorBehaviors.Add(Type.GetType("PlayerCamera"));
		actorBehaviors.Add(Type.GetType("PlayerInputs"));
		actorBehaviors.Add(Type.GetType("PlayerInteract"));
		actorBehaviors.Add(Type.GetType("PlayerMove"));
		actorBehaviors.Add(Type.GetType("PlayerSecondaries"));
		actorBehaviors.Add(Type.GetType("EnemyAttack"));
		actorBehaviors.Add(Type.GetType("EnemyMove"));

		SceneManager.LoadScene(SceneDefs.PLAYER_HUD_SCENE, LoadSceneMode.Additive);
	}

	public ActorScriptable getActorScriptable(string id)
	{
		ActorScriptable actor;
		if (actorScriptables.TryGetValue(id, out actor))
		{
			return actor;
		}

		return null;
	}

	public EffectScriptable getEffectScriptable(string id)
	{
		EffectScriptable effect;
		if (effectScriptables.TryGetValue(id, out effect))
		{
			return effect;
		}

		return null;
	}

	public MutationScriptable getMutationScriptable(string id)
	{
		MutationScriptable mutation;
		if (mutationScriptables.TryGetValue(id, out mutation))
		{
			return mutation;
		}

		return null;
	}
	public SoundScriptable getSoundScriptable(string id)
	{
		SoundScriptable sound;
		if (soundScriptables.TryGetValue(id, out sound))
		{
			return sound;
		}

		return null;
	}

	public WeaponScriptable getWeaponScriptable(string id)
	{
		WeaponScriptable weapon;
		if (weaponScriptables.TryGetValue(id, out weapon))
		{
			return weapon;
		}

		return null;
	}

	public static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object
	{
		//AssetBundle bundle;
		var guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
		foreach (var t in guids)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(t);
			var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			if (asset != null)
			{
				yield return asset;
			}
		}
	}
}