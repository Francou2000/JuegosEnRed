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

    // Start is called before the first frame update
    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        playerRigidbody = gameObject.GetComponent<Rigidbody2D>();
        if (photonView.IsMine)
        {
            currentLives = maxLives; // Reset lives
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            //float x = Input.GetAxis("Horizontal");
            Vector3 movement = new Vector3(moveFactor, 0, 0) * moveSpeed * Time.deltaTime;
            transform.position += movement;
            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && canJump)
            {
                Debug.Log("saltando");
                playerRigidbody.AddForce(new Vector2(0, jumpForce * 100));
                canJump = false;
            }
            //transform.Translate(movement.normalized*moveSpeed*Time.deltaTime, Space.World);

            //new Vector3(moveFactor,0,0)* moveSpeed * Time.deltaTime;
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
            Debug.Log("toco piso");
            // other.gameObject.GetComponent<PlayerBasic>().ChangeDirection();
            // Debug.Log("Choco con otro pj");
        }
        else
        {
            canJump = true;
            Debug.Log("ya puede saltar");
        }
    }

    public void GetDamage()
    {
        if (currentLives != 0)
        {
            currentLives--;
            lifeUI[currentLives].SetActive(false);
           
        }else
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
