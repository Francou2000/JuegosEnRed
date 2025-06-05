using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerBasic>();
            if (player != null && player.photonView.IsMine)
            {
                player.GetDamage();
            }
        }
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyBase>();
            if (enemy != null && enemy.photonView.IsMine)
            {
                enemy.HandleStomp();
            }
        }
    }
}
