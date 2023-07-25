using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject Player;
    private PlayerController RR;
    private GameObject cameralookAt, cameraPos;
    private float speed = 0;
    private float defaltFOV = 0, desiredFOV = 0;
    [Range(0, 50)] public float smothTime = 8;

    private void Start()
    {
        // Find and assign the necessary game objects and components
        Player = GameObject.FindGameObjectWithTag("Player");
        RR = Player.GetComponent<PlayerController>();
        cameralookAt = Player.transform.Find("Camera LookAt").gameObject;
        cameraPos = Player.transform.Find("Camera Constraint").gameObject;

        // Initialize default and desired field of view values
        defaltFOV = Camera.main.fieldOfView;
        desiredFOV = defaltFOV + 15;
    }

    private void FixedUpdate()
    {
        // Follow the player and adjust the field of view
        follow();
        boostFOV();
    }

    // Follow the player's movement
    private void follow()
    {
        speed = RR.KPH / smothTime;
        gameObject.transform.position = Vector3.Lerp(transform.position, cameraPos.transform.position, Time.deltaTime * speed);
        gameObject.transform.LookAt(cameralookAt.gameObject.transform.position);
    }

    // Adjust the field of view based on nitrus usage
    private void boostFOV()
    {
        if (RR.nitrusFlag)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, desiredFOV, Time.deltaTime * 5);
        }
        else
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, defaltFOV, Time.deltaTime * 5);
        }
    }
}