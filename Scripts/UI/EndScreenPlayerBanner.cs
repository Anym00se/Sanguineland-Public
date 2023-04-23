using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndScreenPlayerBanner : MonoBehaviour
{
    public GameObject player;
    [SerializeField] private JoinedPlayerBanner banner;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text deathsText;
    [SerializeField] private TMP_Text revivesText;
    [SerializeField] private TMP_Text superWeaponsText;


    // Start is called before the first frame update
    void Start()
    {
        banner.player = player;

        // Details
        Player playerScript = player.GetComponent<Player>();
        killsText.text = string.Format("Kills: {0}", playerScript.GetKills());
        deathsText.text = string.Format("Deaths: {0}", playerScript.GetDeaths());
        revivesText.text = string.Format("Revives: {0}", playerScript.GetRevives());
        superWeaponsText.text = string.Format("Super Weapons: {0}", playerScript.GetSuperWeapons());
    }
}
