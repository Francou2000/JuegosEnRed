using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button connectButton;
    [SerializeField] private GameObject waitingText;

    private void Start()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
        nicknameInput.onValueChanged.AddListener(OnNicknameChanged);

        connectButton.interactable = false;
        waitingText.SetActive(false);
    }

    private void OnNicknameChanged(string value)
    {
        connectButton.interactable = !string.IsNullOrWhiteSpace(value);
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
