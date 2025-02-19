using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;
using static SceneDefs;

public class GameManager : MonoBehaviour
{
	private static GameManager instance = null;

	public GameObject startingSelect;

	public delegate void CloseMenusEvent();
	public event CloseMenusEvent closeMenusEvent;

	public delegate void InitHudEvent();
	public event InitHudEvent initHudEvent;

	public delegate void InventoryOpenEvent();
	public event InventoryOpenEvent inventoryOpenEvent;

	public delegate void MuteEvent();
	public event MuteEvent muteEvent;

	public delegate void MovementLockedEvent();
	public event MovementLockedEvent movementLockedEvent;

	public delegate void MovementUnlockedEvent();
	public event MovementUnlockedEvent movementUnlockedEvent;

	public delegate void PowerChangedEvent(bool powerOn);
	public event PowerChangedEvent powerChangedEvent;

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

	public delegate void StartMusicEvent(MusicScriptable music);
	public event StartMusicEvent startMusicEvent;

	public delegate void UpdateCellCount(int count);
	public event UpdateCellCount updateCellCount;

	public delegate void UpdateHealthEvent(float newHealth);
	public event UpdateHealthEvent updateHealthEvent;

	public delegate void UpdateKeycardCount(int newCount, PickupDefs.keycardType type);
	public event UpdateKeycardCount updateKeycardCount;

	public delegate void UpdateShieldEvent(float shield);
	public event UpdateShieldEvent updateShieldEvent;

	public AudioManager audioManager;
	public EffectManager effectManager;
	public LevelManager levelManager;

	public LoadingHandler loadingHandler = null;

	public GameObject[] weaponPrefabs;

	public GameObject mutPBeast;
	public GameObject mutPBladeWing;
	public GameObject mutPLimb;

	public GameObject weapPBladeArm;
	public GameObject weapPFist;

	public LayerMask actorLayers;
	public LayerMask collisionLayer;
	public LayerMask findWeaponLayers;
	public LayerMask lineOfSightLayers;
	public LayerMask excludeCollisionLayers;
	public LayerMask soundLayer;
	public LayerMask unwalkableLayers;

	public PickupMaxCounts pickupMaxScriptable;
	[HideInInspector]
	public int[] maxPickups = new int[PickupDefs.MAX_USABLE_ITEM_TYPE + 1];

	public PlayerStats playerStats;

	public bool isLoaded = false;
	public int currentScene = -1;

	public static string ACTOR_LAYER					= "Actor";
	public static string DAMAGE_LAYER					= "Damage";
	public static string OBJECT_LAYER					= "Object";
	public static string OBJECT_MID_LAYER				= "ObjectMid";
	public static string OBJECT_HIGH_LAYER				= "ObjectHigh";
	public static string OBJECT_EXCLUDE_ACTOR_LAYER		= "ObjectExcludeActor";
	public static string THROWN_WEAPON_LAYER			= "ThrownWeapon";
	public static string WALL_COLLISION_LAYER			= "WallCollision";
	public static string UI_LAYER						= "UI";

	public static string CHAR_SCRIP_ID_SCIENTIST = "scientist";

	public static string WEAP_SCRIP_ID_BLADEARM = "bladeArm";
	public static string WEAP_SCRIP_ID_FISTS = "fists";
	public static string WEAP_SCRIP_ID_RULER = "ruler";

	public static Color COLOR_BLUE		= new Color(0.1F, 0.1F, 0.4F, 1F);
	public static Color COLOR_GREEN		= new Color(0.15F, 0.4f, 0, 1F);
	public static Color COLOR_RED		= new Color(0.4F, 0.1F, 0.1F, 1F);
	public static Color COLOR_YELLOW	= new Color(0.54F, 0.54F, 0, 1F);
	public static Color COLOR_VIOLET	= new Color(0.35F, 0F, 0.45F, 1F);
	public static Color COLOR_IFRAME	= new Color(0.9F, 0.3F, 0.3F, 1F);

	public List<Type> actorBehaviors = new List<Type>();

