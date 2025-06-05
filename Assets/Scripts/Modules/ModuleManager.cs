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
        Debug.Log("[ModuleManager] Starting module initialization...");

        if (startModule == null)
        {
            Debug.LogError("[ModuleManager] startModule is not assigned!");
            return;
        }
        Vector3 spawnPos = new Vector3(horizontalOffset, 0f, 0f);

        // Create current module (start)
        currentModule = PhotonNetwork.Instantiate(startModule.name, spawnPos, Quaternion.identity);
        currentModule.GetComponent<ModuleIdentity>().role = ModuleIdentity.Role.Start;
        SpawnEnemiesInModule(currentModule);
        if (currentModule == null)
        {
            Debug.LogError("[ModuleManager] Failed to instantiate currentModule!");
            return;
        }

        // Create next module above
        Vector3 nextPos = spawnPos + Vector3.up * moduleHeight;
        nextModule = AddRandomModule(nextPos);

        // Create last module below (another copy of start module)
        Vector3 lastPos = spawnPos - Vector3.up * moduleHeight;
        lastModule = PhotonNetwork.Instantiate(startModule.name, lastPos, Quaternion.identity);
        Debug.Log("[ModuleManager] Module initialization completed.");
    }

    private GameObject AddRandomModule(Vector3 position)
    {
        if (modulePool.Count == 0)
        {
            Debug.LogError("[ModuleManager] Module pool is empty!");
            return null;
        }

        GameObject prefab = modulePool[Random.Range(0, modulePool.Count)];
        GameObject module = PhotonNetwork.Instantiate(prefab.name, position, Quaternion.identity);
        SpawnEnemiesInModule(module);
        return module;
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
        if (currentModule == null)
        {
            Debug.LogError("[ModuleManager] Current module is null.");
            return new Transform[0];
        }

        Transform spawnParent = currentModule.transform.Find("PlayerSpawns");
        if (spawnParent == null)
        {
            Debug.LogError("[ModuleManager] No PlayerSpawns found in current module.");
            return new Transform[0];
        }

        Transform[] spawns = new Transform[spawnParent.childCount];
        for (int i = 0; i < spawns.Length; i++)
            spawns[i] = spawnParent.GetChild(i);

        return spawns;
    }

    public void AssignCurrentModule(GameObject module)
    {
        currentModule = module;
    }

    public void TryShiftModule(GameObject entered)
    {
        if (entered == nextModule)
        {
            Debug.Log("[ModuleManager] Shifting module references...");

            PhotonNetwork.Destroy(lastModule);
            lastModule = currentModule;
            currentModule = nextModule;

            Vector3 spawnPos = currentModule.transform.position + Vector3.up * moduleHeight;
            nextModule = AddRandomModule(spawnPos);
        }
    }
}
