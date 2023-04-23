using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : MonoBehaviour
{
    public static ClientManager instance { get; private set; }

    [Header("Settings")]
    public int difficulty;
    public int timeOfDay;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;
    public int graphicsQuality;
    private bool allowFriendlyFire = true;

    [SerializeField] private AudioSource buttonSound;
    [SerializeField] private AudioSource menuMusic;
    [SerializeField] private AudioSource gamePlayMusic;
    [SerializeField] private AudioSource gameEndMusic;


    private void Awake()
    {
        // Assign the global ClientManager.instance object
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // Prevent duplicates in the scene
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateSettings();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            //
        }
    }

    public void UpdateSettings()
    {
        timeOfDay = PlayerPrefs.GetInt("TimeOfDay", 0);
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
        graphicsQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
        difficulty = PlayerPrefs.GetInt("Difficulty", 3);
    }

    public bool GetAllowFriendlyFire()
    {
        allowFriendlyFire = PlayerPrefs.GetInt("AllowFriendlyFire", 1) == 1 ? true : false;
        return allowFriendlyFire;
    }

    public void SetAllowFriendlyFire(bool newState)
    {
        allowFriendlyFire = newState;
        PlayerPrefs.SetInt("AllowFriendlyFire", newState ? 1 : 0);
    }

    public void PlayButtonSound()
    {
        buttonSound.volume = Utils.GetSoundVolume();
        buttonSound.Play();
    }

    public void PlayMenuMusic()
    {
        StopAllMusic();

        menuMusic.volume = Utils.GetMusicVolume();
        menuMusic.Play();
    }

    public void PlayGameplayMusic()
    {
        StopAllMusic();

        gamePlayMusic.volume = Utils.GetMusicVolume();
        gamePlayMusic.Play();
    }

    public void PlayGameEndMusic()
    {
        StopAllMusic();

        gameEndMusic.volume = Utils.GetMusicVolume();
        gameEndMusic.Play();
    }

    public void StopAllMusic()
    {
        // Stop all previous musics
        menuMusic.Stop();
        gamePlayMusic.Stop();
        gameEndMusic.Stop();
    }

    public void UpdateMusicVolume()
    {
        menuMusic.volume = Utils.GetMusicVolume();
        gamePlayMusic.volume = Utils.GetMusicVolume();
        gameEndMusic.volume = Utils.GetMusicVolume();
    }
}
