using System.Collections;
using UnityEngine;

public class Sound : MonoBehaviour
{
	public float timer;

	public SoundScriptable scriptable;
	public CircleCollider2D soundCollider;

	void FixedUpdate()
	{
		timer -= Time.deltaTime;

		if (timer <= 0)
		{
			Destroy(this.gameObject);
		}
	}

	public void startSound(SoundScriptable scriptable)
	{
		this.timer = scriptable.length;
		this.scriptable = scriptable;

		if (scriptable != null && soundCollider != null)
		{
			soundCollider.radius = scriptable.radius;
			gameObject.SetActive(true);
		}
	}
}