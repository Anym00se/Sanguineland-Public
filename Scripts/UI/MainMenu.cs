using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject P_Multiplayer;
    public GameObject P_Settings;
    public GameObject P_Info;

    public GameObject joinServerButtonPrefab;
    public Transform serversListParent;

    private NetworkManager networkManager;
    public Button hostServerButton;
    public Toggle allowFriendlyFireToggle;


    void Start()
    {
        // Make sure local timescale is back to one
        Time.timeScale = 1f;

        // Disconnect every time a client goes to Main Menu
        // The initial state should always be "disconnected"
        NetworkManager.instance.DisconnectCR();

        ClientManager.instance.PlayMenuMusic();
    }

    public void StartSinglePlayer()
    {
        Utils.PlayButtonSound();
        StartCoroutine(StartSinglePlayerCR());
    }

    private IEnumerator StartSinglePlayerCR()
    {
        GetNetworkManager().DisconnectCR();

        while (GetNetworkManager().IsOfflineMode() == false || GetNetworkManager().IsInLobby())
        {
            yield return null;
        }

        HostServer();
    }

    public void ToggleMultiplayer()
    {
        Utils.PlayButtonSound();

        P_Multiplayer.SetActive(!P_Multiplayer.activeSelf);
        P_Settings.SetActive(false);
        P_Info.SetActive(false);

        // Case: Multiplayer section toggled active
        if (P_Multiplayer.activeSelf)
        {
            GetNetworkManager().ConnectCR();
            ShowServers();
            StartCoroutine(ShowServersWithDelay(1));
            StartCoroutine(ShowServersWithDelay(2));
            allowFriendlyFireToggle.isOn = ClientManager.instance.GetAllowFriendlyFire();
        }
        else
        {
            GetNetworkManager().DisconnectCR();
        }
    }

    public void ToggleSettings()
    {
        Utils.PlayButtonSound();

        P_Settings.SetActive(!P_Settings.activeSelf);
        P_Multiplayer.SetActive(false);
        P_Info.SetActive(false);
    }

    public void ToggleInfo()
    {
        Utils.PlayButtonSound();

        P_Info.SetActive(!P_Info.activeSelf);
        P_Multiplayer.SetActive(false);
        P_Settings.SetActive(false);
    }

    public void ExitGame()
    {
        Utils.PlayButtonSound();
        Application.Quit();
    }

    public void ShowServers()
    {
        Debug.Log("Show Servers");

        // List all available servers
        if (!GetNetworkManager().IsOfflineMode() && GetNetworkManager().IsInLobby())
        {
            ToggleMultiplayerSpinner(false);

            // Destroy the previous buttons
            List<GameObject> removableObjects = new List<GameObject>();
            foreach(Transform child in serversListParent)
            {
                removableObjects.Add(child.gameObject);
            }
            foreach(GameObject obj in removableObjects)
            {
                Destroy(obj);
            }

            // Create new buttons
            foreach(RoomInfo room in GetNetworkManager().GetRoomsList())
            {
                GameObject button = Instantiate(joinServerButtonPrefab);
                button.transform.SetParent(serversListParent, false);

                JoinServerButton buttonScript = button.GetComponent<JoinServerButton>();
                buttonScript.roomInfo = room;
                button.GetComponent<Button>().onClick.AddListener(delegate { JoinServer(room.Name); });
            }
        }
        else
        {
            ToggleMultiplayerSpinner(true);
        }
    }

    private NetworkManager GetNetworkManager()
    {
        if (!networkManager)
        {
            networkManager = NetworkManager.instance;
        }
        return networkManager;
    }

    public void JoinServer(string name)
    {
        Utils.PlayButtonSound();

        GetNetworkManager().JoinServer(name);
        // Photon should automatically sync scenes
    }

    public void HostServer()
    {
        Utils.PlayButtonSound();

        string name = GetHostedServerName();
        GetNetworkManager().CreateServer(name);

        string levelName = "";
        switch(ClientManager.instance.timeOfDay)
        {
            case 0:
                levelName = "Day";
                break;
            case 1:
                levelName = "Evening";
                break;
            case 2:
                levelName = "Night";
                break;
        }

        GetNetworkManager().LoadLevel(levelName);
    }

    private string GetHostedServerName()
    {
        return PlayerPrefs.GetString("PlayerName", "") + "'s game";
    }

    public void ToggleAllowFriendlyFire()
    {
        ClientManager.instance.SetAllowFriendlyFire(allowFriendlyFireToggle.isOn);
    }

    private void ToggleMultiplayerSpinner(bool doShow)
    {
        P_Multiplayer.transform.Find("P_Spinner").gameObject.SetActive(doShow);
        P_Multiplayer.transform.Find("P_Servers").gameObject.SetActive(!doShow);
    }

    // Use this function only on Refresh button's Onclick!
    public void PlayButtonSound()
    {
        Utils.PlayButtonSound();
    }

    private IEnumerator ShowServersWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ShowServers();
    }
}
