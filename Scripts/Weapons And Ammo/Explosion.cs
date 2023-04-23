using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public Transform sphere;
    public SphereCollider expCollider;
    public float damage;
    private float knockback = 20f;
    public float explosionRadius;
    public float explosionDuration = 0.2f;
    private float currentExplosionDurationLeft;
    private float sphereInitialSize = 0.1f;
    public Light expLight;
    public AnimationCurve animationCurve;
    public AnimationCurve damageFallOff;
    public bool allowFriendlyFire = true;
    public bool doDamageEnemies = true;
    public float lightIntensityMultiplier = 1f;
    public Color lightColor;
    public float lightRange = 20f;
    [SerializeField] private AudioSource sound;
    public GameObject spawnedByObj;


    void Awake()
    {
        sphere.localScale = Vector3.one * sphereInitialSize;
        SetCorrectColliderSize();
        currentExplosionDurationLeft = explosionDuration;
        expLight.intensity = 0f;
    }

    void Start()
    {
        CalculateDamage();
        expLight.color = lightColor;
        expLight.range = lightRange;

        if (sound)
        {
            PlaySound();
        }
    }

    void FixedUpdate()
    {
        // This is only animation. Damage is calculated in Start
        float sizeMultiplier = animationCurve.Evaluate(1f - currentExplosionDurationLeft / explosionDuration) * explosionRadius;
        sphere.localScale = Vector3.one * sizeMultiplier * 2f;
        SetCorrectColliderSize();
        expLight.intensity = (sphere.localScale.x / explosionRadius) * 5f * lightIntensityMultiplier;
        currentExplosionDurationLeft -= Time.fixedDeltaTime;

        // Destroy self when animation ends
        if (currentExplosionDurationLeft <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void SetCorrectColliderSize()
    {
        expCollider.radius = sphere.localScale.x / 2f;
    }

    void CalculateDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(hitCollider.gameObject.transform.position, transform.position);
            float scaledDamage = damage * damageFallOff.Evaluate(distance / explosionRadius);
            Vector3 knockbackVector = (hitCollider.transform.position - transform.position).normalized * (50f / distance) * knockback;

            // Case: Hit a Player
            if (hitCollider.gameObject.CompareTag("Player") && allowFriendlyFire && GameSyncer.instance.GetAllowFriendlyFire())
            {
                // Players take only 20% damage and half knockback
                Player player = hitCollider.gameObject.GetComponent<Player>();
                player.ReceiveKnockback(knockbackVector * 0.5f);
                player.ChangeHealth(-scaledDamage * 0.2f);
            }

            // Case: Hit an Enemy
            else if (doDamageEnemies && hitCollider.gameObject.CompareTag("Enemy"))
            {
                Enemy enemy = hitCollider.gameObject.GetComponent<Enemy>();
                enemy.ReceiveKnockback(knockbackVector);
                enemy.ChangeHealth(-scaledDamage, spawnedByObj);
            }
        }
    }

    public void PlaySound()
    {
        if (!GameSyncer.instance.IsPaused() && !sound.isPlaying)
        {
            sound.volume = Utils.GetSoundVolume();
            sound.pitch = Random.Range(0.9f, 1.1f);
            sound.Play();
        }
    }
}
