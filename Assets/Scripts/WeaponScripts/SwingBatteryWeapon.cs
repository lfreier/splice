using Unity.VisualScripting;
using UnityEngine;

public class SwingBatteryWeapon : BasicWeapon
{
	private Sprite baseSprite;
	public SpriteRenderer[] batterySprites;

	public float startingCharge = 2;
	public float chargedDamage = 2.5F;
	public int maxBatteries = 0;
	public int filledBatteries = 0;

	public AudioClip soundChargedActorHit;
	public float chargedActorHitVolume = 0.2F;
	public AudioClip soundChargedSwing;
	public float chargedSwingVolume = 0.4F;

	public override bool attack(LayerMask layerName)
	{
		soundMade = false;
		anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
		//lastTargetLayer = targetLayer;
		actorWielder.invincible = false;

		string soundName = _weaponScriptable.soundSwing.name;
		float volume = _weaponScriptable.soundSwingVolume;
		if (filledBatteries > 0)
		{
			soundName = this.soundChargedSwing.name;
			volume = chargedSwingVolume;
		}
		if (soundName != null)
		{
			playSound(weaponSwingAudioPlayer, soundName, volume);
		}

		return true;
	}
	public override void reduceDurability(float reduction)
	{
		if (durability < 0)
		{
			return;
		}

		/* don't add durability when charging the battery */
		if (reduction > 0)
		{
			durability -= reduction;
		}

		if (reduction <= 0)
		{
			fillBatteries(-(int)reduction);
			sprite.sprite = baseSprite;
		}
		else
		{
			depleteBatteries((int)reduction);
		}

		if (durability <= (_weaponScriptable.durability / 4) && damagedSprite != null)
		{
			//set to damaged sprite
			sprite.sprite = damagedSprite;
		}

		if (durability <= 0)
		{
			if (_weaponScriptable.soundBreak != null)
			{
				AudioClip toPlay;
				if (gameManager == null)
				{
					gameManager = GameManager.Instance;
				}
				gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundBreak.name, out toPlay);
				if (toPlay != null)
				{
					if (actorWielder == null)
					{
						weaponAudioPlayer.PlayOneShot(toPlay, gameManager.effectsVolume);
					}
					else
					{
						actorWielder.actorAudioSource.PlayOneShot(toPlay, gameManager.effectsVolume);
					}
				}
				if (actorWielder != null)
				{
					this.actorWielder.drop();
				}
			}

			Destroy(this.transform.parent.gameObject);
		}
	}

	public void depleteBatteries(int toDeplete)
	{
		float newCount = Mathf.Max(0, filledBatteries - toDeplete);

		for (int i = filledBatteries - 1; i >= newCount; i--)
		{
			batterySprites[i].enabled = false;
		}
		filledBatteries = (int)newCount;
	}

	public void fillBatteries(int toFill)
	{
		float newCount = Mathf.Min(maxBatteries, toFill + filledBatteries);

		for (int i = filledBatteries; i < newCount; i ++)
		{
			batterySprites[i].enabled = true;
		}
		filledBatteries = (int)newCount;
	}

	public void setBatteries(int toSet)
	{
		float newCount = Mathf.Min(maxBatteries, toSet);

		int i = 0;
		for (i = 0; i < newCount; i++)
		{
			batterySprites[i].enabled = true;
		}
		for ( ; i < maxBatteries; i ++)
		{
			batterySprites[i].enabled = false;
		}
		filledBatteries = (int)newCount;

		isInit = true;
	}

	public override float getWeaponDamage()
	{
		if (filledBatteries > 0)
		{
			return chargedDamage;
		}
		return base.getWeaponDamage();
	}

	public override void init()
	{
		if (durability == 0)
		{
			durability = _weaponScriptable.durability;
		}
		if (maxBatteries == 0)
		{
			maxBatteries = batterySprites.Length;
		}
		setBatteries((int)startingCharge);

		baseSprite = sprite.sprite;
	}

	public override void playSound(AudioSource player, string soundName, float volume)
	{
		if (soundName == _weaponScriptable.soundActorHit.name)
		{
			if (filledBatteries > 0)
			{
				soundName = this.soundChargedActorHit.name;
				volume = chargedActorHitVolume;
			}
		}

		AudioClip toPlay;
		if (gameManager.audioManager.soundHash.TryGetValue(soundName, out toPlay) && toPlay != null)
		{
			player.PlayOneShot(toPlay, volume * gameManager.effectsVolume);
		}
	}
}