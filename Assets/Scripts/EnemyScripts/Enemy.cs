using UnityEditor;
using UnityEngine;

public class Enemy
{
	float health;

	public Enemy()
	{
		health = 0;
	}

	public float takeDamage(float damage)
	{
		health -= damage;
		health = health < 0 ? 0 : health;
		return health;
	}
}