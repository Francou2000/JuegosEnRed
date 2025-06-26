using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionTriggers : MonoBehaviour
{
    public enum TriggerType { Ground, Side }
    public TriggerType triggerType;

    public PlayerBasic player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!player.photonView.IsMine) return;

        switch (triggerType)
        {
            case TriggerType.Ground:
                if (other.gameObject.layer == LayerMask.NameToLayer("Floor"))
                    player.SetGrounded(true);
                else if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
                    player.ChangeDirection();
                break;

            case TriggerType.Side:
                // Ignore self
                if (other.gameObject == player.gameObject) return;

                // Change direction if hit anything solid (walls, platforms, other players, enemies)
                player.TryChangeDirection();
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!player.photonView.IsMine) return;

        if (triggerType == TriggerType.Ground && other.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            player.SetGrounded(false);
        }
    }
}
