using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionCanvas : MonoBehaviour
{
    [SerializeField] private GameObject characterSelectionWrapper;
    [SerializeField] private GameObject spinner;

    void Start()
    {
        ShowSpinner();
        StartCoroutine(ShowCharacterSelection());

        ClientManager.instance.StopAllMusic();
    }

    private void ShowSpinner()
    {
        spinner.SetActive(true);
        characterSelectionWrapper.SetActive(false);
    }

    private IEnumerator ShowCharacterSelection()
    {
        // Wait for everything to be ready
        while (!IsEverythingReady())
        {
            yield return null;
        }

        // Everything ready
        spinner.SetActive(false);
        characterSelectionWrapper.SetActive(true);

        // Spawn Game Syncer for each client
        NetworkManager.instance.SpawnGameSyncer();
    }

    private bool IsEverythingReady()
    {
        return NetworkManager.instance.IsConnectedAndReady();
    }

    public void SpawnGunner()
    {
        Utils.PlayButtonSound();
        NetworkManager.instance.SpawnGunner();
    }

    public void SpawnTorcher()
    {
        Utils.PlayButtonSound();
        NetworkManager.instance.SpawnTorcher();
    }

    public void SpawnBombardier()
    {
        Utils.PlayButtonSound();
        NetworkManager.instance.SpawnBombardier();
    }

    public void SpawnLaserer()
    {
        Utils.PlayButtonSound();
        NetworkManager.instance.SpawnLaserer();
    }
}
