using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer.TagObject == null)
        {
            SpawnLocalPlayer();
        }
    }

    private void SpawnLocalPlayer()
    {
        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length;
        Vector3 spawnPos = spawnPoints[spawnIndex].position;

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
        PhotonNetwork.LocalPlayer.TagObject = player;
        photonView.RPC("RPC_PlayerSpawned", RpcTarget.MasterClient);
    }

    private void StartGame()
    {
        photonView.RPC("RPC_StartWorld", RpcTarget.All);
        ModuleManager.Instance.InitializeModules();
    }

    [PunRPC]
    private void RPC_AssignPlayer(int viewID)
    {
        GameObject playerObj = PhotonView.Find(viewID).gameObject;
        PhotonNetwork.LocalPlayer.TagObject = playerObj;
    }

    [PunRPC]
    private void RPC_StartWorld()
    {
        worldMovement.StartGame();
    }


    [PunRPC]
    public void RPC_ReportDeath(string deadPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Only MasterClient will execute this
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

        // Stop movement and world
        PlayerBasic[] players = FindObjectsOfType<PlayerBasic>();
        foreach (var p in players)
            p.gameEnded = true;

        GameManager.Instance.worldMovement.StopGame();

        StartCoroutine(CallReturnToMenu());
    }

    private IEnumerator CallReturnToMenu()
    {
        yield return new WaitForSeconds(5f);

        // Leave room cleanly
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

    [PunRPC]
    private void RPC_PlayerSpawned()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        playersReady++;

        if (playersReady >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("RPC_StartGame", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_StartGame()
    {
        worldMovement.StartGame();
    }
}
