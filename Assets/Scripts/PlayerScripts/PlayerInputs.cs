using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs: MonoBehaviour
{
	private ushort inputMatrix = 0x0000;

	private float attackAction;

	private float moveCamAction;
	private Vector2 pointerLoc;

	private Vector2 moveAction;
	private Vector2 oldMoveAction;

	private float interactAction;
	private float lastInteractAction;

	private float throwAction;
	private float lastThrowAction;

	[SerializeField]
	private InputActionReference attack, move, moveCam, interact, pointer, throwVal;

	public float attackInput() { return attackAction; }
	public float moveCamInput() { return attackAction; }
	public Vector2 pointerPos() { return pointerLoc; }
	public Vector2 moveInput() { return moveAction; }
	public float interactInput() { return interactAction; }
	public float throwInput() { return throwAction; }

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

		//mouse inputs
		Vector2 mousePos = pointer.action.ReadValue<Vector2>();
		pointerLoc = Camera.main.ScreenToWorldPoint(mousePos);

		//move inputs
		moveAction = move.action.ReadValue<Vector2>();

		//interact inputs
		lastInteractAction = interactAction;
		interactAction = interact.action.ReadValue<float>();

		//throw inputs
		throwAction = throwVal.action.ReadValue<float>();

		inputMatrix = (Fshort(attackAction)) | (Fshort(interactAction) * 2) | (Fshort(throwAction) * 4);
	}
}