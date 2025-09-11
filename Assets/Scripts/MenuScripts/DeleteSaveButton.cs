using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeleteSaveButton : MonoBehaviour
{
	bool isHeld = false;

	public float holdTimerLength = 3F;
	public Image holdToFillImage;
	public SaveSlotsMenu saveMenu;

	public int buttonSaveSlot;

	private float holdTimer = 0;

	void Update()
	{
		//increment timer
		if (isHeld && holdTimerLength > 0 && holdToFillImage != null)
		{
			holdTimer += Time.fixedUnscaledDeltaTime;
			holdToFillImage.fillAmount = holdTimer / holdTimerLength;
			if (holdTimer >= holdTimerLength)
			{
				holdTimer = holdTimerLength;
				isHeld = false;
				deleteSave(buttonSaveSlot);
			}
		}
		//decrement timer
		else if (holdTimer > 0)
		{
			holdTimer -= Time.fixedUnscaledDeltaTime;
			holdToFillImage.fillAmount = holdTimer / holdTimerLength;
		}
	}

	public void onDeleteButtonPress()
	{
		isHeld = true;
	}

	public void onDeleteButtonRelease()
	{
		isHeld = false;
	}

	void deleteSave(int saveSlot)
	{
		if (null == SaveManager.loadPlayerDataFromDisk(saveSlot))
		{
			return;
		}
		else
		{
			SaveManager.deleteSaveSlot(saveSlot);
			GameManager gm = GameManager.Instance;
			if (gm != null && saveMenu.menu != null)
			{
				gm.playSound(saveMenu.menu.menuClickPlayer, gm.audioManager.errorSound.name, 1F);
			}
		}

		saveMenu.initMenu();
	}
}