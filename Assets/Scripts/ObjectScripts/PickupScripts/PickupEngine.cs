using System.Collections;
using UnityEngine;

public class PickupEngine : MonoBehaviour
{
	public SpriteRenderer pickupGlow;
	private bool glowEnabled = true;
	public SpriteRenderer pickupHighlight;
	private Color glowColor;

	public bool onlyHighight = false;

	public float glowTimer = 0.7F;
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
		if (pickupGlow != null)
		{
			glowColor = pickupGlow.color;
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (onlyHighight || pickupGlow == null)
		{
			return;
		}

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
		glowEnabled = false;
		if (pickupGlow != null)
		{
			pickupGlow.enabled = false;
		}
	}

	public void enableHighlight()
	{
		if (pickupGlow != null)
		{
			pickupGlow.enabled = false;
		}
		pickupHighlight.enabled = true;
	}

	public void disableHighlight()
	{
		if (glowEnabled && pickupGlow != null)
		{
			pickupGlow.enabled = true;
		}
		pickupHighlight.enabled = false;
	}
}