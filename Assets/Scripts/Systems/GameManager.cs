using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerPrefab;
    public WorldMovement worldMovement;

    private Dictionary<int, GameObject> spawnedPlayers = new Dictionary<int, GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnAllPlayers();
        }
    }

    private void SpawnAllPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int actorNumber = player.ActorNumber;
            int spawnIndex = actorNumber % spawnPoints.Length;
            Vector3 spawnPos = spawnPoints[spawnIndex].position;

            GameObject playerObj = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity, 0);
            spawnedPlayers[actorNumber] = playerObj;

            photonView.RPC("RPC_AssignPlayer", player, playerObj.GetComponent<PhotonView>().ViewID);
        }

        Invoke(nameof(StartGame), 1.0f); // Delay start to allow setup
    }

    private void StartGame()
    {
        photonView.RPC("RPC_StartWorld", RpcTarget.All);
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
}
