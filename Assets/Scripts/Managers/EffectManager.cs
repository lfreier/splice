using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
	private static EffectManager instance = null;

	public EffectScriptable bleed1;
	public EffectScriptable bleed1Half;
	public EffectScriptable bleed2Half;

	public EffectScriptable stunHalf;
	public EffectScriptable stunThrow;
	public EffectScriptable stunParry;
	public EffectScriptable stun1;
	public EffectScriptable stun1Half;
	public EffectScriptable stun2Half;
	public EffectScriptable stun3;
	public EffectScriptable stunPermanent;

	public EffectScriptable iFrame0;
	public EffectScriptable iFrame1;

	public EffectScriptable sus1;
	public EffectScriptable seeking1;
	public EffectScriptable notice1;

	public EffectScriptable turretSus;
	public EffectScriptable turretSeeking;
	public EffectScriptable turretNotice;

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