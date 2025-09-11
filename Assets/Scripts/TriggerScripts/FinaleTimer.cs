using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinaleTimer : MonoBehaviour
{
	public float timerLength = 60;
	private float timer;
	private bool timerStop = false;

	public float secondTimerLength = 5;
	private float secondTimer = 0;

	private bool done;

	public TextMeshProUGUI displayTimer;
	public Image redLayer;
	private float redAlpha = 0;

	void Start()
	{
		displayTimer.text = string.Format("{0:00}:{1:00}:{2:000}", (int)(timer / 60), (int)(timer % 60), (int)((timer - (int)(timer % 360)) * 1000));
		timer = timerLength + Time.deltaTime;
		secondTimer = 0;
		done = false;
	}

	void Update()
	{
		if (done)
		{
			return;
		}

		if (timer > 0)
		{
			timer -= Time.deltaTime;
		}
		if (timer <= 0 && !timerStop)
		{
			displayTimer.text = string.Format("{0:00}:{1:00}:{2:000}", 0, 0, 0);
			displayTimer.color = Color.red;
			Time.timeScale = 0.3F;
			timer = 0;
			timerStop = true;

			if (secondTimer == 0)
			{
				secondTimer = secondTimerLength + Time.deltaTime;
				redAlpha = 0;
			}
		}
		else
		{
			displayTimer.text = string.Format("{0:00}:{1:00}:{2:000}", (int)(timer / 60), (int)(timer % 60), (int)((timer - (int)(timer % 360)) * 1000));
		}

		if (secondTimer != 0)
		{
			secondTimer -= Time.deltaTime;
			redAlpha = (secondTimerLength - secondTimer) / 2;
			redLayer.color = new Color(redLayer.color.r, redLayer.color.g, redLayer.color.b, redAlpha);
			if (secondTimer <= 0)
			{
				secondTimer = 0;
				done = true;
				GameManager.Instance.playerStats.player.kill();
			}
		}
	}
}