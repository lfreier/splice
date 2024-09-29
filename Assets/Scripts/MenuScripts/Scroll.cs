using System.Collections;
using UnityEngine;

public class Scroll : MonoBehaviour
{
	public float homeY;
	public float span = 0;
	public float scrollSpeed = 1;

	void Start()
	{
		homeY = transform.position.y;
	}

	void Update()
	{
		float yPos = transform.position.y;

		yPos += scrollSpeed;

		if (yPos - homeY >= span)
		{
			yPos = homeY;
		}

		transform.position = new Vector2(transform.position.x, yPos);
	}
}