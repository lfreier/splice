using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneDefs;

public class EndingCredits : MonoBehaviour
{
	public Image[] fadeInImages;
	public float fadeInSpeed = 0.25F;

	public float buttonTimerLength = 5F;
	private float buttonTimer = 0F;

	private bool quitOnInput = false;

	private bool fadeInCredits = true;
	private PlayerInputs input;

	void Start()
	{
		fadeInCredits = true;
		buttonTimer = 0F;
		quitOnInput = false;
	}

	void Update()
	{
		if (fadeInCredits)
		{
			if (fadeInImages.Length > 0)
			{
				foreach (Image image in fadeInImages)
				{
					if (image != null)
					{
						image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Min(1F, image.color.a + (fadeInSpeed * Time.deltaTime)));
					}
				}

				if (fadeInImages[0].color.a == 1F)
				{
					fadeInCredits = false;
					buttonTimer = buttonTimerLength;
				}
			}
		}

		if (buttonTimer != 0)
		{
			buttonTimer -= Time.deltaTime;
			if (buttonTimer <= 0)
			{
				buttonTimer = 0F;
				//now quit on player input
				quitOnInput = true;
			}
		}

		if (quitOnInput)
		{
			if (input == null)
			{
				input = GameManager.Instance.playerStats.player.GetComponentInChildren<PlayerInputs>();
			}

			if (input != null && (input.attackInput() != 0 || input.interactInput() != 0 || input.inventoryInput() != 0))
			{
				quitOnInput = false;
				GameManager.Instance.quitToMenu();
			}
		}
	}
}