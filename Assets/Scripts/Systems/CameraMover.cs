using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float speedMultiplier = .25f;
    private bool isMoving = false;

    public void StartCamera()
    {
        isMoving = true;
    }

    public void StopCamera()
    {
        isMoving = false;
    }

    private void Update()
    {
        if (!isMoving) return;

        Vector3 movement = new Vector3(0, speed * speedMultiplier * Time.deltaTime, 0);
        transform.position += movement;
    }
}
