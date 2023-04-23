using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerNickname : MonoBehaviour
{
    [SerializeField] private Player player;
    private TMP_Text nicknameText;

    void Start()
    {
        nicknameText = gameObject.GetComponent<TMP_Text>();
        nicknameText.text = player.GetPhotonNickname();

        // Disable the nickname over Player's head for own character
        if (player.PhotonViewIsMine())
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Update the Photon nickname in Update because it's not synced right at Start
        if (nicknameText.text == "")
        {
            nicknameText.text = player.GetPhotonNickname();
        }
    }
}
