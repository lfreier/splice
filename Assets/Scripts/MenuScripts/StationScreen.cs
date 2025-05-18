using System.Collections;
using UnityEngine;

public abstract class StationScreen : MonoBehaviour
{
	public GameManager gameManager;
	public StationMenuManager menuManager;
	public SaveStation station;

	public virtual void init(StationMenuManager manager)
	{
		gameManager = GameManager.Instance;
		station = manager.station;
		menuManager = manager;
	}

	public virtual void onBackButton()
	{
		menuManager.backToMenu();
	}
}