using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveTriggerHandler : MonoBehaviour
{
    [SerializeField] private Player playerScript;
    private float reviveRadius;

    // Start is called before the first frame update
    void Start()
    {
        reviveRadius = gameObject.GetComponent<SphereCollider>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        // Allow reviving only if death animation has completed
        if (playerScript.deathAnimationCompleted)
        {
            bool playerIsInReviveArea = false;

            if (playerScript.isDead)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, reviveRadius);
                foreach (Collider col in hitColliders)
                {
                    Player colPlayer = col.gameObject.GetComponent<Player>();
                    if (colPlayer && colPlayer != playerScript && !colPlayer.isDead)
                    {
                        playerIsInReviveArea = true;
                        playerScript.mostRecentRevivee = colPlayer;
                    }
                }
            }

            if (playerScript.isDead && playerIsInReviveArea)
            {
                playerScript.ToggleIsBeingRevived(true);
            }
            else
            {
                playerScript.ToggleIsBeingRevived(false);
            }
        }
    }
}
