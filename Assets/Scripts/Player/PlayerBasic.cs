using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;
using System.Collections;

public class PlayerBasic : MonoBehaviourPunCallbacks
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float moveFactor = 1f;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private bool isGrounded = false;

    [Header("Health")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;
    public bool gameEnded = false;

    private Rigidbody2D playerRigidbody;
    private Animator playerAnimator;

    private IEnumerator Start()
    {
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

        // Debug manual leave
        if (Input.GetKeyDown(KeyCode.L))
        {
            LeftLobby();
            Debug.Log("Leaving lobby...");
        }
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
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

    private void Jump()
    {
        playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Jump");
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

    private void FlipSprite(float direction)
    {
        float scaleX = Mathf.Sign(direction) * 2f;
        transform.localScale = new Vector3(scaleX, 2f, 2f);
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
