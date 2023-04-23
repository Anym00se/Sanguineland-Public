using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameCircle : Ammo
{
    [SerializeField] private Transform particlesTr;
    private Collider col;
    [SerializeField] private AnimationCurve flameAlphaOverTime;


    public void Awake()
    {
        col = gameObject.GetComponent<Collider>();
        transform.localScale = Vector3.zero;
        particlesTr.localScale = transform.localScale;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        ScaleCircle();
        ToggleCollider();
        SetCircleTransparency();
        
        transform.position = spawnedByObj.transform.position;
        transform.rotation = Quaternion.identity;
    }

    private void ToggleCollider()
    {
        col.enabled = col.enabled ? false : true;
    }

    private void ScaleCircle()
    {
        transform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, 1f - lifeTime / originalLifeTime);

        particlesTr.localScale = transform.localScale;
    }

    private void SetCircleTransparency()
    {
        var main = particlesTr.GetComponent<ParticleSystem>().main;
        main.startColor = new Color(1f, 1f, 1f, flameAlphaOverTime.Evaluate(1f - lifeTime / originalLifeTime)); 
    }
}
