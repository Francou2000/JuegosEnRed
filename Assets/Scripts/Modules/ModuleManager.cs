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

    private List<GameObject> activeModules = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void InitializeModules()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Vector3 spawnPos = Vector3.zero;

        // Spawn starting module
        GameObject start = PhotonNetwork.Instantiate(startModule.name, spawnPos, Quaternion.identity);
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
        Transform lastTop = activeModules[activeModules.Count - 1].transform.Find("ModuleTopMarker");
        return lastTop != null ? lastTop.position : Vector3.zero;
    }

    private void AddRandomModule(Vector3 position)
    {
        GameObject prefab = modulePool[Random.Range(0, modulePool.Count)];
        GameObject module = PhotonNetwork.Instantiate(prefab.name, position, Quaternion.identity);
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
