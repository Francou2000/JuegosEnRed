using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

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

    public void ShowLoseScreen()
    {
        losePanel.SetActive(true);
    }
}
