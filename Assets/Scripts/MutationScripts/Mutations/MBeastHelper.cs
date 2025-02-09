using System.Collections;
using UnityEngine;


public class MBeastHelper : MonoBehaviour
{
	public MBeast baseBeast;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		baseBeast.triggerCollision(collision);
	}
}