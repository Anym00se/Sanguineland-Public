using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bombardier : Player
{
    public float cannonMaxReach = 20f; // meters
    private float launchAngle = -45f;
    public GameObject ammoPrefabRocket;

    [PunRPC]
    public override void Shoot()
    {
        if (isUsingSuperWeapon)
        {
            ShootRocket();
            canShoot = false;
            shootTimer = 0.15f;
        }
        else
        {
            ammoLeft--;

            // Calculate the velocity required to reach cannonMaxReach
            float maxVelocity = Utils.GetLaunchVelocityToReachCoordinates(Vector3.zero, Vector3.forward * cannonMaxReach, launchAngle);

            // Calculate the required velocity to reach mouse position
            Vector3 targetCoordinates = GetSyncedMouseWorldPosition();
            float initialVelocity = Utils.GetLaunchVelocityToReachCoordinates(ammoSpawn.position, targetCoordinates, launchAngle);

            // Rotate the ammoSpawn to the correct angle
            ammoSpawn.localRotation = Quaternion.Euler(launchAngle, 0f, 0f);

            // Clamp the velocity
            initialVelocity = Mathf.Clamp(initialVelocity, 1f, maxVelocity);

            GameObject ammoInstance = Instantiate(ammoPrefab, ammoSpawn.position, Quaternion.identity);
            ammoInstance.GetComponent<Rigidbody>().AddForce(ammoSpawn.forward * initialVelocity * ammoInstance.GetComponent<Rigidbody>().mass, ForceMode.Impulse);
            ammoInstance.GetComponent<Ammo>().spawnedByObj = gameObject;

            canShoot = false;
            shootTimer = 1f;

            PlayGunShotSound();
        }
    }

    private void ShootRocket()
    {
        GameObject ammoInstance = Instantiate(ammoPrefabRocket, transform.position + Vector3.up * 25f, Quaternion.identity);
        ammoInstance.GetComponent<Rocket>().spawnedByObj = gameObject;
        ammoInstance.GetComponent<Rocket>().targetPosition = GetSyncedMouseWorldPosition();
    }
}
