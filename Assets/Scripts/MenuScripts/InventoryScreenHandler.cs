using System.Collections;
using TMPro;
using UnityEngine;

public class InventoryScreenHandler : MonoBehaviour
{
	public TextMeshProUGUI		mutCountText;
	public TextMeshProUGUI[]	keycardDisplayCount;

	private GameManager gameManager;

	void Start()
	{
		gameManager = GameManager.Instance;

		gameManager.updateCellCount += updateMutCount;
		gameManager.updateKeycardCount += updateKeycardCount;
	}

	private void OnDestroy()
	{
		gameManager.updateCellCount -= updateMutCount;
		gameManager.updateKeycardCount -= updateKeycardCount;
	}

	private void updateMutCount(int count)
	{
		mutCountText.SetText("" + gameManager.playerStats.mutationBar.ToString() + " / 100");
	}

	private void updateKeycardCount(int newCount, PickupDefs.keycardType type)
	{
		if ((int)type >= keycardDisplayCount.Length)
		{
			return;
		}

		keycardDisplayCount[(int)type].SetText(newCount.ToString());
	}
}