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
			this.actorWielder.drop();
			Debug.Log("Weapon broke: " + this.name);
			//actorWielder.actorAudioSource.PlayOneShot(weaponBreakSound);
			//might need to wait for sound to play out
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
		if (maxBatteries == 0)
		{
			maxBatteries = batterySprites.Length;
		}
		fillBatteries((int)startingCharge);

		baseSprite = sprite.sprite;
	}
}