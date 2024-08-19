using System.Collections;
using UnityEngine;

public class Sound : MonoBehaviour
{
	public float timer;

	public SoundScriptable scriptable;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void FixedUpdate()
	{
		timer -= Time.deltaTime;

		if (timer <= 0)
		{
			Destroy(this.gameObject);
		}
	}

	public void start(SoundScriptable scriptable)
	{
		this.timer = scriptable.length;
		this.scriptable = scriptable;
	}
}