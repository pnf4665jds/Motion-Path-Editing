using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{

    public float translationSensitivity = 2;
    public float zoomSensitiviy = 10;

    public float rotationSensitiviry = 2;

    public string mouseHorizontalAxisName = "Mouse X";
    public string mouseVerticalAxisName = "Mouse Y";
    public string scrollAxisName = "Mouse ScrollWheel";

    Camera _camera;

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        //  translation
        float translateX = 0;
        float translateY = 0;

        if (Input.GetMouseButton(2))
        {
            translateY = Input.GetAxis(mouseVerticalAxisName) * translationSensitivity;
            translateX = Input.GetAxis(mouseHorizontalAxisName) * translationSensitivity;
        }


        float zoom = Input.GetAxis(scrollAxisName) * zoomSensitiviy;

        transform.Translate(-translateX, -translateY, zoom);

        // rotation

        float rotationX = 0;
        float rotationY = 0;

        if (Input.GetMouseButton(1))
        {
            rotationX = Input.GetAxis(mouseVerticalAxisName) * rotationSensitiviry;
            rotationY = Input.GetAxis(mouseHorizontalAxisName) * rotationSensitiviry;
        }

        transform.Rotate(0, rotationY, 0, Space.World);
        transform.Rotate(-rotationX, 0, 0);

        
    }

   
}