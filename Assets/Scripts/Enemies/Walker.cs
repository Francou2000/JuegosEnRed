using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Walker : EnemyBase
{
    public float moveSpeed = 2f;
    public Transform groundCheck;
    public float groundCheckDistance = 2f;
    public LayerMask groundLayer;

    private bool movingLeft = true;

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Vector2 direction = movingLeft ? Vector2.left : Vector2.right;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        bool groundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, Color.red);
        if (!groundAhead)
        {
            Flip();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!collision.collider.CompareTag("Player") && !collision.collider.isTrigger)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingLeft = !movingLeft;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

}
