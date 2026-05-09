using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Core vehicle physics controller. Handles drivetrain torque distribution,
/// Ackermann-geometry steering, braking, and the nitro boost system.
/// Publishes state to <see cref="RaceEventBus"/> each physics tick so that UI
/// and audio systems react without polling or direct coupling.
/// </summary>
public class PlayerController : MonoBehaviour
{
    internal enum DriveType { FrontWheelDrive, RearWheelDrive, AllWheelDrive }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Drivetrain")]
    [SerializeField] private DriveType drive = DriveType.AllWheelDrive;

    [Header("Wheel References")]
    public WheelCollider[] wheels    = new WheelCollider[4];
    public GameObject[]    wheelMesh = new GameObject[4];

    [Header("Engine")]
    public float motorTorque = 400f;

    [Header("Steering — Ackermann Geometry")]
    [SerializeField] private float wheelBase   = 2.55f; // front–rear axle distance (m)
    [SerializeField] private float rearTrack   = 1.5f;  // lateral rear-wheel separation (m)
    [SerializeField] private float turnRadius  = 6f;

    [Header("Braking")]
    [SerializeField] private float brakeTorque          = 500f;
    [SerializeField] private float idleBrakeTorque      = 10f;
    [SerializeField] private float brakeSpeedThreshold  = 10f;  // KPH

    [Header("Nitro")]
    [Tooltip("Optional ScriptableObject preset — overrides the fields below when assigned.")]
    [SerializeField] private KartStats stats;

    public  float nitrusValue;
    [HideInInspector] public bool nitrusFlag;

    [SerializeField] private float nitroCapacity     = 10f;
    [SerializeField] private float nitroRechargeRate = 0.5f;  // units / sec
    [SerializeField] private float nitroDrainRate    = 1f;    // units / sec
    [SerializeField] private float nitroBoostForce   = 5000f; // Newtons

    [Header("Race")]
    public bool   hasFinished;
    public string carName;
    public GameObject        finishPanel;
    public TextMeshProUGUI   raceTimeText;
    [SerializeField] private int totalLaps = 2;

    // ── Runtime state ─────────────────────────────────────────────────────────

    [HideInInspector] public float KPH;

    private InputManager IM;
    private CarEffects   carEffects;
    private Gamemanager  gameManager;
    private Rigidbody    rb;
    private float        brakePower;
    private int          lapCount;

    // ── Unity messages ────────────────────────────────────────────────────────

    private void Start()
    {
        IM         = GetComponent<InputManager>();
        rb         = GetComponent<Rigidbody>();
        carEffects = GetComponent<CarEffects>();

        GameObject gm = GameObject.Find("GameManager");
        if (gm != null) gameManager = gm.GetComponent<Gamemanager>();
        else Debug.LogError("GameManager not found in scene.");

        ApplyKartStatsPreset();
    }

    private void FixedUpdate()
    {
        AnimateWheels();
        ApplySteering();
        ApplyDrive();
        KPH = rb.velocity.magnitude * 3.6f;

        if (gameObject.CompareTag("AI")) return;

        ApplyNitro();
        RaceEventBus.PublishSpeedChanged(KPH, 180f);
        RaceEventBus.PublishNitroChanged(nitrusValue / nitroCapacity);

        if (gameManager != null && gameManager.raceStarted)
            gameManager.raceTime += Time.deltaTime;
    }

    // ── Physics ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Ackermann steering geometry: the inner wheel follows a tighter arc than
    /// the outer wheel, eliminating tyre scrub through corners.
    /// </summary>
    private void ApplySteering()
    {
        float input = IM.horizontal;
        if (Mathf.Approximately(input, 0f))
        {
            wheels[0].steerAngle = 0f;
            wheels[1].steerAngle = 0f;
            return;
        }

        float inner = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrack * 0.5f)) * input;
        float outer = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrack * 0.5f)) * input;

        wheels[0].steerAngle = (input > 0f) ? inner : outer;
        wheels[1].steerAngle = (input > 0f) ? outer : inner;
    }

    private void ApplyDrive()
    {
        ApplyBrakes();

        float torque = IM.vertical * motorTorque;
        switch (drive)
        {
            case DriveType.AllWheelDrive:
                foreach (var w in wheels) w.motorTorque = torque * 0.25f;
                break;
            case DriveType.RearWheelDrive:
                wheels[2].motorTorque = wheels[3].motorTorque = torque * 0.5f;
                break;
            default: // FrontWheelDrive
                wheels[0].motorTorque = wheels[1].motorTorque = torque * 0.5f;
                break;
        }
    }

    private void ApplyBrakes()
    {
        if (IM.vertical < 0f && KPH >= brakeSpeedThreshold)
            brakePower = brakeTorque;
        else if (Mathf.Approximately(IM.vertical, 0f))
            brakePower = idleBrakeTorque;
        else
            brakePower = 0f;

        foreach (var w in wheels) w.brakeTorque = brakePower;
    }

    public void ApplyNitro()
    {
        bool boosting = IM.boosting && nitrusValue > 0f;

        if (boosting)
        {
            nitrusValue = Mathf.Max(0f, nitrusValue - nitroDrainRate * Time.deltaTime);
            rb.AddForce(transform.forward * nitroBoostForce);
            carEffects.startNitrusEmitter();
        }
        else
        {
            carEffects.stopNitrusEmitter();
            if (nitrusValue < nitroCapacity)
                nitrusValue = Mathf.Min(nitroCapacity, nitrusValue + nitroRechargeRate * Time.deltaTime);
        }
    }

    private void AnimateWheels()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheelMesh[i].transform.SetPositionAndRotation(pos, rot);
        }
    }

    // ── Finish line ───────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Finish") || gameManager == null || !gameManager.raceStarted)
            return;

        lapCount++;
        RaceEventBus.PublishLapCompleted(lapCount, totalLaps);

        if (lapCount >= totalLaps && !hasFinished)
            StartCoroutine(FinishRace());
    }

    private IEnumerator FinishRace()
    {
        hasFinished = true;
        RaceEventBus.PublishRaceFinished(carName, gameManager.raceTime);
        yield return new WaitForSeconds(1f);
        Time.timeScale = 0f;
        finishPanel.SetActive(true);
        raceTimeText.text = $"Race finished! Time: {gameManager.raceTime:0.00}s";
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// When a <see cref="KartStats"/> preset is assigned, its values replace the
    /// individually serialized fields — allowing hot-swappable kart configurations.
    /// </summary>
    private void ApplyKartStatsPreset()
    {
        if (stats == null) return;

        motorTorque       = stats.maxMotorTorque;
        wheelBase         = stats.wheelBase;
        rearTrack         = stats.rearTrack;
        turnRadius        = stats.turnRadius;
        brakeTorque       = stats.brakeTorque;
        idleBrakeTorque   = stats.idleBrakeTorque;
        brakeSpeedThreshold = stats.brakeSpeedThreshold;
        nitroCapacity     = stats.nitroCapacity;
        nitroRechargeRate = stats.nitroRechargeRate;
        nitroDrainRate    = stats.nitroDrainRate;
        nitroBoostForce   = stats.nitroBoostForce;
    }
}
