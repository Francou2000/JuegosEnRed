using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LauncherManager : MonoBehaviourPunCallbacks
{
    [SerializeField] PhotonView player1Prefab;
    [SerializeField] Transform player1Spawn;
    [SerializeField] WorldMovement Map;
    
   

    
    public void Start()
    {
       
        int playerNumer=PhotonNetwork.PlayerList.Length;
        Debug.Log("Connected to Server");
        if (playerNumer > 2)
        {
            Debug.Log("sala llena, modo observador activo");
        }
        else
        {
            PhotonNetwork.Instantiate(player1Prefab.name, player1Spawn.position, player1Spawn.rotation);
        }
     
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to MASTER");
        //PhotonNetwork.JoinRandomOrCreateRoom();
        
    }
    
    void Update()
    {
        // Mostrar el número de jugadores conectados en la consola
        if (PhotonNetwork.InRoom)
        {
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount >= 2)Map.StartGame();
            Debug.Log("Número de jugadores conectados: " + playerCount);
        }
        
    }
}
