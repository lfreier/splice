using System.Collections;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
	private float _interactInput;
	private float lastInteractInput;

	public Actor player;

	public PlayerInputs inputs;

	private void Start()
	{
		lastInteractInput = 0;
	}
	void Update()
	{

	}

	void InteractInputs()
	{
		_interactInput = inputs.interactInput();
		if (_interactInput > 0 && lastInteractInput == 0)
		{
			Collider2D[] hitTargets = Physics2D.OverlapCircleAll(this.transform.position, 1F, weaponLayer);

			foreach (Collider2D target in hitTargets)
			{
				if (target.tag.StartsWith("Object"))
				{
					if (target.tag.Equals(WeaponDefs.OBJECT_WEAPON_TAG))
					{
						Debug.Log("Picking up: " + target.name);
						if (player.equip(target.gameObject.transform.GetChild(0).gameObject))
						{
							//Make sure to only pick up one weapon
							break;
						}
					}
				}
			}
		}
	}
}