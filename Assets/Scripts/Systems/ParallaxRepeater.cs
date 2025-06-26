using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxRepeater : MonoBehaviour
{
    public float repeatHeight = 14f;
    public Transform cameraTransform;

    private Vector3 startPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        startPosition = transform.position;
    }

    void Update()
    {
        // Check if the camera moved far enough to require repositioning this background
        if (cameraTransform.position.y > transform.position.y + repeatHeight)
        {
            // Move this background up by two heights (assuming two backgrounds stacked)
            transform.position += new Vector3(0f, repeatHeight * 2f, 0f);
        }
    }
}
