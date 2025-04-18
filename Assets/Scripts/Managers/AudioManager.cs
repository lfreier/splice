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

	public AudioClip weaponFistSwing;
	public AudioClip weaponFistWallHit;
	public AudioClip weaponFistObstacleHit;
	//public AudioClip weaponFistActorHit;

	public AudioClip weaponMetalSwing;
	public AudioClip weaponMetalWallHit;
	public AudioClip weaponMetalObstacleHit;
	public AudioClip weaponMetalActorHit;
	public AudioClip weaponMetalBreak;

	public AudioClip weaponStunSwing;
	public AudioClip weaponStunHit;
	public AudioClip weaponStunObstacleHit;
	public AudioClip weaponStunActorHit;
	public AudioClip weaponStunBreak;

	public AudioClip weaponScalpelSwing;
	public AudioClip weaponScalpelWallHit;
	public AudioClip weaponScalpelObstacleHit;
	public AudioClip weaponScalpelActorHit;
	public AudioClip weaponScalpelBreak;

	public AudioClip weaponWrenchSwing;
	public AudioClip weaponWrenchWallHit;
	public AudioClip weaponWrenchObstacleHit;
	//public AudioClip weaponWrenchActorHit;
	public AudioClip weaponWrenchBreak;

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