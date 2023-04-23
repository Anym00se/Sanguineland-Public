using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomness : MonoBehaviour
{
    public float minimumRotation = 0f;
    public float maximumRotation = 359f;
    public float minimumSize = 1f;
    public float maximumSize = 1f;


    void Start()
    {
        transform.rotation = Quaternion.Euler(0f, Random.Range(minimumRotation, maximumRotation), 0f);
        transform.localScale = Vector3.one * Random.Range(minimumSize, maximumSize);
    }
}
