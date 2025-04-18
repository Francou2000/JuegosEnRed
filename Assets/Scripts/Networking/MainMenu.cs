using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public string sceneToLoad;
    public TMP_InputField NickName;
    public Button StartButton;
    private const string nicknameKey = "playerNickname";
    private string nickname;

    private void Start()
    {
        StartButton.onClick.AddListener(HandleConnectButton);
        NickName.onValueChanged.AddListener(VerifyName);
    }

    private void VerifyName(string value)
    {
        if (NickName.text.Length == 0)
        {
            StartButton.interactable = false;
        }
        if (NickName.text.Length >= 1 && !StartButton.interactable)
        {
            StartButton.interactable = true;
        }

        nickname = value;
    }

    private void HandleConnectButton()
    {
        PlayerPrefs.SetString(nicknameKey, nickname);
        PhotonNetwork.NickName = nickname.ToUpper();
        print(nickname+" Connecting...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(nickname + " connected!");
        SceneManager.LoadScene(sceneToLoad);
    }

}
