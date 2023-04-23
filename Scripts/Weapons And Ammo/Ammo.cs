using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    public float damage = 10f;
    public float knockback = 0f;
    public bool destroyOnCollision = true;
    public float lifeTime = 1f;
    protected float originalLifeTime;
    public bool hasInfiniteLifetime = false;
    public GameObject spawnedByObj;
    [SerializeField] private AudioSource sound;


    public virtual void Start()
    {
        originalLifeTime = lifeTime;

        // Play a sound if there is one
        if (sound)
        {
            PlaySound();
        }
    }


    public virtual void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f && !hasInfiniteLifetime)
        {
            Destroy(gameObject);
        }
    }

    public virtual void FixedUpdate()
    {
        if (transform.position.y < 0f)
        {
            Destroy(gameObject);
        }
    }

    public float GetDamage()
    {
        return damage;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject)
        {
            HandleCollision(collider.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject)
        {
            HandleCollision(collision.gameObject);
        }
    }

    public virtual void HandleCollision(GameObject other)
    {
        if (other != spawnedByObj && destroyOnCollision)
        {
            // Whitelist of tags of objects which can destroy ammo
            if (other.CompareTag("Enemy") || other.CompareTag("Player") || other.CompareTag("Terrain") || other.CompareTag("Environment"))
            {
                Destroy(gameObject);
            }
        }
    }

    public void PlaySound()
    {
        sound.volume = Utils.GetSoundVolume();
        sound.pitch = Random.Range(0.9f, 1.1f);
        sound.Play();
    }
}
