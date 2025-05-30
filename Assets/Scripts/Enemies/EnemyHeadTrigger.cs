using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHeadTrigger : MonoBehaviour
{
    private EnemyBase parentEnemy;

    private void Start()
    {
        parentEnemy = GetComponentInParent<EnemyBase>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var playerRb = other.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        if (playerRb.velocity.y <= 0f)
        {
            parentEnemy.HandleStomp();
        }
    }
}
