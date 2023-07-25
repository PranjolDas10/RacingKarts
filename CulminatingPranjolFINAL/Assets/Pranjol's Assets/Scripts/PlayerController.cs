using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PlayerController : MonoBehaviour
{
    internal enum driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }
    [SerializeField] private driveType drive;

    private InputManager IM;
    private CarEffects CarEffects;
    private Gamemanager GameMan;

    public WheelCollider[] wheels = new WheelCollider[4];
    public GameObject[] wheelMesh = new GameObject[4];
    public float motorTorque = 200;
    public float steeringMax = 25;
    private float radius = 6;
    private float horizontal;
    private float vertical;
    [HideInInspector] public float KPH;
    private float brakPower = 0;
    private float totalPower;
    private Rigidbody rigidbody;

    /*[HideInInspector]*/
    public float nitrusValue;
    [HideInInspector] public bool nitrusFlag = false;

    public bool hasFinished;
    public string carName;

    private int finishLineCounter = 0;
    public GameObject finishPanel;
    public TextMeshProUGUI raceTimeText;

    //private float raceTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        getObjects();
    }

    private void Update()
    {
    }

    void FixedUpdate()
    {
        animateWheels();
        moveVehicle();
        steerVehicle();
        if (gameObject.tag == "AI") return;
        activateNitrus();
        if (GameMan.raceStarted == true)
        {
            GameMan.raceTime += Time.deltaTime;
        }
    }

    private void steerVehicle()
    {
        //steering
        //acerman steering formula
        //steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * horizontalInput;

        if (IM.horizontal > 0)
        {
            //rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * IM.horizontal;
        }
        else if (IM.horizontal < 0)
        {
            wheels[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius - (1.5f / 2))) * IM.horizontal;
            wheels[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (radius + (1.5f / 2))) * IM.horizontal;
            //transform.Rotate(Vector3.up * steerHelping);

        }
        else
        {
            wheels[0].steerAngle = 0;
            wheels[1].steerAngle = 0;
        }
    }

    // Move the vehicle based on the input
    private void moveVehicle()
    {
        // Movement logic goes here

        brakeVehicle();

        if (drive == driveType.allWheelDrive)
        {
            // All-wheel drive
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = IM.vertical * (motorTorque / 4);
            }
        }
        else if (drive == driveType.rearWheelDrive)
        {
            // Rear-wheel drive
            for (int i = 2; i < wheels.Length; i++)
            {
                wheels[i].motorTorque = IM.vertical * (motorTorque / 2);
            }
        }
        else
        {
            // Front-wheel drive
            for (int i = 0; i < wheels.Length - 2; i++)
            {
                wheels[i].motorTorque = IM.vertical * (motorTorque / 2);
            }
        }

        KPH = rigidbody.velocity.magnitude * 3.6f;
    }

    // Apply braking to the vehicle
    private void brakeVehicle()
    {
        if (vertical < 0)
        {
            brakPower = (KPH >= 10) ? 500 : 0;
        }
        else if (vertical == 0 && (KPH <= 10 || KPH >= -10))
        {
            brakPower = 10;
        }
        else
        {
            brakPower = 0;
        }
    }

    // Animate the wheels to match the collider positions
    void animateWheels()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;
        for (int i = 0; i < 4; i++)
        {
            wheels[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;
        }
    }

    // Get references to the required objects
    private void getObjects()
    {
        IM = GetComponent<InputManager>();
        rigidbody = GetComponent<Rigidbody>();
        CarEffects = GetComponent<CarEffects>();
        GameMan = GetComponent<Gamemanager>();
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject != null)
        {
            GameMan = gameManagerObject.GetComponent<Gamemanager>();
        }
        else
        {
            Debug.LogError("GameManager object not found!");
        }
    }

    // Activate the nitrus boost
    public void activateNitrus()
    {
        if (!IM.boosting && nitrusValue <= 10)
        {
            nitrusValue += Time.deltaTime / 2;
        }
        else
        {
            nitrusValue -= (nitrusValue <= 0) ? 0 : Time.deltaTime;
        }

        if (IM.boosting)
        {
            if (nitrusValue > 0)
            {
                CarEffects.startNitrusEmitter();
                rigidbody.AddForce(transform.forward * 10000);
            }
            else
            {
                CarEffects.stopNitrusEmitter();
            }
        }
        else
        {
            CarEffects.stopNitrusEmitter();
        }
    }

    // Handle collision with the finish line
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish") && GameMan.raceStarted && !gameObject.CompareTag("AI"))
        {
            finishLineCounter++;

            if (finishLineCounter == 2)
            {
                StopRace();
            }
        }
    }

    // Stop the race and display the finish panel
    private void StopRace()
    {
        StartCoroutine(PauseGameAfterDelay(1f));
    }

    // Pause the game after a delay and activate the finish panel
    private IEnumerator PauseGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Time.timeScale = 0f;  // Pause the game

        // Activate the finish panel
        finishPanel.SetActive(true);
        raceTimeText.text = "Race finished! Time: " + GameMan.raceTime.ToString("0.00") + " seconds";
    }
}
