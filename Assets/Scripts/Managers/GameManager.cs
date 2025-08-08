using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static GameManager;
using static SceneDefs;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	private static GameManager instance = null;

	public GameObject startingSelect;

	public delegate void BasicFunc();

	public delegate void CloseMenusEvent();
	public event CloseMenusEvent closeMenusEvent;

	public delegate void InitHudEvent();
	public event InitHudEvent initHudEvent;

	public delegate void InventoryOpenEvent();
	public event InventoryOpenEvent inventoryOpenEvent;

	public delegate void VolumeChangeEvent(float change);
	public event VolumeChangeEvent volumeChangeEvent;

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

	public delegate void PlayerAbilitySecondaryEvent();
	public event PlayerAbilitySecondaryEvent playerAbilitySecondaryEvent;
	public delegate void PlayerAbilitySecondaryReleaseEvent();
	public event PlayerAbilityReleaseEvent playerAbilitySecondaryReleaseEvent;

	public delegate void PlayerSecondaryEvent();
	public event PlayerSecondaryEvent playerSecondaryEvent;

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
	public PrefabManager prefabManager;

	public LoadingHandler loadingHandler = null;

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
	public SaveManager saveManager;

	public bool isLoaded = false;
	public int currentScene = -1;
	public int currentSaveSlot = 0;

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

	public float musicVolume = 0.5F;
	public float effectsVolume = 0.5F;

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
		await SceneManager.LoadSceneAsync(SCENE_INDEX_MASK[(int)SCENE.LOADING], LoadSceneMode.Additive);

		while (loadingHandler == null){}
		loadingHandler.reloadHUD = true;
		loadingHandler.resetPlayerData = true;
		loadBackgroundScenes();

		playerStats = new PlayerStats();
		playerStats.gameManager = this;

		saveManager = new SaveManager();

		for (int i = 0; i < playerStats.saveStationUses.Length; i ++)
		{
			playerStats.saveStationUses[i] = 1;
		}

		/* This is mainly for debugging - making sure we set the level if we don't load it */
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene curr = SceneManager.GetSceneAt(i);
			if (SceneDefs.isLevelScene((SCENE)SCENE_BUILD_MASK[curr.buildIndex]))
			{
				currentScene = SCENE_BUILD_MASK[curr.buildIndex];
				switch (currentScene)
				{
					case (int)SCENE.LEVEL_START:
						levelManager.lastSavedSpawn = LevelManager.levelSpawnIndex.levelStartSpawn;
						break;
					case (int)SCENE.LEVEL_OFFICE:
						levelManager.lastSavedSpawn = LevelManager.levelSpawnIndex.levelOfficeSpawn;
						break;
					case (int)SCENE.LEVEL_HUB:
						levelManager.lastSavedSpawn = LevelManager.levelSpawnIndex.levelHubSpawn;
						break;
					case (int)SCENE.LEVEL_WAREHOUSE:
						levelManager.lastSavedSpawn = LevelManager.levelSpawnIndex.levelWarehouseSpawn;
						break;
					case (int)SCENE.LEVEL_ARCH:
						levelManager.lastSavedSpawn = LevelManager.levelSpawnIndex.levelArchSpawn;
						break;
					case (int)SCENE.LEVEL_RND:
						levelManager.lastSavedSpawn = LevelManager.levelSpawnIndex.levelRndSpawn;
						break;
					default:
						break;
				}
				levelManager.lastSavedLevelIndex = currentScene;
				await levelManager.startNewLevel(-1, -1);
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

	public void gameOver()
	{
		Time.timeScale = 1;
		CameraHandler camHan = Camera.main.GetComponent<CameraHandler>();
		if (camHan != null)
		{
			camHan.enabled = false;
		}
		Camera.main.transform.position = playerStats.player.transform.position;
		AudioListener audio = Camera.main.GetComponent<AudioListener>();
		if (audio != null)
		{
			audio.enabled = false;
		}
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

	public async void nextLevel(Actor player, SCENE nextSceneIndex, int spawnIndex)
	{
		CameraHandler camHan = Camera.main.GetComponent<CameraHandler>();
		camHan.enabled = false;
		Camera.main.transform.position = player.transform.position;
		AudioListener audio = Camera.main.GetComponent<AudioListener>();
		audio.enabled = false;
		loadingHandler.reloadHUD = true;

		levelManager.saveLevelState(currentScene);
		saveManager.levelSaveData[currentScene] = levelManager.currSaveData;
		

		//TODO: when able to go back to previous levels, this should be the general idea
		//int lastScene = currentScene;
		currentScene = (int)nextSceneIndex;

		await loadingHandler.LoadSceneGroup(new int[] { currentScene }, true, true);

		levelManager.currSaveData = null;

		await levelManager.startNewLevel(spawnIndex, (int)nextSceneIndex);
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

	public void loadPausedScene(Actor playerActor, SCENE toLoad)
	{
		Time.timeScale = 0;

		for (int j = 0; j < SceneManager.sceneCount; j++)
		{
			Scene curr = SceneManager.GetSceneAt(j);
			if (curr.buildIndex == SCENE_INDEX_MASK[(int)toLoad])
			{
				SceneManager.UnloadSceneAsync(curr.buildIndex);
				continue;
			}
		}

		SceneManager.LoadSceneAsync((int)toLoad, LoadSceneMode.Additive);
		playerActor.gameManager.levelManager.camHandler.stopCam(true);

		PlayerInputs inputs = playerActor.GetComponentInChildren<PlayerInputs>();
		if (inputs != null)
		{
			inputs.paused = true;
		}
	}

	public void playSound(AudioSource player, string soundName, float volume)
	{
		if (player == null)
		{
			Debug.Log("Audio player for sound \'" + soundName + " \' is null");
			return;
		}

		AudioClip toPlay;
		if (audioManager.soundHash.TryGetValue(soundName, out toPlay) && toPlay != null)
		{
			player.Stop();
			player.PlayOneShot(toPlay, volume * effectsVolume);
		}
	}

	public void save(Actor player, int levelSaveIndex)
	{
		levelManager.lastSavedLevelIndex = levelSaveIndex;
		playerStats.savePlayerData(player);
		saveManager.savePlayerDataToDisk(currentSaveSlot);

		levelManager.saveLevelState(currentScene);
		saveManager.saveDataToDisk(levelManager.currSaveData, currentSaveSlot);
		saveManager.loadAllData();
	}

	public static void updateRectSize(RectTransform rect, VerticalLayoutGroup vert, int type)
	{
		/* elevator display size*/
		if (type == 0 && rect != null && vert != null)
		{
			// 1366 x 768
			if (Screen.width < 1920)
			{
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 60F);
				vert.spacing = 15;
			}
			// 1920 x 1080
			else if (Screen.width < 2560)
			{
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 72F);
				vert.spacing = 30;
			}
			// 2560 x  x 1440
			else if (Screen.width < 3840)
			{
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 144F);
				vert.spacing = 0;
			}
			// 3840 x 2160
			else
			{
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 216F);
				vert.spacing = 0;
			}
		}
	}

	public static void updateCellFontSize(TextMeshProUGUI text, int type)
	{
		//this is dumb because my canvas is set to auto-sizing, but w/e. it works
		if (text == null)
		{
			return;
		}

		/* hud cell text size*/
		if (type == 0)
		{
			// 1366 x 768
			if (Screen.width < 1920)
			{
				text.fontSize = 40.6F;
			}
			// 1920 x 1080
			else if (Screen.width < 2560)
			{
				text.fontSize = 48.3047F;
			}
			// 2560 x  x 1440
			else if (Screen.width < 3840)
			{
				text.fontSize = 48.3785F;
			}
			// 3840 x 2160
			else
			{
				text.fontSize = 48.45235F;
			}
		}
		/* station menu font */
		else if (type == 1)
		{
			// 1366 x 768
			if (Screen.width < 1920)
			{
				text.fontSize = 39.6F;
			}
			// 1920 x 1080
			else if (Screen.width < 2560)
			{
				text.fontSize = 55.65F;
			}
			// 2560 x  x 1440
			else if (Screen.width < 3840)
			{
				text.fontSize = 74F;
			}
			// 3840 x 2160
			else
			{
				text.fontSize = 111.35F;
			}
		}
		/* save slots menu font */
		else if (type == 2)
		{
			// 1366 x 768
			if (Screen.width < 1920)
			{
				text.fontSize = 38F;
			}
			// 1920 x 1080
			else if (Screen.width < 2560)
			{
				text.fontSize = 48F;
			}
			// 2560 x  x 1440
			else if (Screen.width < 3840)
			{
				text.fontSize = 58F;
			}
			// 3840 x 2160
			else
			{
				text.fontSize = 96F;
			}
		}
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

	public void signalVolumeChangeEvent(float change)
	{
		volumeChangeEvent?.Invoke(change);
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

	public void signalPlayerAbilitySecondaryReleaseEvent()
	{
		playerAbilitySecondaryReleaseEvent?.Invoke();
	}

	public void signalPlayerAbilitySecondaryEvent()
	{
		playerAbilitySecondaryEvent?.Invoke();
	}

	public void signalPlayerSecondaryEvent()
	{
		playerSecondaryEvent?.Invoke();
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