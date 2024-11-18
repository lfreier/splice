using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutationHandler : MonoBehaviour
{
	public void triggerMutationAnim(mutationType type)
	{
		switch (type)
		{
			case mutationType.mWing:
				MBladeWing mWing = GetComponentInChildren<MBladeWing>();
				if (mWing != null)
				{
					mWing.bladeWingDash();
				}
				break;
			case mutationType.mLimb:
			default:
				break;
		}
	}
}
