using UnityEngine;
using Photon.Pun;

public class LauncherManager : MonoBehaviourPunCallbacks
{
    [SerializeField] PhotonView player1Prefab;
    // [SerializeField] GameObject player2Prefab;
    [SerializeField] Transform player1Spawn;
    
    public void Start()
    {
        
        Debug.Log("Connected to Server");
        PhotonNetwork.JoinRandomOrCreateRoom();
        //PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Server");
        PhotonNetwork.JoinRandomOrCreateRoom();
        
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(player1Prefab.name, player1Spawn.position, player1Spawn.rotation);
    }
}
