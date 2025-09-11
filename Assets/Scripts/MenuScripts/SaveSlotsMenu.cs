using System.Collections;
using TMPro;
using UnityEngine;

public class SaveSlotsMenu : MonoBehaviour
{
	public MenuHandler menu;

	public TextMeshProUGUI[] slotsText;

	private bool[] slotValid;

	private void OnEnable()
	{
		initMenu();
	}

	public void initMenu()
	{
		int i = 0;
		slotValid = new bool[SaveManager.TOTAL_SAVES];
		for (i = 0; i < SaveManager.TOTAL_SAVES; i ++)
		{
			PlayerSaveData playerSave = SaveManager.loadPlayerDataFromDisk(i);
			GameManager.updateCellFontSize(slotsText[i], 2);
			if (null == playerSave)
			{
				slotsText[i].text = "N/A";
				slotValid[i] = false;
			}
			else
			{
				PrefabManager preMan = GameManager.Instance.prefabManager;
				MutationInterface mutInf;

				if (playerSave.mutationPrefabList != null && playerSave.mutationPrefabList.Length > 0)
				{
					switch (playerSave.mutationPrefabList[0])
					{
						case (int)mutationType.mSpore:
							mutInf = preMan.mutPSpore.GetComponentInChildren<MutationInterface>();
							break;
						case (int)mutationType.mSpider:
							mutInf = preMan.mutPSpider.GetComponentInChildren<MutationInterface>();
							break;
						case (int)mutationType.mRaptor:
							mutInf = preMan.mutPRaptor.GetComponentInChildren<MutationInterface>();
							break;
						case (int)mutationType.mLimb:
						default:
							mutInf = preMan.mutPLimb.GetComponentInChildren<MutationInterface>();
							break;
					}

					string mutName = mutInf.getDisplayName();

					slotsText[i].text = "SLOT " + 0 + (i + 1) + " - " + mutName;
				}
				else
				{
					slotsText[i].text = "SLOT " + 0 + (i + 1);
				}
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