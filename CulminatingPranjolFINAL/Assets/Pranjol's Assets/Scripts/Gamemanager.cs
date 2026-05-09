using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Central race coordinator. Manages the pre-race countdown, freeze/unfreeze
/// cycle, and live standings. Subscribes to <see cref="RaceEventBus"/> events
/// so that HUD elements update reactively rather than being polled every frame.
/// </summary>
public class Gamemanager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Player Reference")]
    public PlayerController RR;

    [Header("HUD — Speedometer")]
    public GameObject needle;
    [SerializeField] private float needleStartAngle = 220f;   // angle at 0 KPH
    [SerializeField] private float needleEndAngle   = -49f;   // angle at maxSpeed KPH
    [SerializeField] private float maxSpeedKph      = 180f;

    [Header("HUD — Nitro")]
    public Slider nitrusSlider;

    [Header("HUD — Position")]
    public Text currentPosition;

    [Header("HUD — Countdown")]
    public float timeLeft = 4f;
    public Text  timeLeftText;
    [HideInInspector] public bool countdownFlag;

    [Header("Standings — Leaderboard")]
    public GameObject[] presentGameObjectVehicles;
    public List<vehicle> presentVehicles = new List<vehicle>();
    [Tooltip("Set to true once the leaderboard UI row objects are assigned.")]
    [SerializeField] private bool leaderboardReady;

    [Header("Race State")]
    public float raceTime;
    public bool  raceStarted;

    // ── Private ───────────────────────────────────────────────────────────────

    private List<GameObject> _allVehicles = new List<GameObject>();
    private GameObject[]     _leaderboardRows;

    // ── Unity messages ────────────────────────────────────────────────────────

    private void Awake()
    {
        BuildVehicleList();
        StartCoroutine(StandingsUpdateLoop());
    }

    private void OnEnable()
    {
        RaceEventBus.OnSpeedChanged += HandleSpeedChanged;
        RaceEventBus.OnNitroChanged += HandleNitroChanged;
        RaceEventBus.OnPositionChanged += HandlePositionChanged;
    }

    private void OnDisable()
    {
        RaceEventBus.OnSpeedChanged -= HandleSpeedChanged;
        RaceEventBus.OnNitroChanged -= HandleNitroChanged;
        RaceEventBus.OnPositionChanged -= HandlePositionChanged;
    }

    private void FixedUpdate()
    {
        CountDownTimer();
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    private void BuildVehicleList()
    {
        GameObject[] ai = GameObject.FindGameObjectsWithTag("AI");
        _allVehicles.AddRange(ai);
        _allVehicles.Add(RR.gameObject);

        foreach (GameObject go in ai)
        {
            var im = go.GetComponent<InputManager>();
            var pc = go.GetComponent<PlayerController>();
            presentVehicles.Add(new vehicle(im.currentNode, pc.carName, pc.hasFinished));
        }

        var playerIm = RR.GetComponent<InputManager>();
        presentVehicles.Add(new vehicle(playerIm.currentNode, RR.carName, RR.hasFinished));

        _leaderboardRows = presentGameObjectVehicles;
        leaderboardReady = _leaderboardRows != null && _leaderboardRows.Length == presentVehicles.Count;
    }

    // ── Event-driven HUD updates ──────────────────────────────────────────────

    private void HandleSpeedChanged(float kph, float maxKph)
    {
        if (needle == null) return;
        float range  = needleStartAngle - needleEndAngle;
        float angle  = needleStartAngle - (kph / maxKph) * range;
        needle.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    private void HandleNitroChanged(float normalized)
    {
        if (nitrusSlider != null)
            nitrusSlider.value = normalized;
    }

    private void HandlePositionChanged(int position, int total)
    {
        if (currentPosition != null)
            currentPosition.text = $"{position}/{total}";
    }

    // ── Countdown ─────────────────────────────────────────────────────────────

    private void CountDownTimer()
    {
        if (timeLeft <= -5f) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            UnfreezePlayers();
            if (!raceStarted)
            {
                raceStarted = true;
                RaceEventBus.PublishRaceStarted();
            }
        }
        else
        {
            FreezePlayers();
        }

        if      (timeLeft > 1f)                  timeLeftText.text = timeLeft.ToString("0");
        else if (timeLeft >= -1f && timeLeft <= 1f) timeLeftText.text = "GO!";
        else                                       timeLeftText.text = "";
    }

    private void FreezePlayers()
    {
        if (countdownFlag) return;
        foreach (GameObject v in _allVehicles) v.GetComponent<Rigidbody>().isKinematic = true;
        countdownFlag = true;
    }

    private void UnfreezePlayers()
    {
        if (!countdownFlag) return;
        foreach (GameObject v in _allVehicles) v.GetComponent<Rigidbody>().isKinematic = false;
        countdownFlag = false;
    }

    // ── Standings ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Refreshes live data from each vehicle component, sorts standings using
    /// O(n log n) comparison sort, then publishes the player's position to the
    /// event bus and (optionally) updates the on-screen leaderboard rows.
    /// </summary>
    private void RefreshStandings()
    {
        // Sync live data
        for (int i = 0; i < _allVehicles.Count; i++)
        {
            var pc = _allVehicles[i].GetComponent<PlayerController>();
            var im = _allVehicles[i].GetComponent<InputManager>();
            presentVehicles[i].node        = im.currentNode;
            presentVehicles[i].name        = pc.carName;
            presentVehicles[i].hasFinished = pc.hasFinished;
        }

        // Sort descending: highest waypoint node index = furthest ahead
        if (!RR.hasFinished)
            presentVehicles.Sort((a, b) => b.node.CompareTo(a.node));

        // Update optional leaderboard UI rows
        if (leaderboardReady)
        {
            for (int i = 0; i < _leaderboardRows.Length; i++)
            {
                var rowVehicle = presentVehicles[i];
                var nameLabel  = _leaderboardRows[i].transform.Find("vehicle name")?.GetComponent<Text>();
                var nodeLabel  = _leaderboardRows[i].transform.Find("vehicle node")?.GetComponent<Text>();
                if (nameLabel) nameLabel.text = rowVehicle.name;
                if (nodeLabel) nodeLabel.text = rowVehicle.hasFinished ? "Finished" : "";
            }
        }

        // Derive and publish player position
        for (int i = 0; i < presentVehicles.Count; i++)
        {
            if (presentVehicles[i].name == RR.carName)
            {
                RaceEventBus.PublishPositionChanged(i + 1, presentVehicles.Count);
                break;
            }
        }
    }

    private IEnumerator StandingsUpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            RefreshStandings();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void RestartRace()
    {
        RaceEventBus.ClearAllListeners();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
