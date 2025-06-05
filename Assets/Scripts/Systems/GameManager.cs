using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerPrefab;
    public WorldMovement worldMovement;

    private int playersReady = 0;
    private Dictionary<int, GameObject> spawnedPlayers = new Dictionary<int, GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnectedAndReady) return;

        if (PhotonNetwork.IsMasterClient)
        {
            ModuleManager.Instance.InitializeModules();
            StartCoroutine(WaitThenSpawnPlayer());
        }
        else
        {
            StartCoroutine(WaitThenSpawnPlayer());
        }
    }

    private IEnumerator WaitThenSpawnPlayer()
    {
        yield return new WaitForSeconds(1.0f); // Allow modules to load first
        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length;
        Vector3 spawnPos = spawnPoints[spawnIndex].position;

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
        PhotonNetwork.LocalPlayer.TagObject = player;

        // Tell MasterClient this player is ready
        photonView.RPC("RPC_PlayerSpawned", RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RPC_PlayerSpawned()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        playersReady++;

        if (playersReady >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("RPC_StartWorld", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_StartWorld()
    {
        if (worldMovement == null)
        {
            Debug.LogError("[GameManager] WorldMovement reference is missing!");
            return;
        }

        worldMovement.StartGame();
    }

    [PunRPC]
    public void RPC_ReportDeath(string deadPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        photonView.RPC("RPC_GameOver", RpcTarget.All, deadPlayer);
    }

    [PunRPC]
    public void RPC_GameOver(string loser)
    {
        bool isWinner = PhotonNetwork.NickName != loser;

        if (isWinner)
            UIManager.Instance.ShowWinScreen();
        else
            UIManager.Instance.ShowLoseScreen();

        // Freeze all players
        PlayerBasic[] players = FindObjectsOfType<PlayerBasic>();
        foreach (var p in players)
            p.gameEnded = true;

        worldMovement.StopGame();
        StartCoroutine(CallReturnToMenu());
    }

    private IEnumerator CallReturnToMenu()
    {
        yield return new WaitForSeconds(5f);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}