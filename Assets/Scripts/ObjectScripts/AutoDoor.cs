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
		if (force || !locked)
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
}
