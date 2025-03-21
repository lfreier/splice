﻿using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationSelectPanel : MonoBehaviour
{
	public Image select;
	public Image selectBg;
	public Image selectIcon;
	public RectTransform selectRect;
	public MutationSelectHUD selectHUD;
	public TextMeshProUGUI[] selectTexts;
	public int mutIndex;

	public void selectMutation()
	{
		selectHUD.selectMutation(mutIndex);
	}
}