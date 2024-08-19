using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs: MonoBehaviour
{
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

	private float[] specialActions = new float[MutationDefs.MAX_SLOTS];
	private float[] lastSpecialActions = new float[MutationDefs.MAX_SLOTS];

	[SerializeField]
	private InputActionReference attack, secondaryAttack, move, moveCam, walk, interact, pointer, throwVal, action1, action2;

	public float attackInput() { return attackAction; }
	public float secondaryAttackInput() { return secondaryAttackAction; }
	public float moveCamInput() { return moveCamAction; }
	public Vector2 pointerPos() { return pointerLoc; }
	public Vector2 moveInput() { return moveAction; }
	public float interactInput() { return interactAction; }
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
		//attack inputs
		attackAction = attack.action.ReadValue<float>();
		secondaryAttackAction = secondaryAttack.action.ReadValue<float>();

		//mouse inputs
		Vector2 mousePos = pointer.action.ReadValue<Vector2>();
		pointerLoc = Camera.main.ScreenToWorldPoint(mousePos);

		//move inputs
		moveAction = move.action.ReadValue<Vector2>();
		walkAction = walk.action.ReadValue<float>();

		//interact inputs
		lastInteractAction = interactAction;
		interactAction = interact.action.ReadValue<float>();

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
	}
}