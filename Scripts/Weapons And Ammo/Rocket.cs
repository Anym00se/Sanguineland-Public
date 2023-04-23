using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : Ammo
{
    public float splashDamage;
    public float explosionRadius;
    public GameObject explosionPrefab;
    public Vector3 targetPosition;
    private Rigidbody rb;
    [SerializeField] private GameObject projector;


    public override void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        projector.transform.position = targetPosition + Vector3.up * 20f;
        projector.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        base.Start();
    }

    public override void HandleCollision(GameObject other)
    {
        if (other != spawnedByObj && destroyOnCollision && other.CompareTag("Enemy"))
        {
            CauseExplosion();
            Destroy(gameObject);
        }
    }

    void CauseExplosion()
    {
        GameObject explosionObj = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Explosion explosion = explosionObj.GetComponent<Explosion>();

        explosion.explosionRadius = explosionRadius;
        explosion.damage = splashDamage;
        explosion.lightIntensityMultiplier = 2f;
        explosion.allowFriendlyFire = false;
        explosion.spawnedByObj = spawnedByObj;
    }

    public override void FixedUpdate()
    {
        rb.velocity = (targetPosition - transform.position).normalized * 40f;
        transform.LookAt(targetPosition);

        if (transform.position.y < 0f)
        {
            CauseExplosion();
            Destroy(gameObject);
        }

        // Update target marker position
        projector.transform.position = targetPosition + Vector3.up * 20f;
        projector.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
