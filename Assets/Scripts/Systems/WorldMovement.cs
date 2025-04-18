using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMovement : MonoBehaviour
{
    [SerializeField]private float worldSpeedMultiplier;
    [SerializeField]private float speed;
    

    public void Update()
    {   
        Vector3 movement = new Vector3(0,worldSpeedMultiplier*speed*Time.deltaTime,0);
        gameObject.transform.position += movement;
    }
}