	private Dictionary<string, ActorScriptable> actorScriptables = new Dictionary<string, ActorScriptable>();
	private Dictionary<string, EffectScriptable> effectScriptables = new Dictionary<string, EffectScriptable>();
	private Dictionary<string, MutationScriptable> mutationScriptables = new Dictionary<string, MutationScriptable>();
	private Dictionary<string, SoundScriptable> soundScriptables = new Dictionary<string, SoundScriptable>();
	private Dictionary<string, WeaponScriptable> weaponScriptables = new Dictionary<string, WeaponScriptable>();

	private Dictionary<string, MutationInterface> mutations = new Dictionary<string, MutationInterface>();

	private float hitstopLength;

	public static GameManager Instance
	{
		get
		{
			return instance;
		}
	}

	private async void Awake()
	{
		currentScene = -1;
		if (instance != null && instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			instance = this;
		}

		/* TODO: this sucks */
		actorBehaviors.Add(Type.GetType("PlayerAttack"));
		actorBehaviors.Add(Type.GetType("PlayerCamera"));
		actorBehaviors.Add(Type.GetType("PlayerInputs"));
		actorBehaviors.Add(Type.GetType("PlayerInteract"));
		actorBehaviors.Add(Type.GetType("PlayerMove"));
		actorBehaviors.Add(Type.GetType("PlayerSecondaries"));
		actorBehaviors.Add(Type.GetType("EnemyAttack"));
		actorBehaviors.Add(Type.GetType("EnemyMove"));

		maxPickups = new int[PickupDefs.MAX_USABLE_ITEM_TYPE + 1];
		maxPickups[(int)PickupDefs.usableType.HEALTH_VIAL] = pickupMaxScriptable.healthVialMax;
		maxPickups[(int)PickupDefs.usableType.BATTERY] = pickupMaxScriptable.batteryMax;
		maxPickups[(int)PickupDefs.usableType.REFILL] = pickupMaxScriptable.refillMax;

		/* load necessary background scenes now */
		await SceneManager.LoadSceneAsync((int)SCENE.LOADING, LoadSceneMode.Additive);

		while (loadingHandler == null){}
		loadingHandler.reloadHUD = true;
		loadingHandler.resetPlayerData = true;
		loadBackgroundScenes();

		playerStats = new PlayerStats();
		playerStats.gameManager = this;

		/* This is mainly for debugging - making sure we set the level if we don't load it */
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene curr = SceneManager.GetSceneAt(i);
			if (SceneDefs.isLevelScene(curr.buildIndex))
			{
				currentScene = curr.buildIndex;
				switch (currentScene)
				{
					case (int)SCENE.LEVEL_START:
						levelManager.lastSavedSpawn = (int)LevelManager.levelSpawnIndex.levelStartSpawn;
						break;
					case (int)SCENE.LEVEL_OFFICE:
						levelManager.lastSavedSpawn = (int)LevelManager.levelSpawnIndex.levelOfficeSpawn;
						break;
					case (int)SCENE.LEVEL_HUB:
						levelManager.lastSavedSpawn = (int)LevelManager.levelSpawnIndex.levelHubSpawn;
						break;
					case (int)SCENE.LEVEL_WAREHOUSE:
						levelManager.lastSavedSpawn = (int)LevelManager.levelSpawnIndex.levelWarehouseSpawn;
						break;
					case (int)SCENE.LEVEL_ARCH:
						levelManager.lastSavedSpawn = (int)LevelManager.levelSpawnIndex.levelArchSpawn;
						break;
					default:
						break;
				}
				levelManager.lastSavedLevelIndex = currentScene;
				await levelManager.startNewLevel(-1);
				break;
			}
		}

