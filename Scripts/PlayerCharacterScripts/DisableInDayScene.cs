using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisableInDayScene : MonoBehaviour
{
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Day")
        {
            gameObject.SetActive(false);
        }
    }
}
