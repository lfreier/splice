using System.Collections;
using UnityEngine;

public class SaveStation : MonoBehaviour, UsableInterface
{
	public void use(Actor user)
	{
		return;
		GameManager gameManager = GameManager.Instance;
		if (gameManager != null )
		{
			gameManager.save(user);
		}
		//TODO: failure case
	}
}