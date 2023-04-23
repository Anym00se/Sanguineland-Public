using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFire : MonoBehaviour
{
    public float duration;
    public float damage;
    public SphereCollider terrainFireCollider;
    [SerializeField] private AudioSource sound;
    public GameObject spawnedByObj;


    void Start()
    {
        duration += Random.Range(-0.2f, 0.2f);
        PlaySound();
    }

    void FixedUpdate()
    {
        CalculateDamage();

        // Destroy self after duration has passed
        duration -= Time.fixedDeltaTime;
        if (duration <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void CalculateDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, terrainFireCollider.radius);
        foreach (Collider hitCollider in hitColliders)
        {
            // Case: Hit a Player
            if (
                hitCollider.gameObject.CompareTag("Player") &&
                GameSyncer.instance.GetAllowFriendlyFire() &&
                !hitCollider.gameObject.GetComponent<Player>().isUsingSuperWeapon
            )
            {
                hitCollider.gameObject.GetComponent<Player>().ChangeHealth(-damage);
            }

            // Case: Hit an Enemy
            else if (hitCollider.gameObject.CompareTag("Enemy"))
            {
                hitCollider.gameObject.GetComponent<Enemy>().ChangeHealth(-damage, spawnedByObj);
            }
        }
    }

    public void PlaySound()
    {
        if (!GameSyncer.instance.IsPaused() && !sound.isPlaying)
        {
            // Play terrain fire sounds with reduced volume as there are usually many and they are not important sounds
            sound.volume = Utils.GetSoundVolume() * 0.1f;
            sound.pitch = Random.Range(0.9f, 1.1f);
            sound.Play();
        }
    }
}
