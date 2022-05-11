using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularMove : MonoBehaviour
{

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;



    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        yaw = Mathf.Clamp(yaw, -90f, 90f);       //the rotation range
        pitch = Mathf.Clamp(pitch, -90f, 90f);   //the rotation range

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

    }
}