		isLoaded = true;
		hitstopLength = 0;
	}

	private void FixedUpdate()
	{
		if (hitstopLength > 0)
		{
			hitstopLength -= Time.unscaledDeltaTime;
			if (hitstopLength <= 0)
			{
				hitstopLength = 0;
				Time.timeScale = 1;
			}
		}
	}

	public void gameOver(Actor player)
	{
		CameraHandler camHan = Camera.main.GetComponent<CameraHandler>();
		camHan.enabled = false;
		Camera.main.transform.position = player.transform.position;
		AudioListener audio = Camera.main.GetComponent<AudioListener>();
		audio.enabled = false;
		SceneManager.LoadScene((int)SCENE.GAME_OVER, LoadSceneMode.Additive);
	}

	public void gameWin(Actor player)
	{
		CameraHandler camHan = Camera.main.GetComponent<CameraHandler>();
		camHan.enabled = false;
		Camera.main.transform.position = player.transform.position;
		AudioListener audio = Camera.main.GetComponent<AudioListener>();
		audio.enabled = false;
		Time.timeScale = 0;
		currentScene = (int)SCENE.WIN;
		SceneManager.LoadScene((int)SCENE.WIN, LoadSceneMode.Additive);
	}

	public void hitstop(float length, float speed)
	{
		//TODO: remove if committing to not using hitstop
		/*
		return;
		hitstopLength = length;
		Time.timeScale = speed;
		*/
	}

	public async void nextLevel(Actor player, int nextSceneIndex, int spawnIndex)
	{
		CameraHandler camHan = Camera.main.GetComponent<CameraHandler>();
		camHan.enabled = false;
		Camera.main.transform.position = player.transform.position;
		AudioListener audio = Camera.main.GetComponent<AudioListener>();
		audio.enabled = false;
		loadingHandler.reloadHUD = true;

		//TODO: when able to go back to previous levels, this should be the general idea
		//int lastScene = currentScene;
		//await levelManager.saveLevelState(lastScene);
		currentScene = nextSceneIndex;

		await loadingHandler.LoadSceneGroup(new int[] { currentScene }, true, true);

		levelManager.lastSavedSpawn = spawnIndex;
		levelManager.lastSavedLevelIndex = currentScene;
		levelManager.lastSavedAtStation = false;
		await levelManager.startNewLevel(spawnIndex);
	}

	public async void loadBackgroundScenes()
	{
		List<int> bgScenes = new List<int>(BACKGROUND_SCENES.Length);

		int i = 0, j = 0;
		for (i = 0; i < BACKGROUND_SCENES.Length; i++)
		{
			for (j = 0; j < SceneManager.sceneCount; j++)
			{
				if (BACKGROUND_SCENES[i] == SceneManager.GetSceneAt(j).buildIndex)
				{
					j = -1;
					break;
				}
			}
			if (j >= 0)
			{
				bgScenes.Add(BACKGROUND_SCENES[i]);
			}
		}
		await loadingHandler.LoadSceneGroup(bgScenes.ToArray(), false, false);
	}

	public async void resetLevel()
	{
		playerStats.resetCounts();
		await levelManager.startNewLevel(levelManager.lastSavedSpawn);
	}

	public async void save(Actor player)
	{
		playerStats.savePlayerData(player);
		await levelManager.saveLevelState(currentScene);
		levelManager.lastSavedLevelIndex = currentScene;
	}

	public void signalCloseMenusEvent()
	{
		closeMenusEvent?.Invoke();
	}

	public void signalInitHudEvent()
	{
		initHudEvent?.Invoke();
	}

	public void signalInventoryOpenEvent()
	{
		inventoryOpenEvent?.Invoke();
	}

	public void signalMovementLocked()
	{
		movementLockedEvent?.Invoke();
	}

	public void signalMovementUnlocked()
	{
		movementUnlockedEvent?.Invoke();
	}

	public void signalMuteEvent()
	{
		muteEvent?.Invoke();
	}
	public void signalPowerChangedEvent(bool powerOn)
	{
		powerChangedEvent?.Invoke(powerOn);
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

	public void signalRotationLocked()
	{
		rotationLockedEvent?.Invoke();
	}

	public void signalRotationUnlocked()
	{
		rotationUnlockedEvent?.Invoke();
	}

	public void signalStartMusicEvent(MusicScriptable music)
	{
		startMusicEvent?.Invoke(music);
	}


	public void signalUpdateCellCount(int count)
	{
		updateCellCount?.Invoke(count);
	}

	public void signalUpdateHealthEvent(float newHealth)
	{
		updateHealthEvent?.Invoke(newHealth);
	}

	public void signalUpdateKeycardCount(int newCount, PickupDefs.keycardType type)
	{
		updateKeycardCount?.Invoke(newCount, type);
	}

	public void signalUpdateShieldEvent(float shield)
	{
		updateShieldEvent?.Invoke(shield);
	}
}