using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationHUD : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;


    void Awake()
    {
        // Start with the pause menu hidden
        HidePausePanel();
    }

    public void TogglePause()
    {
        Utils.PlayButtonSound();

        GameSyncer.instance.TogglePause();
    }

    public void ShowPausePanel()
    {
        pausePanel.SetActive(true);
    }

    public void HidePausePanel()
    {
        pausePanel.SetActive(false);
    }

    public void GoToMenu()
    {
        Utils.PlayButtonSound();

        // Go to Main Menu
        NetworkManager.instance.LoadLevel("Main Menu");
    }
}
