using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager instance = null;

	public GameObject weapPBladeArm;
	public GameObject weapPFist;
	public GameObject weapPRuler;

	public static string CHAR_SCRIP_ID_SCIENTIST = "scientist";

	public static string EFCT_SCRIP_ID_BLEED1 = "bleed1";

	public static string WEAP_SCRIP_ID_BLADEARM = "bladeArm";
	public static string WEAP_SCRIP_ID_FISTS = "fists";
	public static string WEAP_SCRIP_ID_RULER = "ruler";

	private Dictionary<string, ActorScriptable> actorScriptables = new Dictionary<string, ActorScriptable>();
	private Dictionary<string, EffectScriptable> effectScriptables = new Dictionary<string, EffectScriptable>();
	private Dictionary<string, MutationScriptable> mutationScriptables = new Dictionary<string, MutationScriptable>();
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

		var actorScriptableObjects = Resources.FindObjectsOfTypeAll(typeof(ActorScriptable));
		foreach (ActorScriptable actor in actorScriptableObjects)
		{
			actorScriptables.Add(actor.name, actor);
		}

		var effectScriptableObjects = Resources.FindObjectsOfTypeAll(typeof(EffectScriptable));
		foreach (EffectScriptable effect in effectScriptableObjects)
		{
			effectScriptables.Add(effect.name, effect);
		}

		var mutationScriptableObjects = Resources.FindObjectsOfTypeAll(typeof(MutationScriptable));
		foreach (MutationScriptable mutation in mutationScriptableObjects)
		{
			mutationScriptables.Add(mutation.name, mutation);
		}

		var weaponScriptableObjects = Resources.FindObjectsOfTypeAll(typeof(WeaponScriptable));
		foreach (WeaponScriptable weapon in weaponScriptableObjects)
		{
			weaponScriptables.Add(weapon.name, weapon);
		}
	}

	// Update is called once per frame
	void Update()
	{

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

	public WeaponScriptable getWeaponScriptable(string id)
	{
		WeaponScriptable weapon;
		if (weaponScriptables.TryGetValue(id, out weapon))
		{
			return weapon;
		}

		return null;
	}
}