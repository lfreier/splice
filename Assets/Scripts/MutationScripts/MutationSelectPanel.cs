using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationSelectPanel : MonoBehaviour
{
	public Image select;
	public Image selectBg;
	public Image selectIcon;
	public RectTransform selectRect;
	public RectTransform textBgRect;
	public MutationSelectHUD selectHUD;
	public TextMeshProUGUI[] selectTexts;
	public int mutIndex;

	private void Start()
	{
		if (selectTexts != null && textBgRect != null && selectTexts.Length > 1)
		{
			if (selectTexts[1].text.Length > 6)
			{
				textBgRect.offsetMin = new Vector2(3, -3);
				textBgRect.offsetMax = new Vector2(3, -3);
			}
			else if (selectTexts[1].text.Length > 5)
			{
				textBgRect.offsetMin = new Vector2(3.5F, -3.5F);
				textBgRect.offsetMax = new Vector2(3.5F, -3.5F);
			}
			else if (selectTexts[1].text.Length < 5)
			{
				textBgRect.offsetMin = new Vector2(5, -5);
				textBgRect.offsetMax = new Vector2(5, -5);
			}
		}
	}

	public void selectMutation()
	{
		selectHUD.selectMutation(mutIndex);
	}
}