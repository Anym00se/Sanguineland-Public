using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameSyncer : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameSyncer instance { get; private set; }

    [Header("Gameplay")]
    private bool paused = false;
    private bool gameStarted = false;
    private bool gameEnded = false;
    public int waveNumber = 0;
    public int enemyMaxCountInThisWave = 0;
    public int enemiesDestroyedThisWave = 0;
    public int enemiesSpawnedThisWave = 0;
    private int totalEnemiesKilled = 0;
    [SerializeField] private AnimationCurve mediumEnemySpawnOdds;
    [SerializeField] private AnimationCurve largeEnemySpawnOdds;
    [SerializeField] private AnimationCurve chargerEnemySpawnOdds;
    [SerializeField] private AnimationCurve spitterEnemySpawnOdds;
    private bool allowFriendlyFire;
    private float timePassed = 0f;

    [Header("Technical")]
    public GameObject startGamePanel;
    [SerializeField] private GameObject endGamePanel;
    private bool pointerCheckedThisFrame = false;
    private bool cursorOnUIObject = false;

    [Header("Audio")]
    [SerializeField] private AudioSource gameStartSound;
    [SerializeField] private AudioSource gameEndSound;

    private enum EnemyTypes{
        SMALL,
        MEDIUM,
        LARGE,
        CHARGER,
        SPITTER
    };


    private void Awake()
    {
        // Assign the global GameSyncer.instance object
        if (instance == null)
        {
            instance = this;

            if (IsMasterClient())
            {
                // Get the initial friendly fire value from ClientManager if photonview is mine
                allowFriendlyFire = ClientManager.instance.GetAllowFriendlyFire();
            }

            startGamePanel.SetActive(false);
            endGamePanel.SetActive(false);
        }
        else
        {
            // Prevent duplicates in the scene
            Destroy(gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Owner
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(gameStarted);
            stream.SendNext(gameEnded);
            stream.SendNext(waveNumber);
            stream.SendNext(enemyMaxCountInThisWave);
            stream.SendNext(enemiesDestroyedThisWave);
            stream.SendNext(enemiesSpawnedThisWave);
            stream.SendNext(allowFriendlyFire);
            stream.SendNext(timePassed);
            stream.SendNext(totalEnemiesKilled);
        }
        // Other clients
        else if (stream.IsReading)
        {
            gameStarted = (bool)stream.ReceiveNext();
            gameEnded = (bool)stream.ReceiveNext();
            waveNumber = (int)stream.ReceiveNext();
            enemyMaxCountInThisWave = (int)stream.ReceiveNext();
            enemiesDestroyedThisWave = (int)stream.ReceiveNext();
            enemiesSpawnedThisWave = (int)stream.ReceiveNext();
            allowFriendlyFire = (bool)stream.ReceiveNext();
            timePassed = (float)stream.ReceiveNext();
            totalEnemiesKilled = (int)stream.ReceiveNext();
        }
    }

    void LateUpdate()
    {
        pointerCheckedThisFrame = false;
    }

    void Update()
    {
        // Case: Game going on in one of the game scenes
        if (
            IsMasterClient() &&
            photonView.IsMine &&
            !IsPaused() &&
            gameStarted && !gameEnded &&
            (
                SceneManager.GetActiveScene().name == "Day" ||
                SceneManager.GetActiveScene().name == "Evening" ||
                SceneManager.GetActiveScene().name == "Night"
            )
        )
        {
            // End game if all players are dead
            if (!IsAtLeastOnePlayerAlive())
            {
                photonView.RPC("EndGameRPC", RpcTarget.AllBuffered);
            }

            timePassed += Time.deltaTime;

            // Case: All enemies destroyed -> start a new wave
            if (enemiesDestroyedThisWave == enemyMaxCountInThisWave)
            {
                // Make sure there is at least one human player before starting a wave
                if (GetPlayerCount() > 0)
                {
                    StartNextWave();
                }
            }
            // Case: Not all enemies yet spawned -> spawn more enemies
            // Do not allow more than half of the total number of enemies in the wave exist simultaneously
            else if (enemiesSpawnedThisWave < enemyMaxCountInThisWave && GetCurrentEnemyCount() < enemyMaxCountInThisWave / 2f)
            {
                SpawnEnemy(GetSpawnableEnemyType());
            }

            // Check if cursor should be visible
            if (IsPointerOverUIObject())
            {
                ShowCursor();
            }
            else
            {
                HideCursor();
            }
        }
        else
        {
            ShowCursor();
        }
    }

    public void Pause()
    {
        if (gameStarted && !gameEnded)
        {
            ShowCursor();
            photonView.RPC("PauseRPC", RpcTarget.AllBuffered);
        }
    }

    public void Unpause()
    {
        HideCursor();
        photonView.RPC("UnpauseRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void PauseRPC()
    {
        SetPauseState(true);

        GetNavigationHUD().ShowPausePanel();
    }

    [PunRPC]
    public void UnpauseRPC()
    {
        SetPauseState(false);

        GetNavigationHUD().HidePausePanel();
    }

    public void TogglePause()
    {
        if (paused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    private void SetPauseState(bool newPauseState)
    {
        paused = newPauseState;

        // Case: pause
        if (paused)
        {
            Time.timeScale = 0f;
        }
        // Case: unpause
        else
        {
            Time.timeScale = 1f;
        }
    }

    public bool IsPaused()
    {
        return paused;
    }

    void SpawnEnemy(EnemyTypes type)
    {
        if (type == EnemyTypes.SMALL)
        {
            GameObject newEnemy = NetworkManager.instance.SpawnEnemy("Enemy_Small");
        }
        else if (type == EnemyTypes.MEDIUM)
        {
            GameObject newEnemy = NetworkManager.instance.SpawnEnemy("Enemy_Medium");
        }
        else if (type == EnemyTypes.LARGE)
        {
            GameObject newEnemy = NetworkManager.instance.SpawnEnemy("Enemy_Large");
        }
        else if (type == EnemyTypes.CHARGER)
        {
            GameObject newEnemy = NetworkManager.instance.SpawnEnemy("Enemy_Charger");
        }
        else if (type == EnemyTypes.SPITTER)
        {
            GameObject newEnemy = NetworkManager.instance.SpawnEnemy("Enemy_Spitter");
        }
    }

    float GetMediumEnemySpawnOdds()
    {
        return mediumEnemySpawnOdds.Evaluate((float)waveNumber / 10f);
    }
    float GetLargeEnemySpawnOdds()
    {
        return largeEnemySpawnOdds.Evaluate((float)waveNumber / 10f);
    }

    float GetChargerEnemySpawnOdds()
    {
        return chargerEnemySpawnOdds.Evaluate((float)waveNumber / 10f);
    }
    float GetSpitterEnemySpawnOdds()
    {
        return spitterEnemySpawnOdds.Evaluate((float)waveNumber / 10f);
    }

    public int GetPlayerCount()
    {
        return GameObject.FindGameObjectsWithTag("Player").Length;
    }

    private void StartNextWave()
    {
        waveNumber++;

        // Case: Game end
        if (waveNumber > 10)
        {
            photonView.RPC("EndGameRPC", RpcTarget.AllBuffered);
        }
        // Case: Next wave
        else
        {
            totalEnemiesKilled += enemiesDestroyedThisWave;

            enemiesDestroyedThisWave = 0;
            enemiesSpawnedThisWave = 0;
            enemyMaxCountInThisWave = waveNumber * ClientManager.instance.difficulty * 10 * GetPlayerCount();

            // Spawn half of the wave's enemies
            for (int i = 0; i < enemyMaxCountInThisWave / 2f; i++)
            {
                SpawnEnemy(GetSpawnableEnemyType());
            }
        }
    }

    private EnemyTypes GetSpawnableEnemyType()
    {
        EnemyTypes type = EnemyTypes.SMALL;

        if (Random.Range(0f, 1f) < GetLargeEnemySpawnOdds())
        {
            type = EnemyTypes.LARGE;
        }
        else if (Random.Range(0f, 1f) < GetMediumEnemySpawnOdds())
        {
            type = EnemyTypes.MEDIUM;
        }
        else if (Random.Range(0f, 1f) < GetSpitterEnemySpawnOdds())
        {
            type = EnemyTypes.SPITTER;
        }
        else if (Random.Range(0f, 1f) < GetChargerEnemySpawnOdds())
        {
            type = EnemyTypes.CHARGER;
        }

        return type;
    }

    public bool GetAllowFriendlyFire()
    {
        return allowFriendlyFire;
    }

    private NavigationHUD GetNavigationHUD()
    {
        GameObject[] hudObjects = GameObject.FindGameObjectsWithTag("NavigationHUD");

        // There should always be only one active NavigationHUD object
        return hudObjects[0].GetComponent<NavigationHUD>();
    }

    public void PhotonDestroy(GameObject obj)
    {
        int id = obj.GetComponent<PhotonView>().ViewID;
        photonView.RPC("PhotonDestroyRPC", RpcTarget.AllBuffered, id);
    }

    [PunRPC]
    public void PhotonDestroyRPC(int id)
    {
        if (IsMasterClient())
        {
            NetworkManager.instance.DestroyViaPhoton(id);
        }
    }

    public void PhotonSpawn(string str, Vector3 position)
    {
        photonView.RPC("PhotonSpawnRPC", RpcTarget.AllBuffered, str, position);
    }

    [PunRPC]
    public void PhotonSpawnRPC(string str, Vector3 position)
    {
        if (IsMasterClient())
        {
            NetworkManager.instance.SpawnViaPhoton(str, position);
        }
    }

    private int GetCurrentEnemyCount()
    {
        return enemiesSpawnedThisWave - enemiesDestroyedThisWave;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public void StartGame()
    {
        photonView.RPC("StartGameRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void StartGameRPC()
    {
        HideCursor();
        gameStarted = true;
        startGamePanel.SetActive(false);

        PlayGameStartSound();
        ClientManager.instance.PlayGameplayMusic();
    }

    [PunRPC]
    public void EndGameRPC()
    {
        ShowCursor();
        totalEnemiesKilled += enemiesDestroyedThisWave;
        gameEnded = true;
        endGamePanel.SetActive(true);

        PlayGameEndSound();
        ClientManager.instance.PlayGameEndMusic();
    }

    public bool HasGameStarted()
    {
        return gameStarted;
    }

    public bool HasGameEnded()
    {
        return gameEnded;
    }

    public string GetTotalGameTime()
    {
        int hours = Mathf.FloorToInt(timePassed / 3600);
        int minutes = Mathf.FloorToInt((timePassed % 3600) / 60);
        int seconds = Mathf.FloorToInt((timePassed % 3600) % 60);

        return string.Format("{0}h {1}min {2}s", hours, minutes, seconds);
    }

    public int GetTotalEnemiesKilled()
    {
        return totalEnemiesKilled;
    }

    private bool IsAtLeastOnePlayerAlive()
    {
        bool atLeastOneAlive = false;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (!player.GetComponent<Player>().isDead)
            {
                atLeastOneAlive = true;
                break;
            }
        }

        return atLeastOneAlive;
    }

    private void ShowCursor()
    {
        Cursor.visible = true;
    }

    private void HideCursor()
    {
        Cursor.visible = false;
    }

    public bool IsPointerOverUIObject()
    {
        if (!pointerCheckedThisFrame)
        {
            pointerCheckedThisFrame = true;
            cursorOnUIObject = Utils.IsPointerOverUIObject();
        }

        return cursorOnUIObject;
    }

    private void PlayGameStartSound()
    {
        gameStartSound.Stop();
        gameStartSound.volume = Utils.GetSoundVolume();
        gameStartSound.Play();
    }

    private void PlayGameEndSound()
    {
        gameEndSound.Stop();
        gameEndSound.volume = Utils.GetSoundVolume();
        gameEndSound.Play();
    }
}
