using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootCooldown = 2f;
    public float projectileSpeed = 8f;

    private bool facingRight = true;
    private float shootTimer;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            Shoot();
            Flip();
            shootTimer = shootCooldown;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = PhotonNetwork.Instantiate(projectilePrefab.name, firePoint.position, Quaternion.identity);
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        proj.GetComponent<Rigidbody2D>().velocity = dir * projectileSpeed;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
