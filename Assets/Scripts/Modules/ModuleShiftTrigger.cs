using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleShiftTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Photon.Pun.PhotonNetwork.IsMasterClient) return;
        if (!other.CompareTag("Module")) return;

        ModuleManager.Instance.TryShiftModule(other.gameObject);
    }
}
