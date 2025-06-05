using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ModuleCleanup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.CompareTag("Module"))
        {
            ModuleManager.Instance.CycleModules();
        }
    }
}
