﻿using UnityEngine;
using System.Collections;

public class SmoothCamera2D : MonoBehaviour
{

    public float dampTime = 0.15f;
    public Vector3 ViewOffset;
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    private void Start()
    {
        if (target)
        {
            Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
            Vector3 delta = target.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            transform.position = transform.position + delta + ViewOffset;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target)
        {
            Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
            Vector3 delta = target.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = transform.position + delta + ViewOffset;
            Vector3 newPosition = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
            transform.position = GetComponent<UnityEngine.U2D.PixelPerfectCamera>().RoundToPixel(newPosition);
        }

    }
}
