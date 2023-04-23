using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Laserer : Player
{
    private LineRenderer lineRenderer;
    public bool drawLaser = false;
    public float drawLaserTimer = 0f;
    private Vector3 laserEndPoint = Vector3.zero;
    private float laserDamage = 1.2f; // Per fixed update tick
    private float laserMaxDistance = 50f;
    private float laserKnockback = 30f;
    private float currentLaserDistance;
    private bool laserPenetratesTargets = false;
    [SerializeField] private GameObject laserParticleEffectPrefab;


    // Timers
    public float laserShootingTime = 1.5f;
    public float reloadTime = 1f;


    [PunRPC]
    public override void Shoot()
    {
        // Draw the laser
        GetLineRenderer().enabled = true;
        canShoot = isUsingSuperWeapon ? true : false; // Don't begin shooting while already shooting (unless using super weapon)
        drawLaser = true;
        drawLaserTimer = isUsingSuperWeapon ? Time.fixedDeltaTime*2f : laserShootingTime;

        PlayGunShotSound();
    }

    public override void Update()
    {
        base.Update();

        // Check where the laser stops
        Vector3 stopPosition = GetSyncedMouseWorldPosition(); // Not actually the stop position, but a position in the correct direction which is enough for this;
        stopPosition.y = ammoSpawn.position.y;
        Vector3 direction = (stopPosition - new Vector3(transform.position.x, ammoSpawn.position.y, transform.position.z)).normalized;
        GameObject nearestObj = GetNearest(GetObjectsInLaserPath(laserMaxDistance));

        // Case: no targets in front OR laser penetrates -> use maximum laser distance
        if (!nearestObj || laserPenetratesTargets)
        {
            currentLaserDistance = laserMaxDistance;
        }
        // Case: Stop at the closest object
        else
        {
            if (nearestObj)
            {
                stopPosition = nearestObj.transform.position;
                stopPosition.y = ammoSpawn.position.y;
            }

            currentLaserDistance = Vector3.Distance(stopPosition, ammoSpawn.position);
        }

        // Update laser visuals positions
        laserEndPoint = ammoSpawn.position + direction * currentLaserDistance;
        GetLineRenderer().SetPositions(new Vector3[2] {ammoSpawn.position, laserEndPoint});

        // Draw the laser
        GetLineRenderer().enabled = drawLaser;
    }

    private GameObject GetNearest(List<GameObject> objects)
    {
        GameObject nearestObj = null;
        float nearestDistance = Mathf.Infinity;

        foreach(GameObject obj in objects)
        {
            float objDistance = Vector3.Distance(obj.transform.position, ammoSpawn.position);
            if (objDistance < nearestDistance)
            {
                nearestDistance = objDistance;
                nearestObj = obj;
            }
        }

        return nearestObj;
    }

    private List<GameObject> GetObjectsInLaserPath(float maxDistance)
    {
        List<GameObject> objects = new List<GameObject>();

        if (drawLaser)
        {
            // Check if someone is in the Laser's way
            RaycastHit[] hits;
            hits = Physics.RaycastAll(GetTransformPositionAtLaserHeight(), ammoSpawn.forward, maxDistance + ammoSpawn.localPosition.z);

            foreach (RaycastHit hit in hits)
            {
                GameObject hitObj = hit.collider.gameObject;
                if (
                    hitObj && (
                        hitObj.CompareTag("Enemy") ||
                        hitObj.CompareTag("Player") ||
                        hitObj.CompareTag("Terrain") ||
                        hitObj.CompareTag("Environment")
                    )
                )
                {
                    objects.Add(hitObj);
                }
            }
        }

        return objects;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        drawLaserTimer -= Time.fixedDeltaTime;
        if (drawLaserTimer > 0f)
        {
            shootTimer = reloadTime;
        }
        else
        {
            drawLaserTimer = 0f;
            drawLaser = false;
        }

        // Inflict laser damage
        foreach (GameObject hitObj in GetObjectsInLaserPath(currentLaserDistance))
        {
            if (hitObj)
            {
                Vector3 knockbackVector = (hitObj.transform.position - transform.position).normalized * laserKnockback;

                if (hitObj.CompareTag("Player") && GameSyncer.instance.GetAllowFriendlyFire() && !isUsingSuperWeapon)
                {
                    Player player = hitObj.GetComponent<Player>();
                    if (player)
                    {
                        player.ReceiveKnockback(knockbackVector);
                        player.ChangeHealth(-laserDamage);
                    }
                }
                else if (hitObj.CompareTag("Enemy"))
                {
                    Enemy enemy = hitObj.GetComponent<Enemy>();
                    if (enemy)
                    {
                        enemy.ReceiveKnockback(knockbackVector);
                        enemy.ChangeHealth(-laserDamage, gameObject);
                    }
                }
            }
        }

        // Spawn spark effects to all hit points
        if (drawLaser)
        {
            foreach(Vector3 hitPosition in GetLaserHitpoints(currentLaserDistance))
            {
                GameObject particle = Instantiate(laserParticleEffectPrefab, hitPosition, Quaternion.identity);
                particle.transform.LookAt(transform.position);
            }
        }
    }

    private LineRenderer GetLineRenderer()
    {
        if (!lineRenderer)
        {
            lineRenderer = gameObject.GetComponent<LineRenderer>();
        }
        return lineRenderer;
    }

    [PunRPC]
    public override void SetSuperWeapon()
    {
        base.SetSuperWeapon();

        laserPenetratesTargets = true;
    }

    public override void SetNormalWeapon()
    {
        base.SetNormalWeapon();

        laserPenetratesTargets = false;
    }

    private List<Vector3> GetLaserHitpoints(float maxDistance)
    {
        List<Vector3> positions = new List<Vector3>();

        // Check if someone is in the Laser's way
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ammoSpawn.position, ammoSpawn.forward, maxDistance);

        foreach (RaycastHit hit in hits)
        {
            GameObject hitObj = hit.collider.gameObject;
            if (
                hitObj && (
                    hitObj.CompareTag("Enemy") ||
                    hitObj.CompareTag("Player") ||
                    hitObj.CompareTag("Terrain") ||
                    hitObj.CompareTag("Environment")
                )
            )
            {
                positions.Add(hit.point);
            }
        }

        return positions;
    }

    private Vector3 GetTransformPositionAtLaserHeight()
    {
        return new Vector3(transform.position.x, ammoSpawn.position.y, transform.position.z);
    }
}
