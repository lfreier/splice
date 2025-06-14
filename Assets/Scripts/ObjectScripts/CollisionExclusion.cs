using System.Collections;
using UnityEngine;

public class CollisionExclusion : MonoBehaviour
{
	public float timer;

	private Collider2D[] attachedColliders1;
	private Collider2D[] attachedColliders2;
	private bool started = false;

	// Update is called once per frame
	void FixedUpdate()
	{
		if (started == true)
		{
			timer -= Time.deltaTime;

			if (timer <= 0)
			{
				foreach (Collider2D coll1 in attachedColliders1)
				{
					foreach (Collider2D coll2 in attachedColliders2)
					{
						if (coll1 != null && coll2 != null)
						{
							Physics2D.IgnoreCollision(coll1, coll2, false);
						}
					}
				}

				Destroy(this);
			}
		}
	}

	public void init(float timer, Collider2D[] colliders1, Collider2D[] colliders2)
	{
		this.timer = timer;
		foreach (Collider2D coll1 in colliders1)
		{
			foreach (Collider2D coll2 in colliders2)
			{
				Physics2D.IgnoreCollision(coll1, coll2, true);
			}
		}
		attachedColliders1 = colliders1;
		attachedColliders2 = colliders2;
		started = true;
	}
}