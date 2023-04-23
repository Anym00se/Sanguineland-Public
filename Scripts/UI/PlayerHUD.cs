using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Player player;

    [SerializeField] private TMP_Text nameText;

    [SerializeField] private Transform HP_Bar;
    [SerializeField] private Transform Ammo_Bar;
    [SerializeField] private Transform SuperWeapon_Bar;
    [SerializeField] private GameObject SuperWeapon_Wrapper;
    [SerializeField] private Transform Wave_Bar;
    [SerializeField] private TMP_Text waveText;

    void Start()
    {
        nameText.text = player.GetPhotonNickname();
    }

    void Update()
    {
        // HP
        HP_Bar.localScale = new Vector3(player.health / player.maxHealth, 1f, 1f);

        // Ammo
        Laserer laserer = player.gameObject.GetComponent<Laserer>();
        if (laserer)
        {
            if (laserer.isUsingSuperWeapon)
            {
                Ammo_Bar.localScale = Vector3.one;
            }
            else
            {
                // Case: shooting
                if (laserer.drawLaser)
                {
                    Ammo_Bar.localScale = new Vector3(laserer.drawLaserTimer / laserer.laserShootingTime, 1f, 1f);
                }
                // Case: reloading
                else
                {
                    Ammo_Bar.localScale = new Vector3(Mathf.Clamp(1f - (laserer.shootTimer / laserer.reloadTime), 0f, 1f), 1f, 1f);
                }
            }
        }
        else
        {
            Ammo_Bar.localScale = new Vector3((float)player.ammoLeft / (float)player.maxAmmo, 1f, 1f);
        }

        // Super weapon
        SuperWeapon_Wrapper.SetActive(player.isUsingSuperWeapon);
        SuperWeapon_Bar.localScale = new Vector3(player.superWeaponTimeLeft / player.superWeaponDuration, 1f, 1f);

        // Wave
        waveText.text = "Wave " + GameSyncer.instance.waveNumber.ToString();
        if (GameSyncer.instance.enemyMaxCountInThisWave > 0)
        {
            Wave_Bar.localScale = new Vector3(1f - (float)GameSyncer.instance.enemiesDestroyedThisWave / (float)GameSyncer.instance.enemyMaxCountInThisWave, 1f, 1f);
        }
    }
}
