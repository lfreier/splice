using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class WeaponPhysics : MonoBehaviour
{
	private WeaponInterface _weapon;
	private WeaponScriptable _weaponScriptable;

	private float currentSpeed;

	private Vector3 _throwMove;
	private Vector3 _lastThrowMove;

	void Start()
	{
		currentSpeed = 0;
	}

	void Update()
	{
		/* if currently being thrown... */
		if (_lastThrowMove.magnitude > 0)
		{
			/* if the throw should damage */
			if (currentSpeed <= _weaponScriptable.throwHurtSpeed)
			{
				_weapon.setHitbox(false);
			}
			if (currentSpeed <= 0)
			{
				_lastThrowMove = new Vector3(0, 0, 0);
				currentSpeed = 0;
			}
		}
	}

	public void calculateThrow()
	{
		if (_throwMove.magnitude > 0)
		{
			_lastThrowMove = new Vector3(_throwMove.x, _throwMove.y, 0);
			_throwMove = new Vector3(0, 0, 0);
			currentSpeed = _weaponScriptable.throwSpeed;
		}

		if (_lastThrowMove.magnitude > 0)
		{
			currentSpeed -= _weaponScriptable.throwWeight;

			_weapon.physicsMove(new Vector3(_lastThrowMove.x * currentSpeed * Time.deltaTime, _lastThrowMove.y * currentSpeed * Time.deltaTime));
			this.transform.Rotate(new Vector3(0, 0, WeaponDefs.THROW_ROTATE_MID * (currentSpeed / _weaponScriptable.throwSpeed) * _weaponScriptable.throwWeight * Time.deltaTime));
		}
	}

	public bool isBeingThrown()
	{
		return _lastThrowMove.magnitude > 0;
	}

	/* THIS MUST BE CALLED IN EACH WEAPON'S START FUNCTION */
	public void linkInterface(WeaponInterface toLink)
	{
		_weapon = toLink;
		_weaponScriptable = _weapon.getScriptable();
	}

	public void startThrow(Vector3 target)
	{
		_throwMove = target;
	}
}