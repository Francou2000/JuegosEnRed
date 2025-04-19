using System;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public string sceneToLoad;
    public TMP_InputField NickName;
    public Button StartButton;
    public GameObject WaitingForPlayerText;
    private const string nicknameKey = "playerNickname";
    private string nickname;
    private int playerCount;

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
        WaitingForPlayerText.SetActive(true);
        ConnectToPhoton();
    }

    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        PhotonNetwork.JoinRandomOrCreateRoom(); 
        //PhotonNetwork.LoadLevel(sceneToLoad);  
        
       
    }

    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            Debug.Log("Número de jugadores conectados: " + playerCount);
            if (playerCount >= 2)PhotonNetwork.LoadLevel(sceneToLoad);
        }
    }
}
