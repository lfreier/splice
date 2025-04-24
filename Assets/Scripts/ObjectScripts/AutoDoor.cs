using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class AutoDoor : MonoBehaviour, UsableInterface
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

	public AudioClip doorOpenSound;
	public AudioClip doorCloseSound;
	public AudioClip doorUnlockSound;

	public AudioSource doorAudioPlayer;

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
			doorUnlockQuiet();
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
		if (doorAudioPlayer != null && gameManager != null && gameManager.audioManager != null && doorOpenSound != null)
		{
			AudioClip toPlay;
			if (gameManager.audioManager.soundHash.TryGetValue(doorOpenSound.name, out toPlay) && toPlay != null)
			{
				doorAudioPlayer.PlayOneShot(toPlay);
			}
		}
	}

	private void doorClose()
	{
		doorAnimator.SetTrigger("Close");
		open = false;
		if (doorAudioPlayer != null && gameManager != null && gameManager.audioManager != null && doorCloseSound != null)
		{
			AudioClip toPlay;
			if (gameManager.audioManager.soundHash.TryGetValue(doorCloseSound.name, out toPlay) && toPlay != null)
			{
				doorAudioPlayer.PlayOneShot(toPlay);
			}
		}
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
		if (doorAudioPlayer != null && gameManager != null && gameManager.audioManager != null && doorUnlockSound != null)
		{
			AudioClip toPlay;
			if (gameManager.audioManager.soundHash.TryGetValue(doorUnlockSound.name, out toPlay) && toPlay != null)
			{
				doorAudioPlayer.PlayOneShot(toPlay);
			}
		}
	}


	public void doorUnlockQuiet()
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

	public bool playerHasKey(PlayerStats stats)
	{
		return stats.keycardCount[(int)lockType] > 0
			&& _doorType != AutoDoor.doorType.REMOTE
			&& locked;
	}

	public virtual bool use(Actor user)
	{
		PlayerStats stats = user.gameManager.playerStats;
		if (playerHasKey(stats))
		{
			doorUnlock();
			stats.useKeycard((int)lockType);
			return true;
		}
		return false;
	}
}
