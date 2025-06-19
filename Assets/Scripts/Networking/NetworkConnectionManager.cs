using System;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkConnectionManager : MonoBehaviourPunCallbacks
{
    public static NetworkConnectionManager Instance;

    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private byte maxPlayers = 2;

    private string cachedNickname;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }


    private void Update()
    {
        
    }

    public void SetNickname(string nickname)
    {
        cachedNickname = nickname;
    }

    public void JoinGame()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.NickName = cachedNickname;
            PhotonNetwork.JoinRandomOrCreateRoom(
            roomOptions: new RoomOptions { MaxPlayers = maxPlayers },
            typedLobby: TypedLobby.Default
        );
        }
        else
        {
            Debug.LogWarning("Photon not connected yet. Cannot join.");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");
        // Do not auto-join here � wait for JoinGame to be called
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room with " + PhotonNetwork.CurrentRoom.PlayerCount + " player(s)");

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayers });
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        GameManager.Instance.photonView.RPC("RPC_Disconnected",RpcTarget.All,PhotonNetwork.NickName);
        Debug.LogWarning("UN JUGADOR ABANDONO LA SALA");
    }
    
}