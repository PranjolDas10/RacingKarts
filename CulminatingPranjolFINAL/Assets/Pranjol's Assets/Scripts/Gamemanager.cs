using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Gamemanager : MonoBehaviour
{
    // Public variables
    public PlayerController RR;
    public GameObject needle;
    public Slider nitrusSlider;

    // Private variables
    private float startPosiziton = 220f, endPosition = -49f;
    private float desiredPosition;
    public float vehicleSpeed;

    private List<GameObject> temporaryList;

    public float timeLeft = 4;
    public Text timeLeftText;
    public bool countdownFlag = false;

    private GameObject[] fullArray;
    public GameObject[] presentGameObjectVehicles;
    public Text currentPosition;
    public List<vehicle> presentVehicles; // List to store vehicle information
    private GameObject[] temporaryArray;
    private bool arrarDisplayed = false;

    public float raceTime = 0f;
    public bool raceStarted = false;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        temporaryList = new List<GameObject>();

        // Find AI carts and add them to the temporary list
        GameObject[] aiCarts = GameObject.FindGameObjectsWithTag("AI");
        temporaryList.AddRange(aiCarts);

        // Add the player's cart to the temporary list
        temporaryList.Add(RR.gameObject);

        // Convert the temporary list to an array and assign it to fullArray
        fullArray = temporaryList.ToArray();

        // Find AI carts and assign them to presentGameObjectVehicles
        presentGameObjectVehicles = GameObject.FindGameObjectsWithTag("AI");

        // Initialize the presentVehicles list
        presentVehicles = new List<vehicle>();

        // Add vehicle information for AI carts to presentVehicles list
        foreach (GameObject R in presentGameObjectVehicles)
        {
            presentVehicles.Add(new vehicle(R.GetComponent<InputManager>().currentNode, R.GetComponent<PlayerController>().carName, R.GetComponent<PlayerController>().hasFinished));
        }

        // Add vehicle information for player's cart to presentVehicles list
        presentVehicles.Add(new vehicle(RR.gameObject.GetComponent<InputManager>().currentNode, RR.carName, RR.hasFinished));

        // Initialize the temporaryArray with the size of presentVehicles list
        temporaryArray = new GameObject[presentVehicles.Count];

        // Start the timed loop coroutine
        StartCoroutine(timedLoop());
    }

    // This method is called when the script starts
    void Start()
    {

    }

    // FixedUpdate is called at a fixed interval
    private void FixedUpdate()
    {
        vehicleSpeed = RR.KPH;
        RR.KPH = vehicleSpeed;
        updateNeedle();
        nitrusUI();
        countDownTimer();
    }

    // Sort the array based on vehicle information
    private void sortArray()
    {
        for (int i = 0; i < fullArray.Length; i++)
        {
            presentVehicles[i].hasFinished = fullArray[i].GetComponent<PlayerController>().hasFinished;
            presentVehicles[i].name = fullArray[i].GetComponent<PlayerController>().carName;
            presentVehicles[i].node = fullArray[i].GetComponent<InputManager>().currentNode;
        }

        // Sort the presentVehicles list based on the node value
        if (!RR.hasFinished)
        {
            for (int i = 0; i < presentVehicles.Count; i++)
            {
                for (int j = i + 1; j < presentVehicles.Count; j++)
                {
                    if (presentVehicles[j].node < presentVehicles[i].node)
                    {
                        vehicle QQ = presentVehicles[i];
                        presentVehicles[i] = presentVehicles[j];
                        presentVehicles[j] = QQ;
                    }
                }
            }
        }

        // Update the displayed array
        if (arrarDisplayed)
        {
            for (int i = 0; i < temporaryArray.Length; i++)
            {
                temporaryArray[i].transform.Find("vehicle node").gameObject.GetComponent<Text>().text = (presentVehicles[i].hasFinished) ? "finished" : "";
                temporaryArray[i].transform.Find("vehicle name").gameObject.GetComponent<Text>().text = presentVehicles[i].name.ToString();
            }
        }

        // Reverse the presentVehicles list
        presentVehicles.Reverse();

        // Update the current position text based on the player's cart
        for (int i = 0; i < temporaryArray.Length; i++)
        {
            if (RR.carName == presentVehicles[i].name)
                currentPosition.text = ((i + 1) + "/" + presentVehicles.Count).ToString();
        }
    }

    // Update the needle rotation based on the vehicle speed
    public void updateNeedle()
    {
        desiredPosition = startPosiziton - endPosition;
        float temp = vehicleSpeed / 180;
        needle.transform.eulerAngles = new Vector3(0, 0, (startPosiziton - temp * desiredPosition));
    }

    // Update the nitrus UI slider
    public void nitrusUI()
    {
        nitrusSlider.value = RR.nitrusValue / 45;
    }

    // Manage the countdown timer
    private void countDownTimer()
    {
        if (timeLeft <= -5) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            unfreezePlayers();
            raceStarted = true;
        }
        else freezePlayers();

        if (timeLeft > 1) timeLeftText.text = timeLeft.ToString("0");
        else if (timeLeft >= -1 && timeLeft <= 1) timeLeftText.text = "GO!";
        else timeLeftText.text = "";
    }

    // Freeze the players during countdown
    private void freezePlayers()
    {
        if (countdownFlag) return;
        foreach (GameObject D in fullArray)
        {
            D.GetComponent<Rigidbody>().isKinematic = true;
        }
        countdownFlag = true;
    }

    // Unfreeze the players after countdown
    private void unfreezePlayers()
    {
        if (!countdownFlag) return;
        foreach (GameObject D in fullArray)
        {
            D.GetComponent<Rigidbody>().isKinematic = false;
        }
        countdownFlag = false;
    }

    // Coroutine for timed loop to continuously sort the array
    private IEnumerator timedLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(.7f);
            sortArray();
        }
    }

    public void RestartRace()
    {
        // Restart the race by reloading the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
}