using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private bool powerOn;

	public void startNewLevel()
	{
		powerOn = true;
	}

	/* TODO: this definitely doesn't work */
	public static void dropItem(GameObject toDrop, Vector2 target)
	{
		Rigidbody2D body = toDrop.GetComponentInChildren<Rigidbody2D>();
		if (body != null)
		{
			Vector2 newTarget = new Vector2(target.x + Random.Range(-1F, 1F), target.y + Random.Range(-1F, 1F));
			body.transform.Translate((newTarget - body.position) * 500F);
			body.transform.RotateAround(body.transform.position, Vector3.forward, Random.Range(-100F, 100F));
		}
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