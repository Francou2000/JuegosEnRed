using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerBasic>().GetDamage();
            other.transform.position = spawnPoint.position;
        }
    }
}
