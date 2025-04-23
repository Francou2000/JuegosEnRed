using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;

public class PlayerBasic : MonoBehaviourPunCallbacks
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float moveFactor;
    private Collider2D playerCollider;
    private Rigidbody2D playerRigidbody;
    private bool canJump;

    public int maxLives = 2;
    private int currentLives;
    [SerializeField] private GameObject[] lifeUI;

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        playerRigidbody = gameObject.GetComponent<Rigidbody2D>();

        currentLives = maxLives; // Reset lives

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
        if (photonView.IsMine)
        {
            // LOCAL movement control
            Vector3 movement = new Vector3(moveFactor, 0, 0) * moveSpeed * Time.deltaTime;
            transform.position += movement;

            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && canJump)
            {
                playerRigidbody.AddForce(new Vector2(0, jumpForce * 100));
                canJump = false;
            }
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
            PhotonNetwork.Destroy(gameObject);   
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
}
