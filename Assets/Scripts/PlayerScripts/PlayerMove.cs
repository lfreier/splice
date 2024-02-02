using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerMove : MonoBehaviour
{

	private float currentSpeed;

	private ActorScriptable playerData;

	public Actor player;

	public LayerMask enemyLayer;
	public LayerMask weaponLayer;

	private Vector2 moveInput;
	private Vector2 oldMoveInput;

	public PlayerInputs inputs;


	private void Start()
	{
		playerData = player.actorData;
		currentSpeed = 0;
	}

	private void Update()
	{
		moveInput = inputs.moveInput();
	}

	private void FixedUpdate()
	{
		// move
		if (moveInput.magnitude > 0)
		{
			oldMoveInput = moveInput;
			currentSpeed += playerData.acceleration * playerData.moveSpeed;
		}
		else
		{
			currentSpeed -= playerData.deceleration * playerData.moveSpeed;
		}

		currentSpeed = Mathf.Clamp(currentSpeed, 0, playerData.maxSpeed);
		player.Move(new Vector3(oldMoveInput.x * currentSpeed * Time.deltaTime, oldMoveInput.y * currentSpeed * Time.deltaTime));
	}
}