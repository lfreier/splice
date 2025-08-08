using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static LevelManager;
using static System.Net.Mime.MediaTypeNames;

public class ElevatorFinaleMenuScreen : StationScreen
{
	public PlayerSpawnScriptable spawnScriptable;
	public string[] finaleText;

	public TextMeshProUGUI displayText;
	public TextMeshProUGUI buttonText;
	public TextMeshProUGUI exitText;

	private int textIndex = 0;
	private LevelManager levelManager;

	public override void init(StationMenuManager manager)
	{
		base.init(manager);
		levelManager = GameManager.Instance.levelManager;
		textIndex = 0;
		displayText.text = finaleText[textIndex];

		GameManager.updateCellFontSize(displayText, 1);
		GameManager.updateCellFontSize(buttonText, 1);
		GameManager.updateCellFontSize(exitText, 1);
	}

	public void onContinueButton()
	{
		if (finaleText == null)
		{
			return;
		}

		textIndex++;
		if (textIndex == finaleText.Length)
		{
			/* last index */
			levelManager.lastSavedSpawn = spawnScriptable.spawnIndex;
			levelManager.lastSavedAtStation = false;

			/* save current level and but set last saved level to next */
			gameManager.save(menuManager.elevator.playerActor, (int)spawnScriptable.sceneIndex);

			gameManager.nextLevel(menuManager.elevator.playerActor, spawnScriptable.sceneIndex, (int)spawnScriptable.spawnIndex);
		}
		else
		{
			displayText.text = finaleText[textIndex];

			if (textIndex == finaleText.Length - 1)
			{
				buttonText.text = "CONTINUE";
			}
			else
			{
				buttonText.text = "NEXT";
			}
		}

		GameManager.updateCellFontSize(buttonText, 1);
	}
}