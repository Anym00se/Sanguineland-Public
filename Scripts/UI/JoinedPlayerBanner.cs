using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class JoinedPlayerBanner : MonoBehaviour
{
    public GameObject player;
    public TMP_Text playerNameText;
    public Image playerIcon;
    [SerializeField] private Sprite gunnerImage;
    [SerializeField] private Sprite torcherImage;
    [SerializeField] private Sprite bombardierImage;
    [SerializeField] private Sprite lasererImage;


    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            playerNameText.text = string.Format("{0} / {1}", player.GetComponent<Player>().GetPhotonNickname(), GetRole());
            playerIcon.sprite = GetPlayerIcon();
        }
    }

    private Sprite GetPlayerIcon()
    {
        Sprite img = null;

        if (player.GetComponent<Gunner>())
        {
            img = gunnerImage;
        }
        else if (player.GetComponent<Torcher>())
        {
            img = torcherImage;
        }
        else if (player.GetComponent<Bombardier>())
        {
            img = bombardierImage;
        }
        else if (player.GetComponent<Laserer>())
        {
            img = lasererImage;
        }

        return img;
    }

    private string GetRole()
    {
        string role = "";

        if (player.GetComponent<Gunner>())
        {
            role = "Gunner";
        }
        else if (player.GetComponent<Torcher>())
        {
            role = "Torcher";
        }
        else if (player.GetComponent<Bombardier>())
        {
            role = "Bombardier";
        }
        else if (player.GetComponent<Laserer>())
        {
            role = "Laserer";
        }

        return role;
    }
}
