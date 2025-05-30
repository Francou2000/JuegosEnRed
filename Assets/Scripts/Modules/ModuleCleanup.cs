using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleCleanup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("CleanupZone"))
        {
            if (ModuleManager.Instance != null)
            {
                ModuleManager.Instance.RemoveModule(transform.parent.gameObject);
            }
        }
    }
}
