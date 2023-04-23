using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndScreen : MonoBehaviour
{
    public Transform P_Players;
    public TMP_Text totalDetailsText;
    public Button exitButton;
    public TMP_Text exitButtonText;
    private float exitButtonTimer = 5f;
    [SerializeField] private GameObject endScreenPlayerBannerPrefab;
    private float timerToUpdateAgain = 1f;
    private bool updatedAgain = false;


    // Start is called before the first frame update
    void Start()
    {
        totalDetailsText.text = GetTotalDetailsText();
        PopulatePlayerDetails();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameSyncer.instance.HasGameEnded())
        {
            // Exit button
            exitButtonTimer -= Time.deltaTime;
            exitButton.interactable = CanExit();

            if (CanExit())
            {
                exitButtonText.text = "Return To Main Menu";
            }
            else
            {
                exitButtonText.text = Mathf.CeilToInt(exitButtonTimer).ToString();
            }

            // Update the details again after a few moments because Master client might have more recent details
            if (!updatedAgain)
            {
                timerToUpdateAgain -= Time.deltaTime;

                if (timerToUpdateAgain <= 0)
                {
                    updatedAgain = true;
                    totalDetailsText.text = GetTotalDetailsText();
                    PopulatePlayerDetails();
                }
            }
        }
    }

    public void ReturnToMainMenu()
    {
        Utils.PlayButtonSound();
        NetworkManager.instance.LoadLevel("Main Menu");
    }

    private bool CanExit()
    {
        return exitButtonTimer <= 0;
    }

    private string GetTotalDetailsText()
    {
        return string.Format(
            "Waves survived: {0}\nZombies killed: {1}\nTime passed: {2}",
            GameSyncer.instance.waveNumber - 1,
            GameSyncer.instance.GetTotalEnemiesKilled(),
            GameSyncer.instance.GetTotalGameTime()
        );
    }

    private void PopulatePlayerDetails()
    {
        // Destroy the previous banners
        List<GameObject> removableObjects = new List<GameObject>();
        foreach(Transform child in P_Players)
        {
            removableObjects.Add(child.gameObject);
        }
        foreach(GameObject obj in removableObjects)
        {
            Destroy(obj);
        }

        // Add new banners
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            GameObject banner = Instantiate(endScreenPlayerBannerPrefab);
            banner.transform.SetParent(P_Players, false);

            EndScreenPlayerBanner bannerScript = banner.GetComponent<EndScreenPlayerBanner>();
            bannerScript.player = player;
        }
    }
}
