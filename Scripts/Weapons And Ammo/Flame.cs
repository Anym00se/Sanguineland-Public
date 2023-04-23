using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : Ammo
{
    public GameObject terrainFirePrefab;
    public float terrainFireDuration;
    [Tooltip("Per fixed update frame")]
    public float terrainFireDamage;


    public override void HandleCollision(GameObject other)
    {
        // Start fires when colliding with Terrain
        if (other.CompareTag("Terrain"))
        {
            Vector3 position = new Vector3(transform.position.x, other.transform.position.y, transform.position.z);
            GameObject terrainFire = Instantiate(terrainFirePrefab, position, Quaternion.identity);
            terrainFire.GetComponent<TerrainFire>().duration = terrainFireDuration;
            terrainFire.GetComponent<TerrainFire>().damage = terrainFireDamage;
            terrainFire.GetComponent<TerrainFire>().spawnedByObj = spawnedByObj;
        }
        else if (other.CompareTag("Environment"))
        {
            Destroy(gameObject);
        }

        // Don't destroy self when colliding with terrain fire
        if (!other.CompareTag("TerrainFire"))
        {
            base.HandleCollision(other);
        }
    }
}
