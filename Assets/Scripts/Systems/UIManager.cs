using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject disconnectPannel;
    public TextMeshProUGUI playerNick;

    [Header("Lives UI")]
    public GameObject[] lifeHearts;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateLivesUI(int livesLeft)
    {
        for (int i = 0; i < lifeHearts.Length; i++)
        {
            if (lifeHearts[i] == null)
            {
                continue;
            }

            lifeHearts[i].SetActive(i < livesLeft);
        }
    }

    public void ShowWinScreen()
    {
        winPanel.SetActive(true);
    }

    public void ShowDisconnect(string text)
    {
        disconnectPannel.SetActive(true);
        playerNick.text = text;
    }

    public void WriteMessage(string text)
    {
        playerNick.text = text;
        //winPanel.GetComponent<TextMeshPro>().text = text + "win";
        //losePanel.GetComponent<TextMeshPro>().text = text + "lose";

    }

    public void ShowLoseScreen()
    {
        losePanel.SetActive(true);
    }
}
