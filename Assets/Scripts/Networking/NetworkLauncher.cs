using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button connectButton;
    [SerializeField] private GameObject waitingText;

    [Header("Scene")]
    [SerializeField] private string gameSceneName;

    private const string NickKey = "playerNickname";

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        connectButton.onClick.AddListener(OnConnectClicked);
        nicknameInput.onValueChanged.AddListener(OnNicknameChanged);
        connectButton.interactable = false;
        waitingText.SetActive(false);
    }

    private void OnNicknameChanged(string value)
    {
        connectButton.interactable = !string.IsNullOrEmpty(value.Trim());
    }

    private void OnConnectClicked()
    {
        // Save & set nickname
        PlayerPrefs.SetString(NickKey, nicknameInput.text);
        PhotonNetwork.NickName = nicknameInput.text.ToUpper();

        // Show waiting indicator
        waitingText.SetActive(true);

        // Connect (if needed)
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        else 
        {
            PhotonNetwork.JoinRandomOrCreateRoom(
            roomOptions: new RoomOptions { MaxPlayers = 2 },
            typedLobby: TypedLobby.Default
            );
        }
    }

    public override void OnConnectedToMaster()
    {
        // Once connected, join or create a room of max 2 players
        PhotonNetwork.JoinRandomOrCreateRoom(
            roomOptions: new RoomOptions { MaxPlayers = 2 },
            typedLobby: TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        // Only the MasterClient loads the game scene—and only when two players are in
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Also handle the case where Master was waiting in room until second player arrives
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // Fallback if no room was available
        PhotonNetwork.CreateRoom(
            roomName: null,
            roomOptions: new RoomOptions { MaxPlayers = 2 },
            typedLobby: TypedLobby.Default
        );
    }
}