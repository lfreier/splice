using System.Collections;
using UnityEngine;

public class PickupEngine : MonoBehaviour
{
	public SpriteRenderer pickupGlow;
	private Color glowColor;

	public float glowTimer = 1F;
	public float glowIncTimer = 0.1F;
	public float glowChange = 0.1F;

	private float timer;
	private float incTimer;

	/* 0   - increase
	 * 1   - decrease
	 * -1  - stay then decrease
	 */
	private int currState = 1;

	// Use this for initialization
	void Start()
	{
		timer = glowTimer;
		incTimer = glowIncTimer;
		glowColor = pickupGlow.color;
	}

	// Update is called once per frame
	void Update()
	{
		if (timer <= 0)
		{
			incTimer = glowIncTimer;
			switch (currState)
			{
				case 0:
					timer = glowTimer / 2;
					currState = -1;
					break;
				case 1:
					timer = glowTimer;
					currState = 0;
					break;
				case -1:
					timer = glowTimer;
					currState = 1;
					break;
				default:
					break;
			}
		}
		
		if (incTimer <= 0)
		{
			incTimer = glowIncTimer;

			switch (currState)
			{
				case 0:
					glowColor.a += glowChange;
					pickupGlow.color = glowColor;
					break;
				case 1:
					glowColor.a -= glowChange;
					pickupGlow.color = glowColor;
					break;
				case -1:
				case -2:
				default:
					break;
			}
		}

		timer -= Time.deltaTime;
		incTimer -= Time.deltaTime;
	}

	public void disableGlow()
	{
		pickupGlow.enabled = false;
	}
}