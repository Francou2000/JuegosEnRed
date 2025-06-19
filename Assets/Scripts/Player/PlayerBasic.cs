using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;
using System.Collections;

public class PlayerBasic : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float moveFactor;
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Jump Checks")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private float wallCheckRadius = 0.15f;
    [SerializeField] private LayerMask platformLayer;

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

        if (photonView.IsMine)
        {
            yield return new WaitUntil(() => UIManager.Instance != null);
            UIManager.Instance.UpdateLivesUI(currentLives);
        }
    }

    void Update()
    {
        if (!photonView.IsMine || gameEnded) return;

        Vector3 movement = new Vector3(moveFactor, 0, 0) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // Coyote jump handling
        bool isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && coyoteTimeCounter > 0f)
        {
            playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            playerAnimator.SetTrigger("Jump");
            coyoteTimeCounter = 0f;
        }

        // Wall detection
        Vector2 wallCheckDirection = moveFactor > 0 ? Vector2.right : Vector2.left;
        bool hitWall = Physics2D.OverlapCircle(wallCheckPoint.position, wallCheckRadius, platformLayer);

        if (hitWall)
        {
            ChangeDirection();
        }

        // Debug: leave room
        if (Input.GetKeyDown(KeyCode.L))
        {
            LeftLobby();
        }
    }

    public void ChangeDirection()
    {
        moveFactor *= -1;

        // Flip sprite based on direction
        if (moveFactor > 0)
            transform.localScale = new Vector3(2, 2, 2);
        else
            transform.localScale = new Vector3(-2, 2, 2);

        playerRigidbody.velocity = new Vector2(moveFactor * moveSpeed, playerRigidbody.velocity.y);

        // Send the new direction to all clients
        photonView.RPC(nameof(RPC_ChangeDirectionVisual), RpcTarget.All, moveFactor);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!photonView.IsMine || gameEnded) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                coyoteTimeCounter = coyoteTime; // Also refresh on land
            }
        }
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

    public void LeftLobby()
    {
        GameManager.Instance.photonView.RPC("RPC_Disconnected", RpcTarget.All, PhotonNetwork.NickName);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheckPoint.position, wallCheckRadius);
        }
    }

    [PunRPC]
    private void RPC_ChangeDirectionVisual(float direction)
    {
        // Flip sprite based on direction
        if (direction > 0)
            transform.localScale = new Vector3(2, 2, 2);
        else
            transform.localScale = new Vector3(-2, 2, 2);
    }
}
