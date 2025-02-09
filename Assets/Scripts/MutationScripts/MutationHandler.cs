using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutationHandler : MonoBehaviour
{
	public List<mutationType> heldMutations;

	private void Start()
	{
		heldMutations = new List<mutationType>();
	}

	public MutationInterface findMutation(mutationType type)
	{
		switch(type)
		{
			case mutationType.mBeast:
				MBeast beast = GetComponentInChildren<MBeast>();
				if (beast != null)
				{
					return beast;
				}
				break;
			case mutationType.mLimb:
				MLimb limb = GetComponentInChildren<MLimb>();
				if (limb != null)
				{
					return limb;
				}
				break;
			case mutationType.mWing:
				MBladeWing wing = GetComponentInChildren<MBladeWing>();
				if (wing != null)
				{
					return wing;
				}
				break;
			default:
				return null;
		}

		return null;
	}
}
