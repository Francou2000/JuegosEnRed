using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private CameraMover cam;

    private bool reconnected = false;
    private string dcPlayer;

    private int playersReady = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnectedAndReady) return;
        PhotonNetwork.EnableCloseConnection = true;
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

        GameObject prefabToUse = PhotonNetwork.IsMasterClient ? player1Prefab : player2Prefab;
        GameObject player = PhotonNetwork.Instantiate(prefabToUse.name, spawnPos, Quaternion.identity);
        PhotonNetwork.LocalPlayer.TagObject = player;

        photonView.RPC("RPC_PlayerSpawned", RpcTarget.MasterClient);
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogWarning("On player entered room" + newPlayer.HasRejoined);
        Debug.LogWarning(newPlayer.ToString());
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("A player entered the room");
            if (newPlayer.NickName.Equals(dcPlayer))
            {
                reconnected = true;
                photonView.RPC("RPC_Reconnected",RpcTarget.All);
                Debug.LogWarning("The same player reconnected");             
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        dcPlayer = otherPlayer.NickName;
        photonView.RPC("RPC_Disconnected",RpcTarget.All,otherPlayer.NickName);
        Debug.LogWarning("A player left the room");
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

        //Game finished message
        StartCoroutine(CallReturnToMenu());
    }

    [PunRPC]
    public void RPC_Reconnected()
    {
        PlayerBasic[] players = FindObjectsOfType<PlayerBasic>();
        foreach (var p in players)
            p.gameEnded = false;

        cam.StartCamera();

        UIManager.Instance.HideDisconect();
        if (!PhotonNetwork.IsMasterClient)
        {
            SpawnLocalPlayer();
        }
    }

    [PunRPC]
    public void RPC_KickPlayer2()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("Kicked by host");
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
        Debug.LogError("Time to return to main menu:" + PhotonNetwork.CurrentRoom.PlayerTtl/1000);
        yield return new WaitForSeconds(10f);

        if (PhotonNetwork.InRoom && reconnected == false)
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