using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	private static AudioManager instance = null;

	public GameObject soundPrefab;

	public AudioListener listener;

	public Dictionary<string, AudioClip> soundHash = new Dictionary<string, AudioClip>();

	public AudioClip[] clipsRef;

	public AudioClip footstepCarpet;

	public AudioClip heartPump;

	public AudioClip parrySuccess;

	public AudioClip playerDeath;
	
	//zombie attack
	//zombie noises?

	//obstacle hitting actor

	//battery charge

	//health vial

	//using computer

	//spore - summonAttempt, summonSuccess, clickTick, mineArm, mineTrigger, mineExplosion
	//spider - web, fang, fangShred
	//raptor - pounce, shred
	//limb - extend, retract, objectHit

	//foldingChair fall

	public AudioClip sciDeath1;
	public AudioClip sciDeath2;
	public AudioClip sciDeath3;
	public AudioClip sciDeath4;

	public static AudioManager Instance
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

		foreach (AudioClip clip in clipsRef)
		{
			soundHash.Add(clip.name, clip);
		}
	}

}