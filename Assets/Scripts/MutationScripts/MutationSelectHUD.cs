using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MutationSelectHUD : MonoBehaviour
{
	/* each x, y pair is for X anchoring spacing */
	public Vector2[] mutSpacing2 = { new Vector2(0.15F, 0.45F), new Vector2(0.55F, 0.85F) };
	public Vector2[] mutSpacing3 = { new Vector2(0.05F, 0.3F), new Vector2(0.375F, 0.625F), new Vector2(0.7F, 0.95F) };
	public Vector2[] mutSpacing4 = { new Vector2(0.04F, 0.24F), new Vector2(0.28F, 0.48F), new Vector2(0.52F, 0.72F), new Vector2(0.76F, 0.96F) };

	public GameObject mutSelectPrefab;

	public GameObject[] selections;

	public GameObject tutorialObject;

	public MutationInterface[] mutArray;

	public MutationSelect select;

	public GameOver gameOver;
	public MenuHandler menu;

	public float clickProtectSeconds;

	private bool acceptHeld;

	public Image acceptFillImage;

	public Button backButton;
	public Button acceptButton;
	public Button nextButton;

	public Image tutorialImage;
	private Sprite[] currTutorialImages;
	private int tutorialImageIndex;

	public float holdTimerLength;
	private float acceptHoldTimer = 0F;

	private int currIndex;

	private float loadInTime = 0.5F;

	public void Update()
	{
		/* only used for hold to click menu buttons */

		//increment timer
		if (acceptHeld && holdTimerLength > 0 && acceptFillImage != null && Time.timeScale <= 0)
		{
			acceptHoldTimer += Time.fixedUnscaledDeltaTime;
			acceptFillImage.fillAmount = acceptHoldTimer / holdTimerLength;
			if (acceptHoldTimer >= holdTimerLength)
			{
				acceptHoldTimer = 0;
				acceptHeld = false;
				acceptMutation(currIndex);
			}
		}
		//decrement timer
		if (acceptHoldTimer > 0 && !acceptHeld)
		{
			acceptHoldTimer -= Time.fixedUnscaledDeltaTime;
			acceptFillImage.fillAmount = acceptHoldTimer / holdTimerLength;
		}
	}

	public void selectMutation(int index)
	{
		currIndex = index;
		enableMutationTutorial(mutArray[index].getTutorialSprites());
	}

	public void acceptMutation(int index)
	{
		/* Hacky way to add protection of mashing click */
		if (Time.unscaledTime - loadInTime < clickProtectSeconds || select == null)
		{
			return;
		}

		if (index < mutArray.Length)
		{
			select.makeSelection(mutArray[index]);
		}
		else
		{
			select.makeSelection(null);
		}
		menu.resumeGame();
	}

	public void enableMutationTutorial(Sprite[] mutTutorial)
	{
		foreach (GameObject obj in selections)
		{
			obj.SetActive(false);
		}

		tutorialObject.SetActive(true);
		currTutorialImages = mutTutorial;
		tutorialImageIndex = 0;
		if (currTutorialImages != null && currTutorialImages.Length > 0)
		{
			tutorialImage.sprite = currTutorialImages[0];
			if (currTutorialImages.Length > 1)
			{
				acceptButton.gameObject.SetActive(false);
				nextButton.gameObject.SetActive(true);
			}
			else
			{
				acceptButton.gameObject.SetActive(true);
				nextButton.gameObject.SetActive(false);
			}
		}
		else
		{
			acceptButton.gameObject.SetActive(true);
			nextButton.gameObject.SetActive(false);
		}
		tutorialImage.enabled = true;
		backButton.interactable = true;
		nextButton.interactable = true;
		acceptButton.interactable = true;
	}

	public void onBackButtonClick()
	{
		if (tutorialImageIndex == 0)
		{
			foreach (GameObject obj in selections)
			{
				obj.SetActive(true);
			}

			tutorialObject.SetActive(false);
			currTutorialImages = null;
			tutorialImageIndex = 0;

			tutorialImage.enabled = false;
			tutorialImage.sprite = null;
			acceptButton.interactable = false;
			backButton.interactable = false;
			nextButton.interactable = false;
		}
		else
		{
			tutorialImageIndex--;
			tutorialImage.sprite = currTutorialImages[tutorialImageIndex];
			acceptButton.gameObject.SetActive(false);
			acceptButton.interactable = false;
			nextButton.gameObject.SetActive(true);
			nextButton.interactable = true;
			backButton.interactable = true;
		}
	}

	public void nextMutationTutorial()
	{
		tutorialImageIndex++;
		tutorialImage.sprite = currTutorialImages[tutorialImageIndex];
		if (tutorialImageIndex >= currTutorialImages.Length - 1)
		{
			acceptButton.gameObject.SetActive(true);
			acceptButton.interactable = true;
			nextButton.gameObject.SetActive(false);
			nextButton.interactable = false;
		}
		else
		{
			acceptButton.gameObject.SetActive(false);
			acceptButton.interactable = false;
			nextButton.gameObject.SetActive(true);
			nextButton.interactable = true;
		}
	}

	private void setAnchors(RectTransform rect, int index)
	{
		float minY = 0.1F;
		float maxY = 0.9F;

		if (selections.Length == 2)
		{
			rect.anchorMin = new Vector2(mutSpacing2[index].x, minY);
			rect.anchorMax = new Vector2(mutSpacing2[index].y, maxY);
		}
		else if (selections.Length == 3)
		{
			rect.anchorMin = new Vector2(mutSpacing3[index].x, minY);
			rect.anchorMax = new Vector2(mutSpacing3[index].y, maxY);
		}
		else if (selections.Length == 4)
		{
			rect.anchorMin = new Vector2(mutSpacing4[index].x, minY);
			rect.anchorMax = new Vector2(mutSpacing4[index].y, maxY);
		}

		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}

	public void OnPointerDown()
	{
		acceptHeld = true;
	}

	public void OnPointerUp()
	{
		acceptHeld = false;
	}

	public void showMutationSelect(MutationSelect source)
	{
		Time.timeScale = 0;
		select = source;
		loadInTime = Time.unscaledTime;

		//save mutation array internally
		GameObject[] objArray = source.mutationPrefabs;
		selections = new GameObject[objArray.Length];
		mutArray = new MutationInterface[objArray.Length];
		gameOver.fadeInImages = new Image[objArray.Length * 3];
		gameOver.fadeInText = new TextMeshProUGUI[objArray.Length * 2];
		int menuLength = menu.menuOptions.Length;
		Image[] newMenuOpts = new Image[menuLength + (objArray.Length * 3)];
		int i = 0;
		for (i = 0; i < menuLength; i ++)
		{
			newMenuOpts[i] = menu.menuOptions[i];
		}
		menu.fadeInText = new TextMeshProUGUI[objArray.Length * 2];

		//set icons
		for (i = 0; i < objArray.Length; i++)
		{
			MutationInterface mut = objArray[i].GetComponentInChildren<MutationInterface>();
			mutArray[i] = mut;
			selections[i] = Instantiate(mutSelectPrefab);
			selections[i].transform.SetParent(this.transform);
			
			MutationSelectPanel panel = selections[i].GetComponentInChildren<MutationSelectPanel>();
			if (panel != null)
			{
				setAnchors(panel.selectRect, i);
				gameOver.fadeInImages[i] = panel.selectIcon;
				gameOver.fadeInImages[i + objArray.Length] = panel.selectBg;
				gameOver.fadeInImages[i + objArray.Length + objArray.Length] = panel.select;
				newMenuOpts[i + menuLength] = panel.selectIcon;
				newMenuOpts[i + menuLength + objArray.Length] = panel.selectBg;
				newMenuOpts[i + menuLength + objArray.Length + objArray.Length] = panel.select;
				panel.selectIcon.sprite = mut.getIcon();
				panel.mutIndex = i;
				panel.selectHUD = this;
				if (panel.selectTexts.Length > 0)
				{
					int j = 0;
					foreach (TextMeshProUGUI text in panel.selectTexts)
					{
						text.SetText(mut.getDisplayName());
						menu.fadeInText[(i * 2) + j] = text;
						gameOver.fadeInText[(i * 2) + j] = text;
						j++;
					}
					panel.selectTexts[0].ForceMeshUpdate();
					panel.selectTexts[1].enableAutoSizing = false;
					panel.selectTexts[1].fontSize = panel.selectTexts[0].fontSize;
				}
			}
		}

		menu.menuOptions = newMenuOpts;
		tutorialObject.SetActive(false);

		gameOver.transitioning = true;
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SceneDefs.SCENE_INDEX_MASK[(int)SceneDefs.SCENE.MUTATION_SELECT]));
	}
}