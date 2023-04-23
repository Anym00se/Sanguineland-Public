using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class JoinServerButton : MonoBehaviour
{
    public RoomInfo roomInfo;
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI roomDetails;


    void Start()
    {
        roomName.text = roomInfo.Name.ToString();
        roomDetails.text = string.Format("Players: {0} / {1}", roomInfo.PlayerCount, roomInfo.MaxPlayers);
    }
}
