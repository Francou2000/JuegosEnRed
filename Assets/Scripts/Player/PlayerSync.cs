using UnityEngine;
using Photon.Pun;

public class PlayerSync : MonoBehaviourPun, IPunObservable
{
    private Vector3 networkPosition;
    private Vector3 networkVelocity;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) //Local Player
        {
            stream.SendNext(transform.position);
            stream.SendNext(rb.velocity);
        }
        else //Remote Player
        { 
            networkPosition = (Vector3)stream.ReceiveNext();    
            networkVelocity = (Vector3)stream.ReceiveNext();
        }
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) 
        {
            //Interpolate manually
            transform.position = Vector3.Lerp(transform.position, networkPosition, 0.3f);
        }
    }
}
