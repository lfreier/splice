using System.Collections;
using UnityEngine;

public class KnockbackTimer : MonoBehaviour
{
	public float timer;
	public Actor attachedActor;

	public void init(float resist)
	{
		attachedActor = GetComponent<Actor>();
		if (attachedActor == null )
		{
			return;
		}

		if (resist <= 0.2)
		{
			timer = 75F;
		}
		else if (resist <= 0.4)
		{
			timer = 0.4F;
		}
		else if (resist <= 0.6)
		{
			timer = 0.25F;
		}
		else if (resist <= 0.8)
		{
			timer = 0.1F;
		}
		else
		{
			timer = 0.05F;
		}
		attachedActor.isKnockedBack = true;
	}

	void FixedUpdate()
	{
		if (timer > 0)
		{
			timer -= Time.deltaTime;
			if (timer <= 0)
			{
				timer = 0;
				attachedActor.isKnockedBack = false;
				Destroy(this);
			}
		}
	}
}