﻿//Based upon the SmoothFollow script included in the Unity Standard Assets package

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private const float Y_ANGLE_MIN = 0.0f;
    private const float Y_ANGLE_MAX = 50.0f;

    private Transform lookAt;
    public GameObject camTransform;
    public Transform camPos;
    public float distance = 10.0f;

    private float currentX = 0.0f;
    private float currentY = 45.0f;
    private float sensitivityX = 4.0f;
    private float sensitivityY = 1.0f;

    private void Start()
    {
        //Set 
        Debug.Log("start method called");
        GameObject target;
        target = GameObject.Find("PropModel");
        lookAt = target.transform;
    }

    public void InitialiseTarget(GameObject targetToLookAt)
    {
        lookAt = targetToLookAt.transform;
    }

    private void Update()
    {
        currentX += Input.GetAxis("Mouse X");
        currentY += Input.GetAxis("Mouse Y");

        currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);
    }

    private void LateUpdate()
    {
        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        camPos.position = lookAt.position + rotation * dir;
        camPos.LookAt(lookAt.position);
    }
}
