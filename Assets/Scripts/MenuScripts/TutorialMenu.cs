using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenu : MonoBehaviour
{
	public Image tutorialImage;
	public Button button;
	GameObject objectToDestroy;

	private void Awake()
	{
		foreach (TutorialSceneLoader loader in FindObjectsByType<TutorialSceneLoader>(FindObjectsSortMode.None))
		{
			if (loader.triggered)
			{
				//this is the one
				objectToDestroy = loader.gameObject;
				this.tutorialImage.sprite = loader.tutorialImage;
				this.tutorialImage.enabled = true;
			}
		}
	}

	private void Update()
	{
		if (Time.timeScale == 1 && objectToDestroy != null)
		{
			button.interactable = false;
			Destroy(objectToDestroy);
		}
	}
}