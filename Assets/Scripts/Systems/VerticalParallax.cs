using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalParallax : MonoBehaviour
{
    public Transform cameraTransform;
    public float depthFactor = 0.5f;
    private float startY;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        startY = transform.position.y;
    }

    void Update()
    {
        float camY = cameraTransform.position.y;
        transform.position = new Vector3(
            transform.position.x,
            startY + camY * depthFactor,
            transform.position.z
        );
    }
}
