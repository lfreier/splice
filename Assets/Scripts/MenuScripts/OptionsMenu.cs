using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	public MenuHandler menu;

	public Slider musicSlider;
	public Slider effectsSlider;

	public AudioSource musicPlayer;

	SaveManager.OptionsSaveData currentOptions;

	void Start()
	{
		initOptions();
	}

	public void initOptions()
	{
		SaveManager.OptionsSaveData options = SaveManager.loadOptionsDataFromDisk();

		if (options.musicVolume < 0 || options.effectsVolume < 0)
		{
			options.musicVolume = 0.5F;
			options.effectsVolume = 0.5F;
		}

		musicSlider.value = options.musicVolume;
		GameManager.Instance.musicVolume = options.musicVolume;

		effectsSlider.value = options.effectsVolume;
		GameManager.Instance.effectsVolume = options.effectsVolume;

		currentOptions.musicVolume = options.musicVolume;
		currentOptions.effectsVolume = options.effectsVolume;
	}

	public void updateMusicVolume()
	{
		if (musicSlider != null)
		{
			currentOptions.musicVolume = musicSlider.value;
			if (musicPlayer != null)
			{
				if (musicSlider.value == 0)
				{
					musicPlayer.Pause();
				}
				else if (musicPlayer.volume == 0)
				{
					musicPlayer.UnPause();
				}
				musicPlayer.volume = musicSlider.value;
			}
			GameManager.Instance.signalVolumeChangeEvent(musicSlider.value);
		}
	}

	public void updateEffectsVolume()
	{
		if (effectsSlider != null)
		{
			currentOptions.effectsVolume = effectsSlider.value;
			GameManager.Instance.effectsVolume = effectsSlider.value;
		}
	}

	public void backAndSave()
	{
		SaveManager.saveOptionsDataToDisk(currentOptions);

		menu.gameObject.SetActive(true);
		this.gameObject.SetActive(false);
		menu.playClickSound();
	}
}