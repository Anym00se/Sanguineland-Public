using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Droppable : MonoBehaviourPunCallbacks
{
    [SerializeField] private float despawnTime;
    private GameSyncer syncer;
    private bool isDestroyed = false;
    [SerializeField] private AudioSource loopingSound;

    // Start is called before the first frame update
    void Start()
    {
        syncer = GameSyncer.instance;

        if (loopingSound)
        {
            loopingSound.Stop();
            loopingSound.volume = Utils.GetSoundVolume();
            loopingSound.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && GetGameSyncer() && !GetGameSyncer().IsPaused() && !isDestroyed)
        {
            despawnTime -= Time.deltaTime;

            if (despawnTime <= 0f)
            {
                isDestroyed = true;
                GetGameSyncer().PhotonDestroy(gameObject);
            }
        }

        if (loopingSound)
        {
            // Pause sound if game paused or not started or already ended 
            if (GameSyncer.instance.IsPaused() || !GameSyncer.instance.HasGameStarted() || GameSyncer.instance.HasGameEnded())
            {
                loopingSound.Pause();
            }
            else
            {
                loopingSound.UnPause();
            }
        }
    }

    private GameSyncer GetGameSyncer()
    {
        if (!syncer)
        {
            syncer = GameSyncer.instance;
        }

        return syncer;
    }
}
