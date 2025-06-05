using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMovement : MonoBehaviour
{
    [SerializeField]private float worldSpeedMultiplier;
    [SerializeField]private float speed;
    private bool isMoving = false;

    public void Update()
    {
        if (!isMoving) return;

        Vector3 movement = new Vector3(0,worldSpeedMultiplier*speed*Time.deltaTime,0);
        gameObject.transform.position += movement;
    }

    public void StartGame()
    {
        speed = -1f;
        isMoving = true;
    }

    public void StopGame()
    {
        isMoving = false;
    }
}
