using UnityEngine;
using Photon.Pun;
public class PlayerBasic : MonoBehaviourPunCallbacks
{
    [SerializeField] private float moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
  
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical"); 
            Vector3 movement = new Vector3(x, y, 0);
            //transform.Translate(movement.normalized*moveSpeed*Time.deltaTime, Space.World);
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
    }
}
