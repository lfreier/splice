using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Scroll : MonoBehaviour
{
	public float homeY;
	public float span = 0;
	public float screensPerSecond = 1;

	public Image image;
	public RectTransform rTransform;
	public Canvas canvas;

	void Start()
	{
		rTransform.offsetMax = new Vector2(0, 0);
		rTransform.offsetMin = new Vector2(0, -(Screen.height * 2));
		homeY = rTransform.offsetMax.y;
		span = Screen.height;
	}

	void LateUpdate()
	{
		float yPos = rTransform.anchoredPosition.y;

		yPos += (canvas.pixelRect.height * screensPerSecond) * Time.deltaTime;

		if (yPos - homeY >= span)
		{
			yPos = homeY - span;
			rTransform.offsetMax = new Vector2(0, 0);
			rTransform.offsetMin = new Vector2(0, -(Screen.height * 2));
		}

		rTransform.anchoredPosition = new Vector3(0, yPos, transform.position.z);
	}
}