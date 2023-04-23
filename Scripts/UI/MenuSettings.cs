using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSettings : MonoBehaviour
{

    private bool ignoreChanges = true;

    [Header("Settings sliders")]
    [SerializeField] private Slider slider_timeOfDay;
    [SerializeField] private Slider slider_masterVolume;
    [SerializeField] private Slider slider_musicVolume;
    [SerializeField] private Slider slider_soundVolume;
    [SerializeField] private Slider slider_graphicsQuality;
    [SerializeField] private Slider slider_difficulty;


    void Awake()
    {
        ignoreChanges = true;

        slider_timeOfDay.value = PlayerPrefs.GetInt("TimeOfDay", 0);
        slider_masterVolume.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        slider_musicVolume.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        slider_soundVolume.value = PlayerPrefs.GetFloat("SoundVolume", 1f);
        slider_graphicsQuality.value = PlayerPrefs.GetInt("GraphicsQuality", 2);
        slider_difficulty.value = PlayerPrefs.GetInt("Difficulty", 3);

        ignoreChanges = false;
    }

    public void Change_TimeOfDay()
    {
        if (ignoreChanges)
        {
            return;
        }
        PlayerPrefs.SetInt("TimeOfDay", Mathf.FloorToInt(slider_timeOfDay.value));
        ClientManager.instance.timeOfDay = Mathf.FloorToInt(slider_timeOfDay.value);
    }

    public void Change_MasterVolume()
    {
        if (ignoreChanges)
        {
            return;
        }
        PlayerPrefs.SetFloat("MasterVolume", slider_masterVolume.value);
        ClientManager.instance.masterVolume = slider_masterVolume.value;

        ClientManager.instance.UpdateMusicVolume();
    }

    public void Change_MusicVolume()
    {
        if (ignoreChanges)
        {
            return;
        }
        PlayerPrefs.SetFloat("MusicVolume", slider_musicVolume.value);
        ClientManager.instance.musicVolume = slider_musicVolume.value;

        ClientManager.instance.UpdateMusicVolume();
    }

    public void Change_SoundVolume()
    {
        if (ignoreChanges)
        {
            return;
        }
        PlayerPrefs.SetFloat("SoundVolume", slider_soundVolume.value);
        ClientManager.instance.soundVolume = slider_soundVolume.value;
    }

    public void Change_GraphicsQuality()
    {
        if (ignoreChanges)
        {
            return;
        }
        PlayerPrefs.SetInt("GraphicsQuality", Mathf.FloorToInt(slider_graphicsQuality.value));
        ClientManager.instance.graphicsQuality = Mathf.FloorToInt(slider_graphicsQuality.value);

        // Change the graphics quality setting
        QualitySettings.SetQualityLevel(Mathf.FloorToInt(slider_graphicsQuality.value), true);
    }

    public void Change_Difficulty()
    {
        if (ignoreChanges)
        {
            return;
        }
        PlayerPrefs.SetInt("Difficulty", Mathf.FloorToInt(slider_difficulty.value));
        ClientManager.instance.difficulty = Mathf.FloorToInt(slider_difficulty.value);
    }
}
