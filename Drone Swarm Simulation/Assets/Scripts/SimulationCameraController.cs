using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimulationCameraController : MonoBehaviour
{
    public Camera mainCamera;       // assign in Inspector
    public float simWidth = 10f;    // simulation bounds width
    public float simHeight = 5f;    // simulation bounds height

    void Start()
    {
        AdjustCamera();
    }

    void AdjustCamera()
    {
        float targetAspect = simWidth / simHeight;
        float panelAspect = (float)Screen.width / Screen.height;

        Camera simCam = GetComponent<Camera>();

        if (panelAspect >= targetAspect)
        {
            // wider than simulation → base on height
            simCam.orthographicSize = simHeight / 2f;
        }
        else
        {
            // taller than simulation → base on width
            simCam.orthographicSize = (simWidth / panelAspect) / 2f;
        }

        // center camera on simulation
        simCam.transform.position = new Vector3(simWidth / 2f, simHeight / 2f, -10f);
    }
}
