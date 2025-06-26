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
    [SerializeField] public  int playersTTL = 10000;
    
    [SerializeField] private string roomID = "";

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

    public void SetNickname(string nickname)
    {
        cachedNickname = nickname;
    } 

    public void SetRoomID(string ID)
    {
        roomID = ID;
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

    public void JoinRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.NickName = cachedNickname;
            PhotonNetwork.JoinRandomOrCreateRoom(
                roomOptions: new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = playersTTL, }, //max cantidad de players y tiempo de "espera" en dc
                roomName: roomID,
                typedLobby: TypedLobby.Default
                );
            
        }
        else
        {
            Debug.LogWarning("Photon not connected yet. Cannot Join.");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");
        // Do not auto-join here  wait for JoinGame to be called
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room with " + PhotonNetwork.CurrentRoom.PlayerCount + " player(s)");
        Debug.LogWarning("Room name " + PhotonNetwork.CurrentRoom.Name);
        Debug.LogWarning(PhotonNetwork.CurrentRoom.ToString());

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogWarning(newPlayer.ToString());
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
        Debug.LogWarning("A player disconnected");
    }

    public void ReconnectRoomWithID()
    {
        PhotonNetwork.RejoinRoom(roomID);
    }
}