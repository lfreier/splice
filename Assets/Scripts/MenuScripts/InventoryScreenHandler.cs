using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static PickupDefs;

public class InventoryScreenHandler : MonoBehaviour
{
	public TextMeshProUGUI		mutCountText;
	public GameObject[]			keycardDisplays;
	public TextMeshProUGUI[]	keycardDisplayCount;
	//TODO: not hack this
	public TextMeshProUGUI[]	itemDisplayCount = new TextMeshProUGUI[2];

	public Canvas invCanvas;

	private GameManager gameManager;

	[SerializeField]
	private InputActionReference cancel;
	private float lastCancel;

	void Start()
	{
		gameManager = GameManager.Instance;
		gameManager.inventoryOpenEvent += loadInventoryScreen;
		//gameManager.closeMenusEvent += quitInventoryScreen;
		invCanvas.enabled = false;
		lastCancel = 1;
	}

	private void Update()
	{
		if (invCanvas.enabled)
		{
			float cancelVal = cancel.action.ReadValue<float>();
			if (lastCancel <= 0 && cancelVal > 0)
			{
				quitInventoryScreen();
			}
			lastCancel = cancelVal;
		}
	}

	private void OnDestroy()
	{
		gameManager.inventoryOpenEvent -= loadInventoryScreen;
		//gameManager.closeMenusEvent -= quitInventoryScreen;
	}

	public void quitInventoryScreen()
	{
		lastCancel = 1;
		invCanvas.enabled = false;
		Time.timeScale = 1;
	}

	public void loadInventoryScreen()
	{
		if (invCanvas.enabled)
		{
			quitInventoryScreen();
			return;
		}

		//TODO: set the item select box to correct item
		//first time an item is picked up, put it in the correct static order, but not static placement

		updateMutCount(gameManager.playerStats.getMutationBar());
		for (int i = 0; i < MAX_USABLE_ITEM_TYPE + 1; i++)
		{
			updateItemCount(gameManager.playerStats.usableItemCount[i], (usableType)i);
		}
		for (int i = 0; i < MAX_KEYCARD_TYPE + 1; i ++)
		{
			updateKeycardCount(gameManager.playerStats.keycardCount[i], (keycardType)i);
		}

		invCanvas.enabled = true;
	}

	private void updateMutCount(int count)
	{
		mutCountText.SetText("" + count.ToString() + " / 100");
	}

	private void updateItemCount(int newCount, PickupDefs.usableType type)
	{
		if ((int)type >= itemDisplayCount.Length)
		{
			return;
		}

		itemDisplayCount[(int)type].SetText(newCount.ToString());

		if (newCount >= gameManager.maxPickups[(int)type])
		{
			itemDisplayCount[(int)type].color = GameManager.COLOR_RED;
		}
		else
		{
			itemDisplayCount[(int)type].color = Color.white;
		}
	}

	private void updateKeycardCount(int newCount, PickupDefs.keycardType type)
	{
		if ((int)type >= keycardDisplayCount.Length)
		{
			return;
		}

		/* TODO: this might need to be saved later */
		if ((int)type < keycardDisplays.Length && keycardDisplays[(int)type].activeSelf == false)
		{
			keycardDisplays[(int)type].SetActive(true);
		}

		keycardDisplayCount[(int)type].SetText(newCount.ToString());
	}
}