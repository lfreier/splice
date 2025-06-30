using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationScreen : StationScreen
{
	public Image mutImage;

	public Button nextButton;
	public Button backButton;

	private Sprite[] mutSprites;
	private int mutImageIndex = 0;

	public override void init(StationMenuManager manager)
	{
		base.init(manager);
		if (station.playerActor != null)
		{
			MutationHandler mutHandle = station.playerActor.mutationHolder.GetComponent<MutationHandler>();
			if (mutHandle != null)
			{
				mutSprites = mutHandle.majorMutation.getTutorialSprites();
				mutImage.sprite = mutSprites[0];
				mutImage.enabled = true;
				mutImageIndex = 0;
				if (mutImageIndex >= mutSprites.Length - 1)
				{
					nextButton.gameObject.SetActive(false);
					nextButton.interactable = false;
				}
			}
		}
	}

	public void nextScreenButton()
	{
		mutImageIndex++;
		mutImage.sprite = mutSprites[mutImageIndex];
		if (mutImageIndex >= mutSprites.Length - 1)
		{
			nextButton.gameObject.SetActive(false);
			nextButton.interactable = false;
		}
		else
		{
			nextButton.gameObject.SetActive(true);
			nextButton.interactable = true;
		}
		menuManager.playClickSound();
	}

	public void backScreenButton()
	{
		if (mutImageIndex == 0)
		{
			mutImage.enabled = false;
			mutImage.sprite = null;
			mutSprites = null;
			mutImageIndex = 0;

			nextButton.interactable = false;
			backButton.interactable = false;

			this.onBackButton();
		}
		else
		{
			mutImageIndex--;
			mutImage.sprite = mutSprites[mutImageIndex];
			nextButton.gameObject.SetActive(true);
			nextButton.interactable = true;
			backButton.interactable = true;
			menuManager.playClickSound();
		}
	}
}