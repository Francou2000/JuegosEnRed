using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class ModuleManager : MonoBehaviourPun
{
    public static ModuleManager Instance;

    [Header("Module Prefabs")]
    public GameObject startModule;
    public List<GameObject> modulePool;

    [Header("Settings")]
    [SerializeField] private float moduleHeight = 20f;
    [SerializeField] private float horizontalOffset = 11f;

    private GameObject lastModule;
    private GameObject currentModule;
    private GameObject nextModule;


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void InitializeModules()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Vector3 spawnPos = new Vector3(horizontalOffset, 0f, 0f);

        // Start with current module
        currentModule = PhotonNetwork.Instantiate(startModule.name, spawnPos, Quaternion.identity);

        // Add next module on top
        Vector3 nextPos = spawnPos + Vector3.up * moduleHeight;
        nextModule = AddRandomModule(nextPos);

        // Add a "last" module underneath
        Vector3 lastPos = spawnPos - Vector3.up * moduleHeight;
        lastModule = PhotonNetwork.Instantiate(startModule.name, lastPos, Quaternion.identity);
    }

    private GameObject AddRandomModule(Vector3 position)
    {
        if (modulePool.Count == 0)
        {
            Debug.LogError("Module pool is empty!");
            return null;
        }

        GameObject prefab = modulePool[Random.Range(0, modulePool.Count)];

        // Since we're using fixed module size, position is already adjusted
        Vector3 spawnPosition = position;

        GameObject module = PhotonNetwork.Instantiate(prefab.name, spawnPosition, Quaternion.identity);
        
        SpawnEnemiesInModule(module);

        return module;     
    }

    public void CycleModules()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.Destroy(lastModule);

        // Slide references
        lastModule = currentModule;
        currentModule = nextModule;

        Vector3 newPos = currentModule.transform.position + Vector3.up * moduleHeight;
        nextModule = AddRandomModule(newPos);
    }

    private void SpawnEnemiesInModule(GameObject module)
    {
        Transform enemySpawns = module.transform.Find("EnemySpawns");
        if (enemySpawns == null) return;

        foreach (Transform spawn in enemySpawns)
        {
            EnemySpawnPoint spawnPoint = spawn.GetComponent<EnemySpawnPoint>();
            if (spawnPoint == null) continue;

            string prefabToSpawn = GetEnemyPrefabName(spawnPoint.enemyToSpawn);
            if (!string.IsNullOrEmpty(prefabToSpawn))
            {
                PhotonNetwork.Instantiate(prefabToSpawn, spawn.position, Quaternion.identity);
            }
        }
    }

    private string GetEnemyPrefabName(EnemySpawnPoint.EnemyType type)
    {
        switch (type)
        {
            case EnemySpawnPoint.EnemyType.Walker: return "WalkerEnemy";
            case EnemySpawnPoint.EnemyType.Ranged: return "RangedEnemy";
            default: return "";
        }
    }

    public Transform[] GetCurrentPlayerSpawns()
    {
        Transform spawnParent = currentModule.transform.Find("PlayerSpawns");

        if (spawnParent == null)
        {
            Debug.LogError("No PlayerSpawns found in current module.");
            return new Transform[0];
        }

        Transform[] spawns = new Transform[spawnParent.childCount];
        for (int i = 0; i < spawnParent.childCount; i++)
            spawns[i] = spawnParent.GetChild(i);

        return spawns;
    }
}
