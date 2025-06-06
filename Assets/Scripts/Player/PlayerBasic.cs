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

    [Header("Health")]
    [SerializeField]private int maxLives = 3;
    private int currentLives;

    public bool gameEnded = false;

    private UIManager uiManager;

    private IEnumerator Start()
    {
        playerCollider = GetComponent<Collider2D>();
        playerRigidbody = GetComponent<Rigidbody2D>();
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

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && canJump)
        {
            playerRigidbody.AddForce(new Vector2(0, jumpForce * 100));
            canJump = false;
        }
    }

    public void ChangeDirection()
    {
        moveFactor *= -1;
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

        // Choose a random or indexed spawn
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        transform.position = spawn.position;
        playerRigidbody.velocity = Vector2.zero; // Cancel momentum
    }
}
