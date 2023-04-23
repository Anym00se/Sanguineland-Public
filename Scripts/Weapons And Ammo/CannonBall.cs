using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : Ammo
{
    public float splashDamage;
    public float explosionRadius;
    public GameObject explosionPrefab;
    public override void HandleCollision(GameObject other)
    {
        if (other != spawnedByObj && destroyOnCollision)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Player") || other.CompareTag("Terrain") || other.CompareTag("Environment"))
            {
                CauseExplosion();
                Destroy(gameObject);
            }
        }
    }

    void CauseExplosion()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
        explosion.GetComponent<Explosion>().damage = splashDamage;
        explosion.GetComponent<Explosion>().spawnedByObj = spawnedByObj;

        if (spawnedByObj.CompareTag("Enemy"))
        {
            explosion.GetComponent<Explosion>().lightIntensityMultiplier = .3f;
            explosion.GetComponent<Explosion>().lightRange = 5f;
            explosion.GetComponent<Explosion>().doDamageEnemies = false;
        }
    }
}
