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
		homeY = rTransform.anchoredPosition.y;
		span = image.sprite.rect.height * 10;
	}

	void LateUpdate()
	{
		float yPos = rTransform.anchoredPosition.y;

		yPos += (canvas.pixelRect.height * screensPerSecond) * Time.deltaTime;

		if (yPos - homeY >= span)
		{
			yPos = homeY;
		}

		rTransform.anchoredPosition = new Vector3(0, yPos, transform.position.z);
	}
}