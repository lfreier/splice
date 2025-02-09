using System.Collections;
using UnityEngine;


public class MLimbHelper : MonoBehaviour
{
	public MLimb baseLimb;


	private void OnTriggerEnter2D(Collider2D collision)
	{
		baseLimb.triggerCollision(collision);
	}
}