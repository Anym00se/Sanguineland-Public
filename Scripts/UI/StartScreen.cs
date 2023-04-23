using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour
{
    public Transform P_JoinedPlayers;
    public TMP_Text T_TimeOfDay;
    public Button startButton;
    public GameObject joinedPlayerBannerPrefab;


    // Start is called before the first frame update
    void Start()
    {
        T_TimeOfDay.text = string.Format("Time of day: {0}", SceneManager.GetActiveScene().name);
    }

    // Update is called once per frame
    void Update()
    {
        startButton.interactable = IsMasterClient();

        if (GameSyncer.instance.GetPlayerCount() != P_JoinedPlayers.childCount)
        {
            PopulateJoinedPlayers();
        }
    }

    bool IsMasterClient()
    {
        return GameSyncer.instance.IsMasterClient();
    }

    public void StartGame()
    {
        GameSyncer.instance.StartGame();
    }

    private void PopulateJoinedPlayers()
    {
        // Destroy the previous banners
        List<GameObject> removableObjects = new List<GameObject>();
        foreach(Transform child in P_JoinedPlayers)
        {
            removableObjects.Add(child.gameObject);
        }
        foreach(GameObject obj in removableObjects)
        {
            Destroy(obj);
        }

        // Add new banners
        GameObject[] joinedPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject joinedPlayer in joinedPlayers)
        {
            GameObject banner = Instantiate(joinedPlayerBannerPrefab);
            banner.transform.SetParent(P_JoinedPlayers, false);

            JoinedPlayerBanner bannerScript = banner.GetComponent<JoinedPlayerBanner>();
            bannerScript.player = joinedPlayer;
        }
    }
}
