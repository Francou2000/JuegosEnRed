using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public Transform[] spawnPoints;
    public GameObject playerPrefab;
    private int playersSpawned = 0;

    public WorldMovement worldMovement;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer.TagObject == null)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length;
            Transform spawnPoint = spawnPoints[spawnIndex];

            GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

            PhotonNetwork.LocalPlayer.TagObject = player;

            photonView.RPC("PlayerSpawned", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void PlayerSpawned()
    {
        playersSpawned++;

        if (playersSpawned >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            worldMovement.StartGame();
        }
    }
}
