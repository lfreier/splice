using System.Collections;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;
using static WeaponDefs;

public class TurretWeapon : BasicWeapon
{
	public GameObject projectilePrefab;
	public Vector3 shootOffset;

	public float flashTimerLength = 0.1F;
	private float flashTimer = 0;

	public float shootingTimerLength = 0.5F;
	public float stopShootingTimerLength = 2F;
	private float shootingTimer = 0;
	private bool startShooting = false;
	private bool keepShooting = false;

	private bool firingFlip = false;

	private float stopShootingTimer = 0;

	private EnemyMove enemyAi = null;

	private void FixedUpdate()
	{
		if (enemyAi == null)
		{
			enemyAi = actorWielder.GetComponentInChildren<EnemyMove>();
		}
		
		if (shootingTimer == 0)
		{
			if (enemyAi.attackTargetActor != null)
			{
				shootingTimer = shootingTimerLength;
			}
			else if (stopShootingTimer == 0 && startShooting)
			{
				shootingTimer = 0;
				stopShootingTimer = stopShootingTimerLength + Time.deltaTime;
			}
		}

		if (shootingTimer != 0)
		{
			shootingTimer -= Time.deltaTime;
			if (shootingTimer <= 0)
			{
				startShooting = true;
				shootingTimer = 0;
			}
		}

		if (stopShootingTimer != 0)
		{
			stopShootingTimer -= Time.deltaTime;
			if (stopShootingTimer <= 0)
			{
				startShooting = false;
				keepShooting = false;
				stopShootingTimer = 0;
			}
		}

		if (flashTimer != 0)
		{
			flashTimer -= Time.deltaTime;
			if (flashTimer <= 0)
			{
				flashTimer = 0;
				trailSprite.enabled = false;
			}
		}
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
				if (firingFlip)
				{
					weaponSwingAudioPlayer.PlayOneShot(toPlay, gameManager.effectsVolume * _weaponScriptable.soundSwingVolume);
				}
				else
				{
					weaponAudioPlayer.PlayOneShot(toPlay, gameManager.effectsVolume * _weaponScriptable.soundSwingVolume);
				}
				firingFlip = !firingFlip;
			}
		}

		if (projectilePrefab != null)
		{
			Vector3 origin = this.transform.TransformPoint(shootOffset);
			Vector3 target = origin + (Vector3.ClampMagnitude(origin - this.transform.position, 1) * 2);
			GameObject projectileObj = Instantiate(projectilePrefab, origin, Quaternion.identity, null);
			Projectile projectile = projectileObj.GetComponentInChildren<Projectile>();
			if (projectile != null)
			{
				projectile.launch(origin, target, actorWielder);
			}
		}

		if (trailSprite != null)
		{
			trailSprite.enabled = true;
			trailSprite.flipX = !trailSprite.flipX;
			flashTimer = flashTimerLength;
		}

		SoundDefs.createSound(this.transform.position, wallHitSound);

		return true;
	}
	override public bool inRange(Vector3 target)
	{
		if (enemyAi != null)
		{
			float aiRotation;
			float currRotation = this.transform.parent.localRotation.eulerAngles.z;

			if (currRotation < 0)
			{
				aiRotation = currRotation + 360;
			}
			else if (currRotation >= 360)
			{
				aiRotation = currRotation - 360;
			}
			else
			{
				aiRotation = currRotation;
			}

			if (startShooting && Mathf.Abs(enemyAi.turretRotateTarget - aiRotation) < enemyAi.moveTargetError)
			{
				keepShooting = true;
				return true;
			}
			else
			{
				return keepShooting;
			}
		}
		else
		{
			return false;
		}
	}

	override public void setStartingPosition(bool side)
	{
		transform.parent.SetLocalPositionAndRotation(new Vector3(_weaponScriptable.equipPosX, _weaponScriptable.equipPosY, 0), Quaternion.Euler(0, 0, transform.parent.localRotation.eulerAngles.z));
		anim.enabled = true;
	}

	override public bool toggleCollider(int enable)
	{
		return base.toggleCollider(enable);
	}
}