using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Controller2D))]
public class EnemyMovement : MonoBehaviour
{
	enum detectMode
	{
		hidden = 0,
		suspicious = 1,
		hostile = 2
	};

	detectMode detected;

	public Enemy enemyInst;

	float currentSpeed;

	public Rigidbody2D playerBody;

	private Vector2 destination;

	private Vector2 moveInput;
	private Vector2 oldMoveInput;

	private float idleTimer;

	public Controller2D controller;
	public Rigidbody2D enemyBody;

	public  ActorScriptable enemyScriptable;
	private float acceleration;
	private float deceleration;
	private float moveSpeed;
	private float maxSpeed;

	void Start()
	{
		enemyInst = new Enemy();

		detected = detectMode.hostile;
		//initialize the NPC AI
		idleTimer = Random.Range(1F, 5F);
	}

	void Update()
	{

		if (detected == detectMode.hostile)
		{
			if (idleTimer > 0)
			{
				//when idle just ended
				if ((idleTimer -= Time.deltaTime) <= 0)
				{
					Vector2 aimDir = playerBody.position - enemyBody.position;
					float aimAngle = (Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg) - 90F;
					float difference = Vector2.Distance(playerBody.position, enemyBody.position);
					enemyBody.rotation = aimAngle;
					destination = Vector2.MoveTowards(enemyBody.position, playerBody.position, Random.Range(0.5F, difference));
					idleTimer = 0;
				}
			}

			if (idleTimer <= 0)
			{
				if (Vector2.Distance(enemyBody.position, destination) < 0.1F)
				{
					//wait for a bit before moving again
					idleTimer = 1F;
					moveInput = new Vector2(0, 0);
				}
				else
				{
					Vector2 aimDir = destination - enemyBody.position;
					float aimAngle = (Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg) - 90F;
					moveInput = Vector2.MoveTowards(enemyBody.position, destination, 1F) - enemyBody.position;
				}
			}
		}
	}

    void FixedUpdate()
	{
		// move
		if (moveInput.magnitude > 0)
		{
			oldMoveInput = moveInput;
			currentSpeed += acceleration * moveSpeed;
		}
		else
		{
			currentSpeed -= deceleration * moveSpeed;
		}

		currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

		controller.Move(new Vector3(oldMoveInput.x * currentSpeed * Time.deltaTime, oldMoveInput.y * currentSpeed * Time.deltaTime));

	}
}
