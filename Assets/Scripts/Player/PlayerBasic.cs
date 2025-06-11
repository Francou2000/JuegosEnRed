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
    private Animator playerAnimator;
    public bool canJump = true;

    [Header("Health")]
    [SerializeField]private int maxLives = 3;
    private int currentLives;

    public bool gameEnded = false;

    private UIManager uiManager;

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
        Debug.unityLogger.Log("puede saltar: " +canJump);
        if (Input.GetKeyDown(KeyCode.W) && canJump)
        //if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && canJump)
        {
            Debug.Log("saltar");
            playerRigidbody.AddForce(new Vector2(0, jumpForce * 100));
            playerAnimator.SetTrigger("Jump");
            canJump = false;
        }
    }

    public void ChangeDirection()
    {
        moveFactor *= -1;
        if (moveFactor > 0)
        {
            transform.localScale = new Vector3(2, 2, 2);
        }
        else
        {
            transform.localScale = new Vector3(-2, 2, 2);
        }

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Debug.Log(other.gameObject.name);
        if (other.gameObject.layer==3)
        {
            
            Debug.Log("chocando con layer 3");
            canJump = true;
        }
        if (!other.gameObject.CompareTag("Floor"))
        {
            ChangeDirection();
        }
        // else
        // {
        //     canJump = true;
        // }
        if (other.gameObject.layer==3)
        {
            
            Debug.Log("chocando con layer 3");
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
