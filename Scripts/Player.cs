using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Technical")]
    public float movementSpeed = 6f;
    public Camera playerCamera;
    public Transform meshObjTransform;
    public Transform ammoSpawn;
    public HPBar hpBar;
    [SerializeField] private GameObject playerHUD;
    public GameObject ammoPrefab;
    public GameObject mousePositionMarker;
    private Vector3 previousPosition = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private int currentFrameNumber = 0;
    public bool isDead = false;
    public bool isBeingRevived = false;
    public float reviveProgress = 0f;
    public float reviveTime = 4f;
    public bool deathAnimationCompleted = false;
    private bool loadingMenu = false; // Set to true when going back to Main Menu to avoid spamming level change
    public Player mostRecentRevivee = null;

    [Header("Audio")]
    [SerializeField] private AudioSource gunShotSound;
    [SerializeField] private AudioSource hpPickupSound;
    [SerializeField] private AudioSource ammoPickupSound;
    [SerializeField] private AudioSource superWeaponPickupSound;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private AudioSource reviveSound;
    [SerializeField] private AudioSource meleeAttackSound;

    private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject deadEffect;
    [SerializeField] private GameObject reviveEffect;
    
    private Vector3 movementVectorByInput = Vector3.zero;

    [Header("Shooting")]
    protected bool canShoot = true;
    private bool doShoot = false; // Fire1 input pressed
    public float shootTimer = 0f;
    public float superWeaponTimeLeft;
    public float superWeaponDuration = 5f;

    [Header("Player Stats")]
    public float health;
    public float maxHealth = 100f;
    public bool isUsingSuperWeapon = false;
    public int ammoLeft;
    public int maxAmmo = 30;
    private float meleeDamage = 10f;
   
    [Header("Stat tracking")]
    private int zombiesKilled = 0;
    private int timesDied = 0;
    private int timesRevived = 0;
    private int numberOfSuperWeapons = 0;


    public virtual void Awake()
    {
        health = maxHealth;
        rb = gameObject.GetComponent<Rigidbody>();
        ammoLeft = maxAmmo;
        
        // Photon
        object[] instantiationData = photonView.InstantiationData;
        playerCamera.enabled = PhotonViewIsMine();
        gameObject.GetComponent<AudioListener>().enabled = PhotonViewIsMine();

        if (PhotonViewIsMine())
        {
            photonView.Owner.NickName = PlayerPrefs.GetString("PlayerName", "Player");
            hpBar.gameObject.SetActive(false);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Owner
        if (stream.IsWriting && PhotonViewIsMine())
        {
            stream.SendNext(zombiesKilled);
            stream.SendNext(timesDied);
            stream.SendNext(timesRevived);
            stream.SendNext(numberOfSuperWeapons);
            stream.SendNext(playerVelocity);
        }
        // Other clients
        else if (stream.IsReading)
        {
            zombiesKilled = (int)stream.ReceiveNext();
            timesDied = (int)stream.ReceiveNext();
            timesRevived = (int)stream.ReceiveNext();
            numberOfSuperWeapons = (int)stream.ReceiveNext();
            playerVelocity = (Vector3)stream.ReceiveNext();
        }
    }

    public virtual void FixedUpdate()
    {
        // Go back to Main Menu when losing connection to the GameSyncer
        // Master client destroys the GameSyncer when leaving the Room
        if (PhotonViewIsMine() && GameSyncer.instance == null && !loadingMenu)
        {
            loadingMenu = true;
            NetworkManager.instance.LoadLevel("Main Menu");
        }

        currentFrameNumber++;
        previousPosition = rb.position;

        if (PhotonViewIsMine() && !isDead && IsGameOnGoing())
        {

            // Move
            rb.MovePosition(rb.position + movementVectorByInput * Time.fixedDeltaTime);

            // Shooting
            if (doShoot && canShoot)
            {
                if (ammoLeft > 0)
                {
                    // When Player is stationary, show a unique shooting animation so that it won't look so stupid
                    animator.SetBool("DoIdleShoot", playerVelocity.sqrMagnitude < 0.1f);

                    photonView.RPC("Shoot", RpcTarget.All);
                }
                else
                {
                    photonView.RPC("PerformMeleeAttack", RpcTarget.All);
                }
            }
            else
            {
                animator.SetBool("DoIdleShoot", false);
            }
        }

        if (isDead)
        {
            if (!deathAnimationCompleted && HasAnimationEnded("Death"))
            {
                deathAnimationCompleted = true;
            }
        }

        if (PhotonViewIsMine())
        {
            playerVelocity = (rb.position - previousPosition) / Time.fixedDeltaTime;
        }
        float velocityMagnitude = playerVelocity.magnitude;

        animator.SetFloat("Velocity", velocityMagnitude * GetPlayerRunningDirection());

        // Fix running animation speed
        if (velocityMagnitude > 0.05)
        {
            animator.speed = movementSpeed / 2f;
        }
        else
        {
            animator.speed = 1f;
        }

        KillIfOutsideBounds();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (PhotonViewIsMine())
        {
            if (Input.GetButtonDown("Pause"))
            {
                GameSyncer.instance.TogglePause();
            }

            playerHUD.SetActive(IsGameOnGoing());
            hpBar.gameObject.SetActive(false);
        }
        else
        {
            playerHUD.SetActive(false);
            hpBar.gameObject.SetActive(IsGameOnGoing());
        }

        if (PhotonViewIsMine() && IsGameOnGoing())
        {
            // Get the movement vector
            // =======================

            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            movementVectorByInput = new Vector3(
                movementSpeed * input.x,
                0f,
                movementSpeed * input.y
            );
            // Clamp the vector length
            movementVectorByInput = Vector3.ClampMagnitude(movementVectorByInput, movementSpeed);

            // Rotation (only rotate the mesh, as the Camera is attached to the root)
            // ======================================================================

            Vector3 mouseWorldPos = GetMouseWorldPosition();

            // Rotate
            if (!isDead)
            {
                meshObjTransform.transform.LookAt(new Vector3(mouseWorldPos.x, meshObjTransform.transform.position.y, mouseWorldPos.z));
            }

            // Shooting
            // ========
            doShoot = Input.GetButton("Fire1") && !GameSyncer.instance.IsPointerOverUIObject();

            mousePositionMarker.transform.position = GetMouseWorldPosition();
            SetMousePositionMarkerVisibility(true);
        }
        else
        {
            SetMousePositionMarkerVisibility(false);
        }

        if (IsGameOnGoing())
        {
            // Reduce shoot timer
            if (shootTimer > 0f)
            {
                shootTimer -= Time.deltaTime;
            }

            // Can shoot when timer has run out
            if (shootTimer <= 0)
            {
                canShoot = true;
            }

            // Reduce super weapon timer
            if (isUsingSuperWeapon)
            {
                superWeaponTimeLeft -= Time.deltaTime;

                if (superWeaponTimeLeft <= 0)
                {
                    SetNormalWeapon();
                }
            }

            if (isDead && isBeingRevived)
            {
                // Play resurrect animation
                animator.SetTrigger("IsBeingRevived");
                animator.speed = 0f;
                animator.Play("Revive", 0, reviveProgress / reviveTime);

                reviveProgress += Time.deltaTime;

                // Resurrection
                if (reviveProgress >= reviveTime)
                {
                    photonView.RPC("UnDie", RpcTarget.AllBuffered);
                }
            }
            // Reset the revive progress when reviving is cancelled for any reason
            else
            {
                reviveProgress = 0f;
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        return Utils.GetYZeroPositionByRay(
            playerCamera.ScreenPointToRay(Input.mousePosition)
        );
    }

    public Vector3 GetSyncedMouseWorldPosition()
    {
        return mousePositionMarker.transform.position;
    }
    
    public bool PhotonViewIsMine()
    {
        return photonView.IsMine;
    }

    [PunRPC]
    public virtual void Shoot()
    {
        // This is a template function of what generally should happen.
        // Override this function in the character's subclass.
        ammoLeft--;
        GameObject ammoInstance = Instantiate(ammoPrefab, ammoSpawn.position, Quaternion.identity);
        ammoInstance.GetComponent<Rigidbody>().AddForce(ammoSpawn.forward * 30f, ForceMode.Impulse);
        ammoInstance.GetComponent<Ammo>().spawnedByObj = gameObject;
    }

    [PunRPC]
    public void ChangeHealthRPC(float amount)
    {
        health += amount;
        if (health <= 0f)
        {
            health = 0f;

            if (PhotonViewIsMine())
            {
                photonView.RPC("Die", RpcTarget.AllBuffered);
            }
        }
        else if (health >= maxHealth)
        {
            health = maxHealth;
        }

        // Update HP bar if not local client.
        // Locally the PlayerHUD script handles the updating.
        if (!PhotonViewIsMine())
        {
            hpBar.UpdateHP(health, maxHealth);
        }
    }

    public void ChangeHealth(float amount, bool forceRPC = false)
    {
        if (PhotonViewIsMine() || forceRPC)
        {
            photonView.RPC("ChangeHealthRPC", RpcTarget.AllBuffered, amount);
        }
    }

    [PunRPC]
    public virtual void Die()
    {
        if (!isDead)
        {
            isDead = true;
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Death");
            gameObject.GetComponent<Collider>().enabled = false;
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezePosition;
            rb.useGravity = false;
            canShoot = false;
            doShoot = false;
            superWeaponTimeLeft = 0f;
            shootTimer = 99999f;

            deadEffect.SetActive(true);

            IncreaseDeaths();

            PlayDeathSound();
        }
    }

    [PunRPC]
    public virtual void UnDie()
    {
        deathAnimationCompleted = false;
        isDead = false;
        animator.SetBool("IsDead", false);
        ToggleIsBeingRevived(false);
        reviveProgress = 0f;

        if (PhotonViewIsMine())
        {
            ChangeHealth(maxHealth * 0.75f);
        }

        // Undo death stuff
        gameObject.GetComponent<Collider>().enabled = true;
        rb.constraints = ~RigidbodyConstraints.FreezePosition;
        rb.useGravity = true;
        canShoot = true;
        doShoot = false;
        superWeaponTimeLeft = 0f;
        shootTimer = 0f;

        if (mostRecentRevivee)
        {
            mostRecentRevivee.photonView.RPC("IncreaseRevives", RpcTarget.AllBuffered);
        }

        PlayReviveSound();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject)
        {
            HandleCollision(collision.gameObject);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject)
        {
            HandleCollision(collider.gameObject);
        }
    }

    void HandleCollision(GameObject other)
    {
        if (PhotonViewIsMine())
        {
            // Case: being shot at
            if (other.CompareTag("Ammo"))
            {
                Ammo ammoScript = other.GetComponent<Ammo>();
                if (ammoScript.spawnedByObj != gameObject)
                {
                    // Take damage if shot by another player and friendly fire is on (and the player is not using super weapon) OR shot by some enemy
                    if (
                        (ammoScript.spawnedByObj && ammoScript.spawnedByObj.CompareTag("Player") && GameSyncer.instance.GetAllowFriendlyFire() && !ammoScript.spawnedByObj.GetComponent<Player>().isUsingSuperWeapon) ||
                        (ammoScript.spawnedByObj && !ammoScript.spawnedByObj.CompareTag("Player")) ||
                        (!ammoScript.spawnedByObj)
                    )
                    {
                        ReceiveKnockback((other.gameObject.GetComponent<Rigidbody>().velocity).normalized * 50f * ammoScript.knockback);
                        float amount = -ammoScript.GetDamage();
                        ChangeHealth(amount);
                    }
                }
            }
            // Case: picked up HP
            else if (other.CompareTag("HPPickup"))
            {
                // Pick up the HP pickup only if not at max hp
                if (health < maxHealth)
                {
                    ChangeHealth(25f);
                    GameSyncer.instance.PhotonDestroy(other);

                    PlayHPPickupSound();
                }
            }
            // Case: picked up more Ammo
            else if (other.CompareTag("AmmoBox"))
            {
                // Collect the Ammo box only if not already at max ammo
                if (ammoLeft < maxAmmo)
                {
                    // Add ammo to Player
                    ammoLeft += Mathf.RoundToInt(maxAmmo / 4);
                    if (ammoLeft > maxAmmo)
                    {
                        ammoLeft = maxAmmo;
                    }
                    GameSyncer.instance.PhotonDestroy(other);

                    PlayAmmoPickupSound();
                }
            }
            // Case: picked up a Super Weapon
            else if (other.CompareTag("SuperWeapon"))
            {
                if (!isUsingSuperWeapon)
                {
                    // Change Player Weapon to Super Weapon
                    photonView.RPC("SetSuperWeapon", RpcTarget.AllBuffered);
                    GameSyncer.instance.PhotonDestroy(other);

                    PlaySuperWeaponPickupSound();
                }
            }
        }
    }

    protected float GetLaunchAngleToHitCoordinates(Vector3 targetCoordinates, float initialVelocity)
    {
        float launchAngle = Utils.GetMinAngle(
            Utils.GetAnglesToHitCoordinate(ammoSpawn.position, targetCoordinates, initialVelocity)
        );

        // Probably due to imaginary root
        if (launchAngle == 0f)
        {
            launchAngle = 45f;
        }

        return launchAngle;
    }

    void SetMousePositionMarkerVisibility(bool newVisibility)
    {
        mousePositionMarker.transform.GetChild(0).gameObject.SetActive(newVisibility);
    }

    [PunRPC]
    public virtual void SetSuperWeapon()
    {
        isUsingSuperWeapon = true;
        superWeaponTimeLeft = superWeaponDuration;
        shootTimer = 0f;

        // Also fill ammo
        ammoLeft = maxAmmo;

        IncreaseSuperWeaponPickups();
    }

    [PunRPC]
    public virtual void SetNormalWeapon()
    {
        isUsingSuperWeapon = false;
        superWeaponTimeLeft = 0f;
    }

    public Vector3 GetVelocity()
    {
        return playerVelocity;
    }

    public void ReceiveKnockback(Vector3 knockbackImpulse)
    {
        rb.AddForce(knockbackImpulse, ForceMode.Impulse);
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

    // Melee Attack doesn't inflict damage to friendlies
    [PunRPC]
    public void PerformMeleeAttack()
    {
        Vector3 center = transform.position + meshObjTransform.up + meshObjTransform.forward * 0.9f;
        float radius = 0.7f;

        // Inflict damage to all enemies inside a sphere
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        foreach (Collider hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.gameObject.GetComponent<Enemy>();
            if (enemy)
            {
                enemy.ReceiveKnockback((enemy.transform.position - transform.position).normalized * 500f);
                enemy.ChangeHealth(-meleeDamage, gameObject);
            }
        }

        canShoot = false;
        shootTimer = 0.8f;

        animator.SetTrigger("MeleeAttack");

        PlayMeleeAttackSound();
    }

    // Returns 1.0f if running forwards, -1.0f if backwards
    private float GetPlayerRunningDirection()
    {
        return Mathf.Sign(Vector3.Dot(meshObjTransform.forward, playerVelocity));
    }

    public string GetPhotonNickname()
    {
        return photonView.Owner.NickName;
    }

    public void ToggleIsBeingRevived(bool toggle)
    {
        animator.SetBool("IsBeingRevived", toggle);
        isBeingRevived = toggle;

        // Enable and disable particle systems
        reviveEffect.SetActive(toggle);
        deadEffect.SetActive(!toggle && isDead);

        // Case: dead
        if (isDead && !toggle)
        {
            animator.Play("Death", 0, 1f);
        }
    }

    public bool HasAnimationEnded(string animationName)
    {
        if (
            animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) && 
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
        )
        {
            return true;
        }

        return false;
    }

    protected void PlayGunShotSound()
    {
        // Play the sound only if not existing gunshot sound already playing (except for Gunners and Bombardiers)
        if (
            IsGameOnGoing() &&
            (!gunShotSound.isPlaying || gameObject.GetComponent<Gunner>() || gameObject.GetComponent<Bombardier>())
        )
        {
            // Play gunshots with reduced volume as they spawn right next to the listener
            gunShotSound.Stop();
            gunShotSound.volume = Utils.GetSoundVolume() * 0.3f;
            gunShotSound.pitch = Random.Range(0.98f, 1.02f);
            gunShotSound.Play();
        }
    }

    private bool IsGameOnGoing()
    {
        return
            !GameSyncer.instance.IsPaused() &&
            GameSyncer.instance.HasGameStarted() &&
            !GameSyncer.instance.HasGameEnded();
    }

    public int GetKills()
    {
        return zombiesKilled;
    }
    public int GetDeaths()
    {
        return timesDied;
    }
    public int GetRevives()
    {
        return timesRevived;
    }
    public int GetSuperWeapons()
    {
        return numberOfSuperWeapons;
    }

    [PunRPC]
    public void IncreaseKillsRPC()
    {
        if (PhotonViewIsMine())
        {
            zombiesKilled++;
        }
    }

    private void IncreaseDeaths()
    {
        if (PhotonViewIsMine())
        {
            timesDied++;
        }
    }

    [PunRPC]
    public void IncreaseRevives()
    {
        if (PhotonViewIsMine())
        {
            timesRevived++;
        }
    }

    private void IncreaseSuperWeaponPickups()
    {
        if (PhotonViewIsMine())
        {
            numberOfSuperWeapons++;
        }
    }

    private void PlayHPPickupSound()
    {
        if (IsGameOnGoing())
        {
            hpPickupSound.Stop();
            hpPickupSound.volume = Utils.GetSoundVolume();
            hpPickupSound.Play();
        }
    }

    private void PlayAmmoPickupSound()
    {
        if (IsGameOnGoing())
        {
            // Play with reduced volume as it's a loud sound
            ammoPickupSound.Stop();
            ammoPickupSound.volume = Utils.GetSoundVolume() * 0.5f;
            ammoPickupSound.Play();
        }
    }

    private void PlaySuperWeaponPickupSound()
    {
        if (IsGameOnGoing())
        {
            superWeaponPickupSound.Stop();
            superWeaponPickupSound.volume = Utils.GetSoundVolume();
            superWeaponPickupSound.Play();
        }
    }

    private void PlayDeathSound()
    {
        if (IsGameOnGoing())
        {
            deathSound.Stop();
            deathSound.volume = Utils.GetSoundVolume();
            deathSound.Play();
        }
    }

    private void PlayReviveSound()
    {
        if (IsGameOnGoing())
        {
            reviveSound.Stop();
            reviveSound.volume = Utils.GetSoundVolume();
            reviveSound.Play();
        }
    }

    private void PlayMeleeAttackSound()
    {
        if (IsGameOnGoing())
        {
            meleeAttackSound.Stop();
            meleeAttackSound.volume = Utils.GetSoundVolume();
            meleeAttackSound.Play();
        }
    }
}
