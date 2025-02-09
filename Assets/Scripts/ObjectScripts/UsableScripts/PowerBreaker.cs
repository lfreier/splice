using System.Collections;
using UnityEngine;

public class PowerBreaker : MonoBehaviour, UsableInterface
{
	public SpriteRenderer switchSprite;

	public void use(Actor user)
	{
		GameManager gm = GameManager.Instance;
		if (gm != null)
		{
			if (gm.levelManager != null)
			{
				if (switchSprite != null && !switchSprite.flipX)
				{
					switchSprite.flipX = true;
				}
				gm.signalPowerChangedEvent(false);
				gm.levelManager.setPower(false);
			}
		}
	}
}