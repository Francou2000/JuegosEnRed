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
    public int activeModuleCount = 3;
    [SerializeField] private float moduleHeight = 20f;
    [SerializeField] private float horizontalOffset = 11f;

    private List<GameObject> activeModules = new List<GameObject>();

    [SerializeField] private WorldMovement worldMovement;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void InitializeModules()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Not MasterClient. Module init skipped.");
            return;
        }

        Debug.Log("Initializing modules...");

        Vector3 spawnPos = new Vector3(horizontalOffset, 0f, 0f);

        if (startModule == null)
        {
            Debug.LogError("Start module is not assigned!");
            return;
        }

        // Spawn starting module
        GameObject start = PhotonNetwork.Instantiate(startModule.name, spawnPos, Quaternion.identity);
        start.transform.SetParent(worldMovement.transform);

        activeModules.Add(start);

        // Add random modules
        for (int i = 1; i < activeModuleCount; i++)
        {
            spawnPos = GetNextModuleTop();
            AddRandomModule(spawnPos);
        }
    }

    private Vector3 GetNextModuleTop()
    {
        if (activeModules.Count == 0)
            return Vector3.zero;

        Vector3 lastPos = activeModules[activeModules.Count - 1].transform.position;
        return lastPos + new Vector3(0f, moduleHeight, 0f);
    }

    private void AddRandomModule(Vector3 position)
    {
        if (modulePool.Count == 0)
        {
            Debug.LogError("Module pool is empty!");
            return;
        }

        GameObject prefab = modulePool[Random.Range(0, modulePool.Count)];

        // Since we're using fixed module size, position is already adjusted
        Vector3 spawnPosition = position;

        GameObject module = PhotonNetwork.Instantiate(prefab.name, spawnPosition, Quaternion.identity);
        module.transform.SetParent(worldMovement.transform);
        activeModules.Add(module);

        SpawnEnemiesInModule(module);
    }

    public void RemoveModule(GameObject module)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        activeModules.Remove(module);
        PhotonNetwork.Destroy(module);

        // Add new module on top
        Vector3 newTop = GetNextModuleTop();
        AddRandomModule(newTop);
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
}
