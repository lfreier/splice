using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
	private static EffectManager instance = null;

	public EffectScriptable bleed1;

	public EffectScriptable stunHalf;
	public EffectScriptable stunParry;
	public EffectScriptable stun1;
	public EffectScriptable stun3;
	public EffectScriptable stunPermanent;

	public EffectScriptable iFrame0;
	public EffectScriptable iFrame1;

	public static EffectManager Instance
	{
		get
		{
			return instance;
		}
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			instance = this;
		}
	}
}