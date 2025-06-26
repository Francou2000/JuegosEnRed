using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_InputField roomIDInput;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button connectButton;
    [SerializeField] private GameObject waitingText;
    
    private void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
        nicknameInput.onValueChanged.AddListener(OnNicknameChanged);
        
        joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        roomIDInput.onValueChanged.AddListener(OnRoomIDChanged);

        connectButton.interactable = false;
        joinRoomButton.interactable = false;
        waitingText.SetActive(false);
    }

    private void OnNicknameChanged(string value)
    {
        connectButton.interactable = !string.IsNullOrWhiteSpace(value);
    }

    private void OnRoomIDChanged(string value)
    {
        joinRoomButton.interactable = !string.IsNullOrWhiteSpace(value);
    }

    private void OnJoinRoomClicked()
    {
        string nickname = nicknameInput.text.Trim().ToUpper();
        PlayerPrefs.SetString("playerNickname", nickname);
        
        string roomID = roomIDInput.text.Trim().ToUpper();
        waitingText.SetActive(true);
        
        NetworkConnectionManager.Instance.SetNickname(nickname);
        NetworkConnectionManager.Instance.SetRoomID(roomID);
        NetworkConnectionManager.Instance.JoinRoom();
    }
    
    private void ReconnectRoom()
    {
        string nickname = nicknameInput.text.Trim().ToUpper();
        PlayerPrefs.SetString("playerNickname", nickname);
        
        string roomID = roomIDInput.text.Trim().ToUpper();
        waitingText.SetActive(true);
        
        NetworkConnectionManager.Instance.SetNickname(nickname);
        NetworkConnectionManager.Instance.SetRoomID(roomID);
        NetworkConnectionManager.Instance.ReconnectRoomWithID();
    }

    private void OnConnectClicked()
    {
        string nickname = nicknameInput.text.Trim().ToUpper();

        PlayerPrefs.SetString("playerNickname", nickname);
        waitingText.SetActive(true);

        NetworkConnectionManager.Instance.SetNickname(nickname);
        NetworkConnectionManager.Instance.JoinGame();
    }
}
