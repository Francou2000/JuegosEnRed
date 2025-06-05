using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ModuleIdentity : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public enum Role { Start, Normal }
    public Role role = Role.Normal;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // MasterClient already knows what it spawned — skip
        if (PhotonNetwork.IsMasterClient) return;

        if (role == Role.Start && ModuleManager.Instance != null)
        {
        Debug.Log("[ModuleIdentity] Assigning currentModule on client.");
        ModuleManager.Instance.AssignCurrentModule(this.gameObject);
        }
    }
}

