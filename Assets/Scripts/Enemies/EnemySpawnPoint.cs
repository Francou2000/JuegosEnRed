using UnityEngine;
public class EnemySpawnPoint : MonoBehaviour
{
    public enum EnemyType
    {
        Walker,
        Ranged
        // Add more if needed
    }

    public EnemyType enemyToSpawn;
}
