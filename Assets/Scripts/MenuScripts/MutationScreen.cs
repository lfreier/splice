using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationScreen : StationScreen
{
	public Image mutImage;

	public override void init(StationMenuManager manager)
	{
		base.init(manager);
		if (station.playerActor != null)
		{
			MutationHandler mutHandle = station.playerActor.mutationHolder.GetComponent<MutationHandler>();
			if (mutHandle != null)
			{
				Sprite[] sprites = mutHandle.majorMutation.getTutorialSprites();
				mutImage.sprite = sprites[0];
				mutImage.enabled = true;
			}
		}
	}
}