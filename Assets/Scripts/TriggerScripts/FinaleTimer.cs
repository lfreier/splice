using System.Collections;
using TMPro;
using UnityEngine;

public class FinaleTimer : MonoBehaviour
{
	public float timerLength = 60;
	private float timer;

	public float secondTimerLength = 5;
	private float secondTimer = 0;

	private bool done;

	public TextMeshProUGUI displayTimer;

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

		timer -= Time.deltaTime;
		if (timer < 0)
		{
			displayTimer.text = string.Format("{0:00}:{1:00}:{2:000}", 0, 0, 0);
			displayTimer.color = Color.red;
			Time.timeScale = 0.3F;
			timer = 0;

			if (secondTimer == 0)
			{
				secondTimer = secondTimerLength + Time.deltaTime;
			}
		}
		else
		{
			displayTimer.text = string.Format("{0:00}:{1:00}:{2:000}", (int)(timer / 60), (int)(timer % 60), (int)((timer - (int)(timer % 360)) * 1000));
		}

		if (secondTimer != 0)
		{
			secondTimer -= Time.deltaTime;
			if (secondTimer <= 0)
			{
				secondTimer = 0;
				done = true;
				GameManager.Instance.playerStats.player.kill();
			}
		}
	}
}