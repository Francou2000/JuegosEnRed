using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;
using System.Collections;
using Photon.Realtime;

public class PlayerBasic : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float moveFactor;

    private string cachedNickname;
    private string cachedRoomName;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private bool isGrounded = false;

    [Header("Health")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    private Collider2D playerCollider;
    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;
    private UIManager uiManager;

    public bool gameEnded = false;

    private IEnumerator Start()
    {
        playerCollider = GetComponent<Collider2D>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        currentLives = maxLives;
        cachedNickname = PhotonNetwork.NickName;
        cachedRoomName = PhotonNetwork.CurrentRoom.Name;

        if (photonView.IsMine)
        {
            yield return new WaitUntil(() => UIManager.Instance != null);
            UIManager.Instance.UpdateLivesUI(currentLives);
        }
    }

    void Update()
    {
        if (!photonView.IsMine || gameEnded) return;

        // Movement
        Vector3 movement = new Vector3(moveFactor, 0, 0) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Coyote timer
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && coyoteTimeCounter > 0f)
        {
            Jump();
            coyoteTimeCounter = 0f;
        }

        // Debug: leave room
        if (Input.GetKeyDown(KeyCode.L))
        {
            
            //LeftLobby();

            Player kick = PhotonNetwork.CurrentRoom.GetPlayer(2);
            PlayerBasic[] players = FindObjectsOfType<PlayerBasic>();
            foreach (var p in players)
                if (!p.name.Equals(cachedNickname))
                {
                    //p.LeftLobby();
                    GameManager.Instance.photonView.RPC("RPC_KickPlayer2",RpcTarget.Others);
                    PhotonNetwork.CloseConnection(kick);
                    Debug.LogError("Kicked player: "+ kick.NickName + " id "+ kick.ActorNumber);
                }
        }
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    private void Jump()
    {
        playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Jump");
    }

    public void TryChangeDirection()
    {
        ChangeDirection();
    }

    public void ChangeDirection()
    {
        moveFactor *= -1;

        // Flip locally
        FlipSprite(moveFactor);

        // Apply movement direction
        playerRigidbody.velocity = new Vector2(moveFactor * moveSpeed, playerRigidbody.velocity.y);

        // Sync flip on all clients
        photonView.RPC("RPC_ChangeDirectionVisual", RpcTarget.Others, moveFactor);
    }

    private void FlipSprite(float direction)
    {
        float scaleX = Mathf.Sign(direction) * 2f;
        transform.localScale = new Vector3(scaleX, 2f, 2f);
    }

    public void GetDamage()
    {
        if (!photonView.IsMine || gameEnded) return;

        currentLives--;
        UIManager.Instance.UpdateLivesUI(currentLives);

        if (currentLives <= 0)
        {
            GameManager.Instance.photonView.RPC("RPC_ReportDeath", RpcTarget.MasterClient, PhotonNetwork.NickName);
        }
        else
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        Transform[] spawnPoints = ModuleManager.Instance.GetCurrentPlayerSpawns();
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("[PlayerBasic] No spawn points found in current module.");
            return;
        }

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        transform.position = spawn.position;
        playerRigidbody.velocity = Vector2.zero;
    }

    /*
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("ON DISCONNECTED ENTER");
        base.OnDisconnected(cause);
        
        PhotonNetwork.ReconnectAndRejoin();
        Debug.Log("T ..." + "\n " + PhotonNetwork.CountOfPlayers);
        //PhotonNetwork.RejoinRoom(cachedRoomName);        
    }*/

    //Debug
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.LogWarning("Other player disconnected: " + otherPlayer.NickName +"/n"+ otherPlayer.ActorNumber +"/n"+otherPlayer.HasRejoined);
    }

    [PunRPC]
    private void RPC_ChangeDirectionVisual(float direction)
    {
        FlipSprite(direction);
    }

    [PunRPC]
    public void RPC_PlayAnimation(string triggerName)
    {
        playerAnimator.SetTrigger(triggerName);
    }
}
