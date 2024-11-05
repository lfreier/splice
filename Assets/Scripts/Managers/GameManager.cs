using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	private static GameManager instance = null;

	public GameObject startingSelect;

	public delegate void PlayerInteractEvent();
	public event PlayerInteractEvent playerInteractEvent;
	public delegate void PlayerInteractReleaseEvent();
	public event PlayerInteractReleaseEvent playerInteractReleaseEvent;

	public delegate void PlayerAbilityEvent();
	public event PlayerAbilityEvent playerAbilityEvent;
	public delegate void PlayerAbilityReleaseEvent();
	public event PlayerAbilityReleaseEvent playerAbilityReleaseEvent;

	public delegate void RotationLockedEvent();
	public event RotationLockedEvent rotationLockedEvent;
	public delegate void RotationUnlockedEvent();
	public event RotationUnlockedEvent rotationUnlockedEvent;

	public AudioManager audioManager;
	public EffectManager effectManager;
	public LevelManager levelManager;

	public Camera mainCam;

	public bool startWithSelect = false;

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
	public static string OBJECT_EXCLUDE_ACTOR_LAYER = "ObjectExcludeActor";
	public static string COLLISION_ACTOR_LAYER = "CollisionActor";
	public static string UI_LAYER = "UI";

	public static string CHAR_SCRIP_ID_SCIENTIST = "scientist";

	public static string WEAP_SCRIP_ID_BLADEARM = "bladeArm";
	public static string WEAP_SCRIP_ID_FISTS = "fists";
	public static string WEAP_SCRIP_ID_RULER = "ruler";

	public static Color COLOR_BLUE = new Color(0.1F, 0.1F, 0.4F, 1F);
	public static Color COLOR_GREEN = new Color(0.15F, 0.4f, 0, 1F);
	public static Color COLOR_RED = new Color(0.4F, 0.1F, 0.1F, 1F);
	public static Color COLOR_YELLOW = new Color(0.6F, 0.6f, 0, 1F);
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

	private void Update()
	{
		if (audioManager == null)
			audioManager = AudioManager.Instance;
		if (effectManager == null)
			effectManager = EffectManager.Instance;
	}

	public void gameOver(Actor player)
	{
		CameraHandler camHan = mainCam.GetComponent<CameraHandler>();
		camHan.enabled = false;
		mainCam.transform.position = player.transform.position;
		AudioListener audio = mainCam.GetComponent<AudioListener>();
		audio.enabled = false;
		player.gameManager.startWithSelect = false;
		SceneManager.LoadScene(SceneDefs.GAME_OVER_SCENE, LoadSceneMode.Additive);

		//player.actorAudioSource.PlayOneShot(audioManager.playerDeath);
		//while (player.actorAudioSource.isPlaying)
		//{

		//}
		//audio.enabled = false;
	}
	public void nextLevel(Actor player)
	{
		CameraHandler camHan = mainCam.GetComponent<CameraHandler>();
		camHan.enabled = false;
		mainCam.transform.position = player.transform.position;
		player.gameManager.startWithSelect = true;
		AudioListener audio = mainCam.GetComponent<AudioListener>();
		audio.enabled = false;
		Time.timeScale = 0;
		SceneManager.LoadScene(SceneDefs.NEXT_LEVEL_SCENE, LoadSceneMode.Additive);
	}

	public void startMutationSelect()
	{
		if (startingSelect == null)
		{
			Debug.Log("No starting select because null");
			return;
		}
		foreach (MutationSelect select in startingSelect.GetComponents<MutationSelect>())
		{
			Debug.Log("Starting with mutation selection");
			select.gameObject.SetActive(true);
		}
	}

	public void signalRotationLocked()
	{
		rotationLockedEvent?.Invoke();
	}

	public void signalRotationUnlocked()
	{
		rotationUnlockedEvent?.Invoke();
	}

	public void signalPlayerInteractEvent()
	{
		playerInteractEvent?.Invoke();
	}

	public void signalPlayerInteractReleaseEvent()
	{
		playerInteractReleaseEvent?.Invoke();
	}

	public void signalPlayerAbilityEvent()
	{
		playerAbilityEvent?.Invoke();
	}

	public void signalPlayerAbilityReleaseEvent()
	{
		playerAbilityReleaseEvent?.Invoke();
	}
}