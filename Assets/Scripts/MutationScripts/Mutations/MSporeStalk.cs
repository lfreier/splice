using System.Collections;
using UnityEngine;

public class MSporeStalk : MonoBehaviour
{
	public float logMult;
	public float logUpMult;
	public float rotationUpMult;
	public float rotationDownMult;
	private float rotationAdd;
	//public float rotationAddSimple;
	public int direction = 1;
	private bool down = false;

	public float rotationMaxChange;
	private float rotationTotal;

	public float swayPeriod;
	private float currSwayPeriod;
	private float swayTimer;

	public MSpore attachedSpore;

	private Actor actor;

	private void Start()
	{
		actor = attachedSpore.actorWielder;
		swayTimer = currSwayPeriod = swayPeriod;
		rotationAdd = 0;
		rotationTotal = 0;
		down = true;
	}

	void Update()
	{
		if (actor != null)
		{
			/*
			swayTimer -= Time.deltaTime;
			transform.SetLocalPositionAndRotation(transform.localPosition, Quaternion.Euler(0, 0, transform.localEulerAngles.z + (direction * rotationAddSimple)));
			if (swayTimer <= 0)
			{
				swayTimer = swayPeriod;
				direction = -direction;
			}
			*/

			swayTimer -= Time.deltaTime;

			if (rotationTotal + rotationAdd >= rotationMaxChange)
			{
				rotationAdd = 0;
			}

			transform.SetLocalPositionAndRotation(transform.localPosition, Quaternion.Euler(0, 0, transform.localEulerAngles.z + (direction * rotationAdd)));
			rotationTotal += rotationAdd;

			if (down)
			{
				rotationAdd = rotationDownMult * Mathf.Pow(logMult, currSwayPeriod - swayTimer);
			}
			else
			{
				rotationAdd = rotationUpMult * Mathf.Pow(logUpMult, currSwayPeriod - swayTimer);
			}

			if (!down && swayTimer <= currSwayPeriod * 0.6F)
			{
				down = true;
			}
			
			if (swayTimer <= 0)
			{
				swayTimer = currSwayPeriod = swayPeriod + Random.Range(-1F, 1F);
				direction = -direction;
				rotationTotal = -rotationMaxChange;
				down = false;
			}
		}
	}
}