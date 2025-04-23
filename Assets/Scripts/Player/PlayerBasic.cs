using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;
using System.Collections;

public class PlayerBasic : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float moveFactor;
    private Collider2D playerCollider;
    private Rigidbody2D playerRigidbody;
    private bool canJump;

    public int maxLives = 2;
    private int currentLives;
    [SerializeField] private GameObject[] lifeUI;

    private bool gameEnded = false;

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        playerRigidbody = gameObject.GetComponent<Rigidbody2D>();

        currentLives = maxLives;

        if (!photonView.IsMine)
        {
            // Disable life UI for remote players
            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (gameEnded) return;

        Vector3 movement = new Vector3(moveFactor, 0, 0) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && canJump)
        {
            playerRigidbody.AddForce(new Vector2(0, jumpForce * 100));
            canJump = false;
        }
    }

    public void ChangeDirection()
    {
        moveFactor = moveFactor * -1;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Floor"))
        {
            ChangeDirection();
        }
        else
        {
            canJump = true;
        }
    }

    public void GetDamage()
    {
        if (!photonView.IsMine) return;

        if (currentLives != 0)
        {
            currentLives--;
            lifeUI[currentLives].SetActive(false);
           
        }
        else
        {
            photonView.RPC("RPC_GameOver", RpcTarget.All, PhotonNetwork.NickName);
        }      
    }

    [ContextMenu("GetID")]
    public void PrintID()
    {
        print(photonView.ViewID);
        print(PhotonNetwork.NickName);
    }

    [PunRPC]
    public void RPC_SetNickName(string nickName)
    {
       
    }

    [PunRPC]
    public void RPC_GameOver(string loserNickname)
    {
        bool isLocalPlayerWinner = (PhotonNetwork.NickName != loserNickname);

        if (isLocalPlayerWinner)
        {
            UIManager.Instance.ShowWinScreen();
        }
        else
        {
            UIManager.Instance.ShowLoseScreen();
        }

        //Stops players movement
        PlayerBasic[] allPlayers = FindObjectsOfType<PlayerBasic>();
        foreach (var player in allPlayers)
        {
            player.gameEnded = true;
        }

        //Stops world movement
        if (GameManager.Instance.worldMovement != null)
        {
            GameManager.Instance.worldMovement.StopGame();
        }

        // Master client tells everyone when to load menu
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CallReturnToMenu());
        }
    }

    private IEnumerator CallReturnToMenu()
    {
        yield return new WaitForSeconds(3f);

        photonView.RPC("RPC_LeaveRoomAndLoadMenu", RpcTarget.All);
    }

    [PunRPC]
    public void RPC_LeaveRoomAndLoadMenu()
    {
        StartCoroutine(LeaveAndLoad());
    }

    private IEnumerator LeaveAndLoad()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();

        while (PhotonNetwork.InRoom)
            yield return null;

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
