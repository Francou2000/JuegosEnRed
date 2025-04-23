using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LauncherManager : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("GameSceneName");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel("GameSceneName");
        }
    }
}
    /*
    [SerializeField] PhotonView player1Prefab;
    [SerializeField] Transform player1Spawn;
    [SerializeField] WorldMovement Map;
    
   

    
    public void Start()
    {
       
        int playerNumer=PhotonNetwork.PlayerList.Length;
        //Debug.Log("Connected to Server");
        if (playerNumer > 2)
        {
            //Debug.Log("sala llena, modo observador activo");
        }
        else
        {
            PhotonNetwork.Instantiate(player1Prefab.name, player1Spawn.position, player1Spawn.rotation);
        }
     
    }

    public override void OnConnectedToMaster()
    {
        //Debug.Log("Connected to MASTER");
        //PhotonNetwork.JoinRandomOrCreateRoom();
        
    }
    
    void Update()
    {
        // Mostrar el número de jugadores conectados en la consola
        if (PhotonNetwork.InRoom)
        {
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount >= 2)Map.StartGame();
            //Debug.Log("Número de jugadores conectados: " + playerCount);
        }
        
    }
    */
