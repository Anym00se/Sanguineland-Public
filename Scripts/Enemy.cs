using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

public class Enemy : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Technical")]
    private NavMeshAgent navmeshAgent;
    private Rigidbody rb;
    public HPBar hpBar;
    [SerializeField] private Animator animator;
    public GameObject nearestPlayer = null; // Public so that it doesn't need to be calculated more than once per frame
    private float destroyTimer = 2f;
    [SerializeField] private AudioSource zombieGrowl;
    [SerializeField] GameObject spitBallPrefab;
    [SerializeField] private Transform spitBallSpawn;
    public Player mostRecentDamager = null; // The Player who has most recently inflicted damage (is the killer when the Enemy dies)
    private float agentVelocity = 0f;

    [Header("Enemy Stats")]
    private float health;
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] [Range(0f, 1f)] float ammoSpawnOdds = 0.4f;
    [SerializeField] [Range(0f, 1f)] float healthSpawnOdds = 0.25f;
    [SerializeField] [Range(0f, 1f)] float superWeaponSpawnOdds = 0.8f;
    private float originalSpeed;
    private float originalAngularSpeed;
    private float originalAcceleration;

    [Header("Attacking")]
    protected bool canAttack = true;
    protected float attackTimer = 0f;
    [SerializeField] protected float maxAttackTimer = 1.5f;
    [SerializeField] protected float attackingDistance = 1f;
    public float attackDamage = 5f;
    public float attackKnockback = 3f;
    public bool hasSpitAttack = false;
    public bool hasChargeAttack = false;
    private bool isCharging = false;
    private Vector3 chargeDestination = Vector3.zero;


    void Awake()
    {
        navmeshAgent = gameObject.GetComponent<NavMeshAgent>();
        health = maxHealth;

        if (photonView.IsMine)
        {
            // Update Enemy count
            GameSyncer.instance.enemiesSpawnedThisWave++;
        }
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        hpBar.UpdateHP(health, maxHealth);
        originalSpeed = GetNavMeshAgent().speed;
        originalAngularSpeed = GetNavMeshAgent().angularSpeed;
        originalAcceleration = GetNavMeshAgent().acceleration;

        // Play sound for the biggest creatures at spawn
        if (maxHealth > 200 && zombieGrowl)
        {
            PlayGrowl();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Owner
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(agentVelocity);
        }
        // Other clients
        else if (stream.IsReading)
        {
            agentVelocity = (float)stream.ReceiveNext();
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            agentVelocity = navmeshAgent.velocity.magnitude;
        }
        bool isWalking = agentVelocity > 0;
        animator.SetBool("IsWalking", isWalking);
        animator.speed = isWalking ? agentVelocity : 1f;

        // Play sound randomly
        // Chargers and Spitters growl only when attacking
        if ((!hasChargeAttack && !hasSpitAttack) && Random.Range(0f, 10000f) > 9999f)
        {
            PlayGrowl();
        }

        KillIfOutsideBounds();
    }

    void FixedUpdate()
    {
        // Enemies need to know their nearest Player on all clients due to some functions
        nearestPlayer = GetNearestPlayer();

        if (photonView.IsMine)
        {
            if (nearestPlayer && !isCharging)
            {
                SetNavMeshAgentDestination(nearestPlayer.transform.position);
            }

            // Reduce the attack timer counter each frame
            if (!canAttack && !isCharging)
            {
                attackTimer -= Time.fixedDeltaTime;

                if (attackTimer <= 0)
                {
                    attackTimer = 0f;
                    canAttack = true;
                    navmeshAgent.isStopped = false;
                }
            }

            // Attacking
            // =========
            if (canAttack && IsWithinAttackingDistance() && !isCharging)
            {
                Attack();
            }

            // When dead, wait for death animation end
            if (health <= 0)
            {
                destroyTimer -= Time.fixedDeltaTime;

                // Destroy the object
                if (destroyTimer <= 0)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }

            // Charging
            if (isCharging && Vector3.Distance(transform.position, chargeDestination) < 1.5f)
            {
                photonView.RPC("StopCharge", RpcTarget.AllBuffered);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject)
        {
            HandleCollision(other.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject)
        {
            HandleCollision(collision.gameObject);
        }
    }

    void HandleCollision(GameObject other)
    {
        if (photonView.IsMine)
        {
            // Take damage from Ammo
            if (other.CompareTag("Ammo"))
            {
                Ammo ammoScript = other.GetComponent<Ammo>();
                // Case: NOT created by self or an ally
                if (ammoScript.spawnedByObj != gameObject && !(ammoScript.spawnedByObj && ammoScript.spawnedByObj.CompareTag("Enemy")))
                {
                    ReceiveKnockback((other.gameObject.GetComponent<Rigidbody>().velocity).normalized * 50f * ammoScript.knockback);
                    float amount = -ammoScript.GetDamage();
                    ChangeHealth(amount, ammoScript.spawnedByObj);
                }
            }
            // Cause damage to Players when charging
            if (hasChargeAttack && isCharging && other.CompareTag("Player"))
            {
                Player playerScript = other.GetComponent<Player>();
                playerScript.ReceiveKnockback(transform.forward * 50f * attackKnockback);
                playerScript.ChangeHealth(-attackDamage, true);

                photonView.RPC("StopCharge", RpcTarget.AllBuffered);
            }
        }
    }

    public NavMeshAgent GetNavMeshAgent()
    {
        return navmeshAgent;
    }

    public void SetNavMeshAgentDestination(Vector3 position)
    {
        navmeshAgent.destination = position;
    }

    public GameObject GetNearestPlayer()
    {
        GameObject nearestPlayer = null;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float nearestDistance = Mathf.Infinity;
        foreach(GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            Player playerScript = player.GetComponent<Player>();
            if (distance < nearestDistance && !playerScript.isDead)
            {
                nearestDistance = distance;
                nearestPlayer = player;
            }
        }

        return nearestPlayer;
    }

    [PunRPC]
    public void ChangeHealthRPC(float amount)
    {
        health += amount;
        if (health <= 0f)
        {
            health = 0f;

            if (photonView.IsMine)
            {
                photonView.RPC("Die", RpcTarget.AllBuffered);
            }
        }
        else if (health >= maxHealth)
        {
            health = maxHealth;
        }
        hpBar.UpdateHP(health, maxHealth);
    }

    public void ChangeHealth(float amount, GameObject damager)
    {
        if (photonView.IsMine)
        {
            if (damager && damager.GetComponent<Player>())
            {
                mostRecentDamager = damager.GetComponent<Player>();
            }

            photonView.RPC("ChangeHealthRPC", RpcTarget.AllBuffered, amount);
        }
    }

    [PunRPC]
    public void Die()
    {
        if (photonView.IsMine)
        {
            // Update Enemy count
            GameSyncer.instance.enemiesDestroyedThisWave++;

            if (mostRecentDamager)
            {
                mostRecentDamager.photonView.RPC("IncreaseKillsRPC", RpcTarget.AllBuffered);
            }
        }

        // A chance to spawn an AmmoBox or an HP_Pickup or a SuperWeapon pickup when dying
        if (photonView.IsMine)
        {
            // Case: Spawn Super Weapon pickup
            if (Random.Range(0f, 1f) <= superWeaponSpawnOdds)
            {
                PhotonNetwork.Instantiate("Drops/SuperWeapon", transform.position, Quaternion.identity);
            }
            else
            {
                // Case: Spawn Ammo Box
                if (Random.Range(0f, 1f) > 0.5f)
                {
                    if (Random.Range(0f, 1f) <= ammoSpawnOdds)
                    {
                        PhotonNetwork.Instantiate("Drops/AmmoBox", transform.position, Quaternion.identity);
                    }
                }
                // Case: Spawn HP_Pickup
                else
                {
                    if (Random.Range(0f, 1f) <= healthSpawnOdds)
                    {
                        PhotonNetwork.Instantiate("Drops/HP_Pickup", transform.position, Quaternion.identity);
                    }
                }
            }
        }

        // Play death animation
        animator.SetTrigger("Death");

        // Make sure the Enemy won't affect the gameplay anymore
        gameObject.GetComponent<Collider>().enabled = false;
        canAttack = false;
        attackTimer = 999f;
        attackKnockback = 0f;
        attackDamage = 0f;
        navmeshAgent.isStopped = true;
        navmeshAgent.speed = 0f;

        hpBar.gameObject.SetActive(false);

        PlayGrowl();
    }

    public void Attack()
    {
        canAttack = false;
        attackTimer = maxAttackTimer;

        if (!hasChargeAttack)
        {
            navmeshAgent.isStopped = true;
        }

        if (nearestPlayer)
        {
            Player player = nearestPlayer.GetComponent<Player>();

            // Case: Melee attacks
            if (!hasChargeAttack && !hasSpitAttack)
            {
                PlayGrowl();

                // Cause knockback
                player.ReceiveKnockback((nearestPlayer.transform.position - transform.position).normalized * 50f * attackKnockback);

                // Inflict damage
                player.ChangeHealth(-attackDamage, true);
            }
            // Case: Charge attacks
            else if (hasChargeAttack)
            {
                photonView.RPC("Charge", RpcTarget.AllBuffered);
            }
            // Case: Spit attacks
            else if (hasSpitAttack)
            {
                photonView.RPC("Spit", RpcTarget.AllBuffered);
            }
        }

        photonView.RPC("TriggerAttackAnimation", RpcTarget.AllBuffered);
    }

    public void ReceiveKnockback(Vector3 knockbackImpulse)
    {
        rb.AddForce(knockbackImpulse, ForceMode.Impulse);
    }

    private bool IsWithinAttackingDistance()
    {
        return nearestPlayer && Vector3.Distance(transform.position, nearestPlayer.transform.position) <= attackingDistance;
    }

    private void KillIfOutsideBounds()
    {
        if (photonView.IsMine)
        {
            Vector3 mapLimits = new Vector3(100, 5, 100);
            Vector3 pos = transform.position;
            if (
                pos.x < -mapLimits.x || pos.x > mapLimits.x ||
                pos.y < -mapLimits.y || pos.y > mapLimits.y ||
                pos.z < -mapLimits.z || pos.z > mapLimits.z
            )
            {
                photonView.RPC("Die", RpcTarget.AllBuffered);
            }
        }
    }

    protected void PlayGrowl()
    {
        if (!GameSyncer.instance.IsPaused() && !zombieGrowl.isPlaying)
        {
            zombieGrowl.volume = Utils.GetSoundVolume();
            zombieGrowl.pitch = Random.Range(0.8f, 1.2f);
            zombieGrowl.Play();
        }
    }

    [PunRPC]
    public void Charge()
    {
        if (photonView.IsMine)
        {
            chargeDestination = SetChargeDestination();
            GetNavMeshAgent().destination = chargeDestination;

            transform.LookAt(nearestPlayer.transform.position);

            GetNavMeshAgent().speed = 5f;
            GetNavMeshAgent().angularSpeed = 200f;
            GetNavMeshAgent().acceleration = 10f;
        }

        isCharging = true;

        PlayGrowl();
    }

    [PunRPC]
    public void StopCharge()
    {
        if (photonView.IsMine)
        {
            GetNavMeshAgent().speed = originalSpeed;
            GetNavMeshAgent().angularSpeed = originalAngularSpeed;
            GetNavMeshAgent().acceleration = originalAcceleration;
        }

        isCharging = false;
    }

    private Vector3 SetChargeDestination()
    {
        Vector3 chargeDestination = Vector3.zero;
        if (nearestPlayer)
        {
            chargeDestination = transform.position + (nearestPlayer.transform.position - transform.position).normalized * 10f;
        }

        return chargeDestination;
    }

    [PunRPC]
    public void Spit()
    {
        float launchAngle = -45f;
        float spitMaxReach = 100f;

        transform.LookAt(nearestPlayer.transform.position);

        // Calculate the velocity required to reach spitMaxReach
        float maxVelocity = Utils.GetLaunchVelocityToReachCoordinates(Vector3.zero, Vector3.forward * spitMaxReach, launchAngle);

        // Calculate the required velocity to reach mouse position
        Vector3 targetCoordinates = nearestPlayer.transform.position;
        float initialVelocity = Utils.GetLaunchVelocityToReachCoordinates(spitBallSpawn.position, targetCoordinates, launchAngle);

        // Rotate the spitBallSpawn to the correct angle
        spitBallSpawn.localRotation = Quaternion.Euler(launchAngle, 0f, 0f);

        // Clamp the velocity
        initialVelocity = Mathf.Clamp(initialVelocity, 1f, maxVelocity);

        GameObject spitBall = Instantiate(spitBallPrefab, spitBallSpawn.position, Quaternion.identity);
        spitBall.GetComponent<Rigidbody>().AddForce(spitBallSpawn.forward * initialVelocity * spitBall.GetComponent<Rigidbody>().mass, ForceMode.Impulse);
        spitBall.GetComponent<Ammo>().spawnedByObj = gameObject;

        PlayGrowl();
    }

    [PunRPC]
    public void TriggerAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }
}
