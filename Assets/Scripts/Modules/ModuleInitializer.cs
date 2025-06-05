using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ModuleInitializer : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log($"[ModuleInitializer] Instantiated on client {PhotonNetwork.LocalPlayer.NickName}");

        if (WorldMovement.Instance != null)
        {
            transform.SetParent(WorldMovement.Instance.transform, true);
            Debug.Log($"[ModuleInitializer] Set parent to WorldMovement for {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[ModuleInitializer] WorldMovement.Instance is null on client {PhotonNetwork.LocalPlayer.NickName}");
        }
    }
}
