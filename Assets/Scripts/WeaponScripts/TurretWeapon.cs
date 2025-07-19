using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static WeaponDefs;

public class TurretWeapon : BasicWeapon
{
	private void OnDestroy()
	{
		GameManager gm = GameManager.Instance;
		if (gm != null)
		{
			gm.signalMovementUnlocked();
			gm.signalRotationUnlocked();
		}
	}

	private void FixedUpdate()
	{
	}

	override public void init()
	{
		base.init();
	}

	override public bool attack(LayerMask targetLayer)
	{
		actorWielder.invincible = false;
		soundMade = false;

		if (_weaponScriptable.soundSwing != null)
		{
			AudioClip toPlay;
			gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundSwing.name, out toPlay);
			if (toPlay != null)
			{
				weaponAudioPlayer.PlayOneShot(toPlay, gameManager.effectsVolume);
			}
		}

		return true;
	}

	override public void setStartingPosition(bool side)
	{
		transform.parent.SetLocalPositionAndRotation(new Vector3(_weaponScriptable.equipPosX, _weaponScriptable.equipPosY, 0), Quaternion.Euler(0, 0, _weaponScriptable.equipRotZ));
		anim.enabled = true;
	}

	override public bool toggleCollider(int enable)
	{
		return base.toggleCollider(enable);
	}
}