using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CameraMover cam;

    private int playersReady = 0;

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
        }

        StartCoroutine(WaitThenSpawnPlayer());
    }

    private IEnumerator WaitThenSpawnPlayer()
    {
        yield return new WaitUntil(() => ModuleManager.Instance.GetCurrentPlayerSpawns().Length > 0);

        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        Transform[] spawns = ModuleManager.Instance.GetCurrentPlayerSpawns();
        Debug.Log(spawns.Length);
        if (spawns.Length == 0)
        {
            Debug.LogError("[GameManager] No spawn points found.");
            return;
        }

        int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber % spawns.Length;
        Vector3 spawnPos = spawns[spawnIndex].position;

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
        PhotonNetwork.LocalPlayer.TagObject = player;

        photonView.RPC("RPC_PlayerSpawned", RpcTarget.MasterClient);
    }
    
    
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        photonView.RPC("RPC_Disconnected",RpcTarget.All,otherPlayer.NickName);
        Debug.LogWarning("UN JUGADOR ABANDONO LA SALA");
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
        if (cam == null)
        {
            Debug.LogError("[GameManager] CameraMover reference is missing!");
            return;
        }

        cam.StartCamera();
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
        UIManager.Instance.WriteMessage(PhotonNetwork.NickName);
        if (isWinner)
            
            UIManager.Instance.ShowWinScreen();
        else
            UIManager.Instance.ShowLoseScreen();

        // Freeze all players
        PlayerBasic[] players = FindObjectsOfType<PlayerBasic>();
        foreach (var p in players)
            p.gameEnded = true;

        cam.StopCamera();
        //mensaje fin del game
        StartCoroutine(CallReturnToMenu());
    }

    [PunRPC]
    public void RPC_Disconnected(string dcPlayer)
    {
        UIManager.Instance.ShowDisconnect(dcPlayer);
        PlayerBasic[] players = FindObjectsOfType<PlayerBasic>();
        foreach (var p in players)
            p.gameEnded = true;
        cam.StopCamera();
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