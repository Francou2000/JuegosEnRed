using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyBase : MonoBehaviourPun
{
    [Header("Components")]
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called by the head trigger on stomp
    public void HandleStomp()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            photonView.RPC("RPC_RequestDestroy", RpcTarget.MasterClient);
        }

        OnStomped();
    }

    //override to add visual/sound effect
    protected virtual void OnStomped() { }

    [PunRPC]
    private void RPC_RequestDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
