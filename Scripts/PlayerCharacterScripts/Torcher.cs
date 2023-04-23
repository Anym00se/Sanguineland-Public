using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Torcher : Player
{
    private float launchVelocity = 20f;
    private float superLaunchVelocity = 50f;
    [SerializeField] private GameObject flameCirclePrefab;
    private float flameCircleCooldown = 0f;


    [PunRPC]
    public override void Shoot()
    {
        // Reduce ammo only if not using super weapon
        if (!isUsingSuperWeapon)
        {
            ammoLeft--;
        }

        // Shoot one meter further away for better user experience
        Vector3 targetCoord = GetSyncedMouseWorldPosition() + (GetSyncedMouseWorldPosition() - transform.position).normalized;

        // Rotate the ammoSpawn to the correct angle
        float launchAngle = -GetLaunchAngleToHitCoordinates(targetCoord, GetLaunchVelocity());
        ammoSpawn.localRotation = Quaternion.Euler(launchAngle, 0f, 0f);

        GameObject ammoInstance = Instantiate(ammoPrefab, ammoSpawn.position, Quaternion.identity);
        ammoInstance.GetComponent<Rigidbody>().AddForce(ammoSpawn.forward * GetLaunchVelocity() * ammoInstance.GetComponent<Rigidbody>().mass, ForceMode.Impulse);
        ammoInstance.GetComponent<Ammo>().spawnedByObj = gameObject;

        canShoot = false;
        shootTimer = 0.05f;

        PlayGunShotSound();
    }

    public float GetLaunchVelocity()
    {
        return isUsingSuperWeapon ? superLaunchVelocity : launchVelocity;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isUsingSuperWeapon)
        {
            flameCircleCooldown -= Time.fixedDeltaTime;
            if (flameCircleCooldown <= 0f)
            {
                SpawnFlameCircle();
                flameCircleCooldown = 0.75f;
            }
        }
    }

    void SpawnFlameCircle()
    {
        GameObject flameCircle = Instantiate(flameCirclePrefab, transform.position, Quaternion.identity);
        flameCircle.GetComponent<FlameCircle>().spawnedByObj = gameObject;
    }
}
