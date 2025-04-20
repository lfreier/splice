using System.Collections;
using UnityEngine;

public class SavePopup : MonoBehaviour
{
	public MenuHandler menu;

	public float waitTime = 0.2F;

	private void Update()
	{
		waitTime -= Time.unscaledDeltaTime;
		if(waitTime <= 0)
		{
			menu.fadeOutScene();
			Destroy(this);
		}
	}

}