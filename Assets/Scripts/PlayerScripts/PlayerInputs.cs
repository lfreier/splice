using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerInputs: MonoBehaviour
{
	private GameManager gameManager = null;

	private float attackAction;
	private float secondaryAttackAction;
	private float lastSecondaryAttackAction;

	private float cycleActiveAction;
	private float lastCycleActiveAction;

	private float moveCamAction;
	private Vector2 pointerLoc;

	private Vector2 moveAction;
	private Vector2 oldMoveAction;

	private float interactAction;
	private float lastInteractAction;

	private float inventoryAction;
	private float lastInventoryAction;

	private float itemAction;
	private float lastItemAction;

	private float throwAction;
	private float lastThrowAction;

	private float walkAction;

	private float pauseAction;
	public bool paused = false;

	private float[] specialActions = new float[MutationDefs.MAX_SLOTS];
	private float[] lastSpecialActions = new float[MutationDefs.MAX_SLOTS];

	[SerializeField]
	private InputActionReference attack, secondaryAttack, move, moveCam, walk, interact, pointer, throwVal, action1, action2, pause, item, inventory, cycleActive;

	public float attackInput() { return attackAction; }
	public float cycleActiveInput() { return cycleActiveAction; }
	public float secondaryAttackInput() { return secondaryAttackAction; }
	public float moveCamInput() { return moveCamAction; }
	public Vector2 pointerPos() { return pointerLoc; }
	public Vector2 moveInput() { return moveAction; }
	public float interactInput() { return interactAction; }
	public float inventoryInput() { return inventoryAction; }
	public float itemInput() { return itemAction; }
	public float pauseInput() { return walkAction; }
	public float throwInput() { return throwAction; }
	public float walkInput() { return walkAction; }

	public float[] actionInputs() { return specialActions; }

	public bool locked = false;
	public bool lockWait = false;

	public static short Fshort(float value)
	{
		return (short)Mathf.FloorToInt(Mathf.Min(value, 1));
	}

	// Update is called once per frame
	void Update()
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}

		if (lockWait && !locked && attack.action.ReadValue<float>() == 0)
		{
			lockWait = false;
		}

		//attack inputs
		if (!lockWait)
		{
			attackAction = attack.action.ReadValue<float>();
		}

		lastSecondaryAttackAction = secondaryAttackAction;
		secondaryAttackAction = secondaryAttack.action.ReadValue<float>();

		//mouse inputs
		if (!paused)
		{
			Vector2 mousePos = pointer.action.ReadValue<Vector2>();
			pointerLoc = Camera.main.ScreenToWorldPoint(mousePos);
		}

		//item inputs
		lastCycleActiveAction = cycleActiveAction;
		cycleActiveAction = cycleActive.action.ReadValue<float>();

		//move inputs
		moveAction = move.action.ReadValue<Vector2>();
 		walkAction = walk.action.ReadValue<float>();

		//interact inputs
		lastInteractAction = interactAction;
		interactAction = interact.action.ReadValue<float>();

		//inventory inputs
		lastInventoryAction = inventoryAction;
		inventoryAction = inventory.action.ReadValue<float>();

		//item inputs
		lastItemAction = itemAction;
		itemAction = item.action.ReadValue<float>();

		//pause input
		pauseAction = pause.action.ReadValue<float>();
		if (pauseAction > 0 && !paused)
		{
			Time.timeScale = 0;

			for (int j = 0; j < SceneManager.sceneCount; j++)
			{
				Scene curr = SceneManager.GetSceneAt(j);
				if (curr.buildIndex == SceneDefs.SCENE_INDEX_MASK[(int)SceneDefs.SCENE.PAUSE])
				{
					SceneManager.UnloadSceneAsync(curr.buildIndex);
					continue;
				}
			}

			SceneManager.LoadSceneAsync((int)SceneDefs.SCENE.PAUSE, LoadSceneMode.Additive);
			gameManager.levelManager.camHandler.stopCam(true);
			paused = true;
		}

		if (inventoryAction != 0 && lastInventoryAction == 0 && !paused)
		{
			gameManager.signalCloseMenusEvent();
			Time.timeScale = 0;
			gameManager.levelManager.camHandler.stopCam(true);
			paused = true;
			gameManager.signalInventoryOpenEvent();
		}

		if (Time.timeScale > 0 && paused == true)
		{
			gameManager.levelManager.camHandler.stopCam(false);
			this.enabled = true;
			paused = false;
		}

		//throw inputs
		throwAction = throwVal.action.ReadValue<float>();

		//camera inputs
		moveCamAction = moveCam.action.ReadValue<float>();

		short i = 0;
		foreach (var action in specialActions)
		{
			lastSpecialActions[i] = action;
			i++;
		}
		//I don't think there's a way around hard coding this but w/e
		specialActions[0] = action1.action.ReadValue<float>();
		specialActions[1] = action2.action.ReadValue<float>();

		/* send out some signals */
		if (specialActions[0] != 0 && lastSpecialActions[0] == 0)
		{
			gameManager.signalPlayerAbilityEvent();
		}
		if (specialActions[0] == 0 && lastSpecialActions[0] != 0)
		{
			gameManager.signalPlayerAbilityReleaseEvent();
		}
		if (specialActions[1] != 0 && lastSpecialActions[1] == 0)
		{
			gameManager.signalPlayerAbilitySecondaryEvent();
		}
		if (specialActions[1] == 0 && lastSpecialActions[1] != 0)
		{
			gameManager.signalPlayerAbilitySecondaryReleaseEvent();
		}
		if (interactAction != 0 && lastInteractAction == 0)
		{
			gameManager.signalPlayerInteractEvent();
		}
		if (interactAction == 0 && lastInteractAction != 0)
		{
			gameManager.signalPlayerInteractReleaseEvent();
		}
		if (itemAction != 0 && lastItemAction == 0)
		{
			gameManager.playerStats.useActiveItem();
		}
		if (cycleActiveAction != 0 && lastCycleActiveAction == 0)
		{
			gameManager.playerStats.cycleActiveItem();
		}
		if (secondaryAttackAction != 0 && lastSecondaryAttackAction == 0)
		{
			gameManager.signalPlayerSecondaryEvent();
		}
	}
}