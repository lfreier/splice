using System.Collections;
using UnityEngine;

public class MusicChange : MonoBehaviour
{
	public MusicScriptable music;
	// Use this for initialization
	void Start()
	{
		if (music != null)
		{
			GameManager gm = GameManager.Instance;
			gm.signalStartMusicEvent(music);
		}
	}

}