using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputs: MonoBehaviour
{
	private GameManager gameManager = null;

	private float attackAction;
	private float secondaryAttackAction;

	private float moveCamAction;
	private Vector2 pointerLoc;

	private Vector2 moveAction;
	private Vector2 oldMoveAction;

	private float interactAction;
	private float lastInteractAction;

	private float throwAction;
	private float lastThrowAction;

	private float walkAction;

	private float pauseAction;
	public bool paused = false;

	public CameraHandler camHandler;

	private float[] specialActions = new float[MutationDefs.MAX_SLOTS];
	private float[] lastSpecialActions = new float[MutationDefs.MAX_SLOTS];

	[SerializeField]
	private InputActionReference attack, secondaryAttack, move, moveCam, walk, interact, pointer, throwVal, action1, action2, pause;

	public float attackInput() { return attackAction; }
	public float secondaryAttackInput() { return secondaryAttackAction; }
	public float moveCamInput() { return moveCamAction; }
	public Vector2 pointerPos() { return pointerLoc; }
	public Vector2 moveInput() { return moveAction; }
	public float interactInput() { return interactAction; }
	public float pauseInput() { return walkAction; }
	public float throwInput() { return throwAction; }
	public float walkInput() { return walkAction; }

	public float[] actionInputs() { return specialActions; }

	public static short Fshort(float value)
	{
		return (short)Mathf.FloorToInt(Mathf.Min(value, 1));
	}

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}

		//attack inputs
		attackAction = attack.action.ReadValue<float>();
		secondaryAttackAction = secondaryAttack.action.ReadValue<float>();

		//mouse inputs
		if (!paused)
		{
			Vector2 mousePos = pointer.action.ReadValue<Vector2>();
			pointerLoc = Camera.main.ScreenToWorldPoint(mousePos);
		}

		//move inputs
		moveAction = move.action.ReadValue<Vector2>();
 		walkAction = walk.action.ReadValue<float>();

		//interact inputs
		lastInteractAction = interactAction;
		interactAction = interact.action.ReadValue<float>();

		//pause input
		pauseAction = pause.action.ReadValue<float>();
		if (pauseAction > 0 && !paused)
		{
			Time.timeScale = 0;

			for (int j = 0; j < SceneManager.sceneCount; j++)
			{
				Scene curr = SceneManager.GetSceneAt(j);
				if (curr.buildIndex == SceneDefs.PAUSE_SCENE)
				{
					SceneManager.UnloadSceneAsync(curr.buildIndex);
					continue;
				}
			}

			SceneManager.LoadSceneAsync(SceneDefs.PAUSE_SCENE, LoadSceneMode.Additive);
			camHandler.stopCam(true);
			paused = true;
		}
		if (Time.timeScale > 0 && paused == true)
		{
			camHandler.stopCam(false);
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
			Debug.Log("Sending ability event");
			gameManager.signalPlayerAbilityEvent();
		}
		if (specialActions[0] == 0 && lastSpecialActions[0] != 0)
		{
			gameManager.signalPlayerAbilityReleaseEvent();
		}
		if (interactAction != 0 && lastInteractAction == 0)
		{
			gameManager.signalPlayerInteractEvent();
		}
		if (interactAction == 0 && lastInteractAction != 0)
		{
			gameManager.signalPlayerInteractReleaseEvent();
		}
	}
}