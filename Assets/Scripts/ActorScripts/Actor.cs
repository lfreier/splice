using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Actor class:
 * Contains data and data interface for basic actors. 
 * Will be used as an interface for interacting with the Actor's 'body'
 */

public class Actor : MonoBehaviour
{
	public ActorScriptable actorData;
	public Controller2D controller;
	public Rigidbody2D actorBody;

	private float health;

	public Actor(ActorScriptable actorScriptable)
	{
		actorData = actorScriptable;
		health = actorData.health;
	}

	public float takeDamage(float damage)
	{
		health -= damage;
		health = health < 0 ? 0 : health;
		return health;
	}

	public void Move(Vector3 moveVector)
	{
		controller.Move(moveVector);
	}

}