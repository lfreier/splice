using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private bool powerOn;

	public void startNewLevel()
	{
		powerOn = true;
	}

	public bool hasPower()
	{
		return powerOn;
	}

	void setPower(bool powerToSet)
	{
		powerOn = powerToSet;
	}
}