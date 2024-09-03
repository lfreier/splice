using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
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
		switch (lockType)
		{
			case PickupDefs.keycardType.BLUE:
				keycardColor = GameManager.COLOR_BLUE;
				break;
			case PickupDefs.keycardType.RED:
			default:
				keycardColor = GameManager.COLOR_RED;
				break;
		}

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

		Collider2D[] hit = Physics2D.OverlapBoxAll(gameObject.transform.position, new Vector2(detectSize, detectSize), 0, gameManager.actorLayers);
		if (!open && !locked && hit.Length > 0)
		{
			doorOpen();
		}
		
		if(open && hit.Length <= 0)
		{
			doorClose();
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
