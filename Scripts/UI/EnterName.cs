using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EnterName : MonoBehaviour
{
    [SerializeField] private Button goToMenuButton;
    [SerializeField] private TMP_InputField nameInput;
 
    // Start is called before the first frame update
    void Start()
    {
        nameInput.text = PlayerPrefs.GetString("PlayerName", "");
    }

    // Update is called once per frame
    void Update()
    {
        goToMenuButton.interactable = nameInput.text != "";

        if (goToMenuButton.interactable && Input.GetButtonDown("Submit"))
        {
            GoToMenu();
        }
    }

    public void GoToMenu()
    {
        Utils.PlayButtonSound();

        string playerName = nameInput.text;
        PlayerPrefs.SetString("PlayerName", playerName);
        SceneManager.LoadScene("Main Menu");
    }
}
