using System.Collections;
using TMPro;
using UnityEngine;

public class SaveSlotsMenu : MonoBehaviour
{
	public MenuHandler menu;

	public TextMeshProUGUI[] slotsText;

	private bool[] slotValid;

	private void Start()
	{
		initMenu();
	}

	public void initMenu()
	{
		int i = 0;
		slotValid = new bool[SaveManager.TOTAL_SAVES];
		for (i = 0; i < SaveManager.TOTAL_SAVES; i ++)
		{
			if (null == SaveManager.loadPlayerDataFromDisk(i))
			{
				slotsText[i].text = "N/A";
				slotValid[i] = false;
			}
			else
			{
				slotsText[i].text = "SLOT " + 0 + (i + 1);
				slotValid[i] = true;
			}
		}
	}

	public void loadSlot(int slotNum)
	{
		if (!slotValid[slotNum])
		{
			return;
		}

		for (int i = 0; i < SaveManager.TOTAL_SAVES; i++)
		{
			slotsText[i].enabled = false;
		}
		this.gameObject.SetActive(false);
		menu.gameObject.SetActive(true);
		menu.startGameFromSaveSlot(slotNum);
	}

	public void backButton()
	{
		menu.gameObject.SetActive(true);
		this.gameObject.SetActive(false);
		menu.playClickSound();
	}
}