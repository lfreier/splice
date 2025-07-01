using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElevatorMenuScene : StationScreen
{
	public GameObject buttonPrefab;
	public List<GameObject> buttonList;
	public List<PlayerSpawnScriptable> elevatorSpawnList;

	public RectTransform scrollRect;
	public Scrollbar scrollbar;

	/*
	private float anchorMinY = 0.85F;
	private float anchorMaxY = 0.95F;
	private float anchorMinX = 0.25F;
	private float anchorMaxX = 0.75F;
	*/

	//private float yChange = -.13F;

	public int maxElevatorsShown = 15;

	public override void init(StationMenuManager manager)
	{
		base.init(manager);

		scrollRect.offsetMin = new Vector2(Screen.width * 0.75F, scrollRect.offsetMin.y);
		scrollbar.value = 1;

		buttonList = new List<GameObject>();
		elevatorSpawnList = new List<PlayerSpawnScriptable>();
		for (int i = 1; i < gameManager.levelManager.elevatorAvailable.Length && i < maxElevatorsShown; i ++)
		{
			if (gameManager.levelManager.elevatorAvailable[i] <= 0)
			{
				continue;
			}
			GameObject newButton = Instantiate(buttonPrefab, this.transform);
			RectTransform buttonRect = newButton.GetComponent<RectTransform>();
			if (buttonRect != null)
			{
				//buttonRect.anchorMin = new Vector2(anchorMinX, anchorMinY + (buttonList.Count * yChange));
				//buttonRect.anchorMax = new Vector2(anchorMaxX, anchorMaxY + (buttonList.Count * yChange));
				//buttonRect.offsetMax = Vector2.zero;
				//buttonRect.offsetMin = Vector2.zero;

				Button button = newButton.GetComponent<Button>();
				if (button != null)
				{
					button.onClick.AddListener(delegate { moveToFloor(newButton); });
					TextMeshProUGUI text = newButton.GetComponentInChildren<TextMeshProUGUI>();
					if (text != null)
					{
						if (manager.elevator != null && manager.elevator.nextSpawn != null && i == (int)manager.elevator.nextSpawn.elevatorIndex && gameManager.levelManager.elevatorAvailable[i] == 1)
						{
							text.text = "*" + gameManager.levelManager.elevatorSpawns[i].floorName + "*";
						}
						else
						{
							text.text = gameManager.levelManager.elevatorSpawns[i].floorName;
						}
					}
				}

				buttonList.Add(newButton);
				elevatorSpawnList.Add(gameManager.levelManager.elevatorSpawns[i]);
			}
		}
	}

	public void moveToFloor(GameObject button)
	{
		LevelManager levelManager = gameManager.levelManager;
		for (int i = 0; i < levelManager.elevatorAvailable.Length && i < maxElevatorsShown; i++)
		{
			if (i >= buttonList.Count)
			{
				break;
			}
			if (buttonList[i].Equals(button))
			{
				if (menuManager.elevator.elevatorIndex == elevatorSpawnList[i].elevatorIndex)
				{
					menuManager.exitMenu();
					break;
				}
				
				/* save when moving to the next floor for the first time */
				if (levelManager.elevatorAvailable[(int)elevatorSpawnList[i].elevatorIndex] == 1)
				{
					levelManager.lastSavedSpawn = elevatorSpawnList[i].spawnIndex;
					levelManager.lastSavedAtStation = false;

					/* save current level and but set last saved level to next */
					gameManager.save(menuManager.elevator.playerActor, (int)elevatorSpawnList[i].sceneIndex);

					if (menuManager.elevator.isExitElevator)
					{
						levelManager.elevatorAvailable[(int)elevatorSpawnList[i].elevatorIndex] = 2;
					}
				}

				gameManager.nextLevel(menuManager.elevator.playerActor, elevatorSpawnList[i].sceneIndex, (int)elevatorSpawnList[i].spawnIndex);
				break;
			}
		}
	}

	public override void onBackButton()
	{
		menuManager.exitMenu();
	}
}