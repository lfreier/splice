using NUnit.Framework.Constraints;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DatabaseScreen : StationScreen
{
	public TextMeshProUGUI[] weaponNames;

	public override void init(StationMenuManager manager)
	{
		base.init(manager);

		for (int i = 0; i < gameManager.playerStats.weaponsScanned.Length; i++)
		{
			if (i < weaponNames.Length && gameManager.playerStats.weaponsScanned[i])
			{
				BasicWeapon weaponPrefab = gameManager.prefabManager.weaponPrefabs[i].GetComponentInChildren<BasicWeapon>();
				if (weaponPrefab != null)
				{
					weaponNames[i].text = weaponPrefab._weaponScriptable.displayName;
				}
			}
		}
		
		foreach (TextMeshProUGUI text in this.GetComponentsInChildren<TextMeshProUGUI>())
		{
			if (text == null)
			{
				continue;
			}
			GameManager.updateCellFontSize(text, 1);
		}
	}

	public void onWeaponClick(int index)
	{
		//if available, go to screen, else error sound
		if (index < gameManager.playerStats.weaponsScanned.Length && gameManager.playerStats.weaponsScanned[index])
		{
			menuManager.playClickSound();
			menuManager.changeScreen(menuManager.scanScreenDB, index);
		}
		else
		{
			gameManager.playSound(menuManager.stationClickPlayer, gameManager.audioManager.errorSound.name, 1F);
		}
	}
}