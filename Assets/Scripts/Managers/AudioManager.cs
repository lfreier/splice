using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	private static AudioManager instance = null;

	private GameManager gameManager;

	public AudioListener listener;

	public AudioClip cellCollectPop;

	public AudioClip actorDeath1;
	public AudioClip actorDeath2;
	public AudioClip actorDeath3;

	public AudioClip doorClose;
	public AudioClip doorOpen;
	public AudioClip doorUnlock;

	public AudioClip footstep;

	public AudioClip heartPump;

	public AudioClip parry;

	public AudioClip playerDeath;

	public AudioClip wallMetal;

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
	}

}