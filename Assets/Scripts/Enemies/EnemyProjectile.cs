using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyProjectile : MonoBehaviourPun
{
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime); // Auto-cleanup
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBasic player = other.GetComponent<PlayerBasic>();
            if (player != null && player.photonView.IsMine)
            {
                player.GetDamage();
            }

            Destroy(gameObject);
        }

        Destroy(gameObject);  
    }
}
