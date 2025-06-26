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
        Debug.LogWarning("ON PLAYER ENTERED ROOM" + newPlayer.HasRejoined);
        Debug.LogWarning(newPlayer.ToString());
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("UN JUGADOR INGRESO");
            if (newPlayer.NickName.Equals(dcPlayer))
            {
                reconnected = true;
                photonView.RPC("RPC_Reconnected",RpcTarget.All);
                Debug.LogWarning("SE RECONECTO EL MISMO WEONNN");
                
            }
        }
        else
        {
            Debug.LogError("No soy master client");
        }
       

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        dcPlayer = otherPlayer.NickName;
        photonView.RPC("RPC_Disconnected",RpcTarget.All,otherPlayer.NickName);
        Debug.LogWarning("UN JUGADOR ABANDONO LA SALA (G.M)");
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
    public void RPC_Reconnected()
    {
        PlayerBasic[] players = FindObjectsOfType<PlayerBasic>();
        foreach (var p in players)
            p.gameEnded = false;
        cam.StartCamera();
        Debug.LogError("FUNCIONA CABRON");
        UIManager.Instance.HideDisconect();
        if (!PhotonNetwork.IsMasterClient)
        {
            SpawnLocalPlayer();
            Debug.LogWarning("no soy masterclient");
        }
        else
        {
            Debug.LogWarning("soy el rayo mcqueen");
            
        }
    }

    [PunRPC]
    public void RPC_KickPlayer2()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("TE KICKEARON MI PANA");
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
        Debug.LogError("Se detecto desconeccion, tiempo de vida para cerrar sesion:" +PhotonNetwork.CurrentRoom.PlayerTtl/1000);
        yield return new WaitForSeconds(10f);
        //yield return new WaitForSeconds(PhotonNetwork.CurrentRoom.PlayerTtl/1000);

        if (reconnected)
        {
            //photonView.RPC("RPC_Reconnected",RpcTarget.All);
            Debug.Log("SE CANCELA LEAVING ROOM");
        }

        if (PhotonNetwork.InRoom && reconnected==false)
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