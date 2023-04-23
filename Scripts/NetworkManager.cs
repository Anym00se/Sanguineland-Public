using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Rendering.PostProcessing;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private List<RoomInfo> roomsList;
    private GUIStyle debugGUIStyle;


    public static NetworkManager instance { get; private set; }

    private void Awake()
    {
        // Assign the global NetworkManager.instance object
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            // Prevent duplicates in the scene
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        debugGUIStyle = new GUIStyle();
        debugGUIStyle.normal.textColor = Color.red;
        debugGUIStyle.fontSize = 22;
        
        roomsList = new List<RoomInfo>();

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 0.01f;
        DisconnectCR();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Connect();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Disconnect();
        }
    }

    private void OnGUI()
    {
        return;

        /* These are only for debugging
        GUILayout.Label("Online: " + (!IsOfflineMode()).ToString(), debugGUIStyle);
        GUILayout.Label("Connected: " + PhotonNetwork.IsConnected.ToString(), debugGUIStyle);
        GUILayout.Label("Connected and ready: " + IsConnectedAndReady().ToString(), debugGUIStyle);
        GUILayout.Label("In lobby: " + PhotonNetwork.InLobby.ToString(), debugGUIStyle);
        
        if (PhotonNetwork.CurrentRoom != null)
        {
            GUILayout.Label("In room: " + PhotonNetwork.CurrentRoom.Name.ToString(), debugGUIStyle);
        }
        */
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomsList = roomList;
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogFormat(
            "{0}, {1}",
            Utils.StringColor("NetworkManager: OnConnectedToMaster", "orange"),
            IsOfflineMode() ?
                Utils.StringColor("Offline", "red") :
                Utils.StringColor("Online", "green")
        );

        // Case: Online Mode -> Join lobby when connected to master
        if (IsOfflineMode() == false)
        {
            Debug.Log(Utils.StringColor("Join Lobby", "cyan"));
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogFormat(
            "{0}, {1}, Cause: {2}",
            Utils.StringColor("NetworkManager: OnDisconnected", "orange"),
            IsOfflineMode() ?
                Utils.StringColor("Offline", "red") :
                Utils.StringColor("Online", "green"),
            cause
        );

        // Case: in Main Menu -> show servers
        if (GetMainMenu())
        {
            GetMainMenu().ShowServers();
        }
        // Case: in game -> go to Main Menu
        else
        {
            LoadLevel("Main Menu");
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log(Utils.StringColor("OnJoinedLobby", "green"));
        GetMainMenu().ShowServers();
    }

    public override void OnJoinedRoom()
    {
        
    }

    public GameObject SpawnEnemy(string enemyTypeName)
    {
        GameObject newEnemy = null;

        if (PhotonNetwork.IsMasterClient)
        {
            newEnemy = PhotonNetwork.Instantiate(
                "Enemies/" + enemyTypeName,
                GetFreeEnemySpawnLocation(),
                Quaternion.identity,
                0
            );
        }

        return newEnemy;
    }

    public List<RoomInfo> GetRoomsList()
    {
        return roomsList;
    }

    public void CreateServer(string name)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(name, roomOptions);
    }

    public void JoinServer(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }

    public bool IsInLobby()
    {
        return PhotonNetwork.InLobby;
    }

    public void SpawnPlayer(string characterType)
    {
        // Send player name as player data
        string[] playerData = new string[1];
        playerData[0] = "w";
        
        PhotonNetwork.Instantiate("PlayerCharacters/" + characterType, GetFreePlayerSpawnLocation(), Quaternion.identity, 0, playerData);

        // Disable BirdView camera
        GameObject.FindWithTag("BirdViewCamera").GetComponent<Camera>().enabled = false;
        GameObject.FindWithTag("BirdViewCamera").GetComponent<PostProcessVolume>().enabled = false;
        GameObject.FindWithTag("BirdViewCamera").GetComponent<AudioListener>().enabled = false;
        // GameObject.FindWithTag("BirdViewCamera").GetComponent<PostProcessLayer>().enabled = false;

        GameObject.FindWithTag("CharacterSelectionCanvas").SetActive(false);
        GameSyncer.instance.startGamePanel.SetActive(true);
    }

    public void SpawnGameSyncer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("GameSyncer", Vector3.zero, Quaternion.identity, 0);
        }
    }

    public string GetCurrentRoomName()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            return PhotonNetwork.CurrentRoom.Name.ToString();
        }
        else
        {
            return "-1";
        }
    }

    public void SpawnGunner()
    {
        SpawnPlayer("Gunner");
    }

    public void SpawnTorcher()
    {
        SpawnPlayer("Torcher");
    }

    public void SpawnBombardier()
    {
        SpawnPlayer("Bombardier");
    }

    public void SpawnLaserer()
    {
        SpawnPlayer("Laserer");
    }

    Vector3 GetFreeEnemySpawnLocation()
    {
        Vector3 randomUnitVector = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        return randomUnitVector * Random.Range(28f, 45f); // The fence is at 25m
    }

    Vector3 GetFreePlayerSpawnLocation()
    {
        Vector3 randomUnitVector = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        return randomUnitVector * 4f * Random.Range(0.5f, 1f);
    }

    public void LoadLevel(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    public bool IsOfflineMode()
    {
        return PhotonNetwork.OfflineMode == true;
    }

    public IEnumerator Connect()
    {
        Debug.Log(Utils.StringColor("Connect", "cyan"));
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.ConnectUsingSettings();

        // Wait for disconnect
        while (PhotonNetwork.IsConnected == false)
        {
            yield return null;
        }
    }

    public IEnumerator Disconnect()
    {
        Debug.Log(Utils.StringColor("Disconnect", "cyan"));
        PhotonNetwork.Disconnect();

        // Wait for disconnect
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        PhotonNetwork.OfflineMode = true;
    }

    public void DisconnectCR()
    {
        StartCoroutine(Disconnect());
    }

    public void ConnectCR()
    {
        StartCoroutine(Connect());
    }

    private MainMenu GetMainMenu()
    {
        MainMenu menu = null;

        GameObject menuObj = GameObject.FindGameObjectWithTag("MainMenu");

        if (menuObj)
        {
            menu = menuObj.GetComponent<MainMenu>();
        }

        return menu;
    }

    public bool IsConnectedAndReady()
    {
        return PhotonNetwork.IsConnectedAndReady == true;
    }

    public void DestroyViaPhoton(int id)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView view = PhotonView.Find(id);
            if (view)
            {
                GameObject obj = view.gameObject;
                PhotonNetwork.Destroy(obj);
            }
        }
        else
        {
            Debug.LogError("Not master trying to destroy over network.");
        }
    }

    public void SpawnViaPhoton(string str, Vector3 position)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(str, position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Not master trying to spawn over network.");
        }
    }
}
