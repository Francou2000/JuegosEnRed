using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerBasic>();
            if (player != null && player.photonView.IsMine)
            {
                player.GetDamage();
                other.transform.position = spawnPoint.position;
            }
        }
    }
}
