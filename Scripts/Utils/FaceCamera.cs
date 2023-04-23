using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        GetCamera();
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position - GetCamera().transform.localPosition);
    }

    private Camera GetCamera()
    {
        if (!mainCamera)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera)
        {
            return mainCamera;
        }
        else
        {
            return GameObject.FindGameObjectWithTag("BirdViewCamera").GetComponent<Camera>();
        }
    }
}
