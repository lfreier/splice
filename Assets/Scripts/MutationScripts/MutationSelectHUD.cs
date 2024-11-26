using System;
using System.Collections;
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

	public MutationInterface[] mutArray;

	public MutationSelect select;

	public GameOver gameOver;
	public MenuHandler menu;

	public float clickProtectSeconds;

	private float loadInTime = 0.5F;

	public void selectMutation(int index)
	{
		/* Hacky way to add protection of mashing click */
		if (Time.unscaledTime - loadInTime < clickProtectSeconds)
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
		menu.menuOptions = new Image[objArray.Length * 3];

		//set icons
		for (int i = 0; i < objArray.Length; i++)
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
				menu.menuOptions[i] = panel.selectIcon;
				menu.menuOptions[i + objArray.Length] = panel.selectBg;
				menu.menuOptions[i + objArray.Length + objArray.Length] = panel.select;
				panel.selectIcon.sprite = mut.getIcon();
				panel.mutIndex = i;
				panel.selectHUD = this;
			}
		}

		gameOver.transitioning = true;
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SceneDefs.MUTATION_SELECT_SCENE));
	}
}