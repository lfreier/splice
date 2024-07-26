using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
	private bool open;
	private bool locked;
	public SpriteRenderer lockSprite;

	public Animator doorAnimator;

	private float detectSize = 3F;

	private GameManager gameManager;

	private void Start()
	{
		open = false;
		locked = false;
		gameManager = GameManager.Instance;
	}

	private void Update()
	{

		RaycastHit2D[] hit = Physics2D.BoxCastAll(new Vector2(gameObject.transform.position.x, gameObject.transform.position.y), new Vector2(detectSize, detectSize), 0F, Vector2.up, detectSize, gameManager.actorLayers);
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
		lockSprite.color = new Color(0.4F,0.1F,0.1F,1F);
	}

	public void doorUnlock()
	{
		locked = false;
		lockSprite.color = new Color(0.15F, 0.4f, 0, 1F);
	}
}
