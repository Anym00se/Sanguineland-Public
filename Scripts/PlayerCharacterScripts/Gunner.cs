using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Gunner : Player
{
    private float normalShootTimer = 0.5f;
    private float superShootTimer = 0.1f;

    [SerializeField] private GameObject shotgunSmokePrefab;


    [PunRPC]
    public override void Shoot()
    {
        // Reduce ammo only if not using super weapon
        if (!isUsingSuperWeapon)
        {
            ammoLeft--;
        }

        // Spawn smoke effect
        Instantiate(shotgunSmokePrefab, ammoSpawn.position, ammoSpawn.rotation);

        // Shoot three pellets with a fixed spread
        float spread = 0.2f;
        float initialVelocity = 30f;

        GameObject ammoInstance1 = Instantiate(ammoPrefab, ammoSpawn.position, Quaternion.identity);
        GameObject ammoInstance2 = Instantiate(ammoPrefab, ammoSpawn.position, Quaternion.identity);
        GameObject ammoInstance3 = Instantiate(ammoPrefab, ammoSpawn.position, Quaternion.identity);

        ammoInstance1.GetComponent<Rigidbody>().AddForce((ammoSpawn.forward + ammoSpawn.right * spread) * initialVelocity, ForceMode.Impulse);
        ammoInstance2.GetComponent<Rigidbody>().AddForce(ammoSpawn.forward * initialVelocity, ForceMode.Impulse);
        ammoInstance3.GetComponent<Rigidbody>().AddForce((ammoSpawn.forward - ammoSpawn.right * spread) * initialVelocity, ForceMode.Impulse);

        ammoInstance1.GetComponent<Ammo>().spawnedByObj = gameObject;
        ammoInstance2.GetComponent<Ammo>().spawnedByObj = gameObject;
        ammoInstance3.GetComponent<Ammo>().spawnedByObj = gameObject;

        canShoot = false;
        shootTimer = GetShootTimer();

        PlayGunShotSound();
    }

    private float GetShootTimer()
    {
        return isUsingSuperWeapon ? superShootTimer : normalShootTimer;
    }
}
