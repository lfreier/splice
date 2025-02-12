using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
	public enum doorType
	{
		AUTO = 0,
		MANUAL = 1,
		REMOTE = 2
	}
	public doorType _doorType = doorType.AUTO;

	private bool open;
	public bool locked;
	public SpriteRenderer lockSprite;

	//private bool stuck = false;

	public PickupDefs.keycardType lockType;

	public Animator doorAnimator;

	private Color keycardColor;

	public float detectSize = 3F;

	private GameManager gameManager;

	private void Start()
	{
		/* Set color */
		keycardColor = PickupDefs.getKeycardColor(lockType);

		open = false;
		if (locked)
		{
			doorLock();
		}
		else
		{
			doorUnlock();
		}

		gameManager = GameManager.Instance;
		if (gameManager != null && _doorType == doorType.AUTO)
		{
			gameManager.powerChangedEvent += handlePowerOutage;
		}
	}

	private void OnDestroy()
	{
		if (gameManager != null && null != (gameManager = GameManager.Instance))
		{
			gameManager.powerChangedEvent -= handlePowerOutage;
		}
	}

	private void FixedUpdate()
	{
		if (_doorType == doorType.AUTO)
		{
			Collider2D[] hit = Physics2D.OverlapBoxAll(gameObject.transform.position, new Vector2(detectSize, detectSize), 0, gameManager.actorLayers);
			if (!open && !locked && hit.Length > 0)
			{
				doorOpen();
			}

			if (open && hit.Length <= 0)
			{
				doorClose();
			}
		}
	}

	private void doorOpen()
	{
		doorAnimator.SetTrigger("Open");
		open = true;
	}

	private void doorClose()
	{
		doorAnimator.SetTrigger("Close");
		open = false;
	}

	public void doorToggle(bool force)
	{
		if ((force || !locked) && _doorType != doorType.AUTO)
		{
			if (open)
			{
				doorClose();
			}
			else
			{
				doorOpen();
			}
		}
		else if (force && locked)
		{
			doorUnlock();
		}
	}

	public void doorLock()
	{
		locked = true;
		lockSprite.color = keycardColor;
	}

	public void doorUnlock()
	{
		locked = false;
		lockSprite.color = GameManager.COLOR_GREEN;
	}

	public bool isOpen()
	{
		return open;
	}

	private void handlePowerOutage(bool powerOn)
	{
		//TODO
		if (!locked)
		{
		}
	}
}
