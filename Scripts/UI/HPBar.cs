using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    public Transform foreground;
    public Transform background;
    public bool hideWhenHPFull = true;
    [SerializeField] private GameObject wrapper;


    public void UpdateHP(float health, float maxHealth)
    {
        float hpPercent = health / maxHealth;
        foreground.localScale = new Vector3(hpPercent, 1f, 1f);
        foreground.localPosition = new Vector3(-0.5f + (hpPercent / 2f), 0f, -0.01f);

        if (hideWhenHPFull && health >= maxHealth)
        {
            wrapper.SetActive(false);
        }
        else
        {
            wrapper.SetActive(true);
        }
    }
}
