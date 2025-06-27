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

	public AudioClip playerDeath;

	//public AudioClip parrySuccess;

	//zombie noises?

	//(x)obstacle hitting actor

	//spore - summonAttempt, (x)summonSuccess, (x)mineArm, (x)mineTrigger, (x)mineExplosion
	//spider - (x) web, (x) webHit, (x) fang, (x) fangShred
	//raptor - (x)pounce, shred
	//limb - (x)extend, (x)retract, (x)blade

	//foldingChair fall
	//menu clicks

	public AudioClip batterySound;
	public AudioClip healthVialSound;
	public AudioClip refillSound;

	public AudioClip obstacleHit;
	public AudioClip obstacleActorHit;

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