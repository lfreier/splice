using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public Animator anim;

	public void launch(Vector2 target)
	{
		
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//destroy it, unless it hits something that takes damage
	}
}