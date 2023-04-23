using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingLight : MonoBehaviour
{

    private float lightIntensity;
    [Range(0,1)]public float flickerPercentage;
    public float flickerSpeed = 1;
    private Light lightComponent;

    // Start is called before the first frame update
    void Start()
    {
        lightComponent = gameObject.GetComponent<Light>();
        lightIntensity = lightComponent.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (flickerPercentage > 0)
        {
            float perlinNoise = flickerPercentage * Mathf.PerlinNoise(Time.time * flickerSpeed, 0.0f);
            lightComponent.intensity = lightIntensity - lightIntensity * perlinNoise;
        }
    }
}