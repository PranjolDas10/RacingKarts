using UnityEngine;

/// <summary>
/// Singleton audio manager. Subscribes to <see cref="RaceEventBus"/> events and
/// plays appropriate clips without any game system needing a direct reference here.
/// Assign clips in the Inspector; unassigned slots are silently skipped.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip raceMusic;
    [SerializeField] private AudioClip countdownMusic;

    [Header("SFX")]
    [SerializeField] private AudioClip countdownBeep;
    [SerializeField] private AudioClip countdownGo;
    [SerializeField] private AudioClip finishLine;

    [Header("Engine")]
    [SerializeField] private AudioClip engineLoop;
    [SerializeField][Range(0.5f, 3f)] private float minPitch = 0.6f;
    [SerializeField][Range(0.5f, 3f)] private float maxPitch = 2.2f;

    [Header("Volumes")]
    [SerializeField][Range(0f, 1f)] private float musicVolume  = 0.4f;
    [SerializeField][Range(0f, 1f)] private float sfxVolume    = 0.8f;
    [SerializeField][Range(0f, 1f)] private float engineVolume = 0.6f;

    private AudioSource _music;
    private AudioSource _sfx;
    private AudioSource _engine;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _music  = AddSource(musicVolume,  loop: true);
        _sfx    = AddSource(sfxVolume,    loop: false);
        _engine = AddSource(engineVolume, loop: true);

        if (countdownMusic) PlayMusic(countdownMusic);
    }

    private void OnEnable()
    {
        RaceEventBus.OnRaceStarted  += HandleRaceStarted;
        RaceEventBus.OnRaceFinished += HandleRaceFinished;
        RaceEventBus.OnSpeedChanged += HandleSpeedChanged;
    }

    private void OnDisable()
    {
        RaceEventBus.OnRaceStarted  -= HandleRaceStarted;
        RaceEventBus.OnRaceFinished -= HandleRaceFinished;
        RaceEventBus.OnSpeedChanged -= HandleSpeedChanged;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandleRaceStarted()
    {
        PlaySFX(countdownGo);
        PlayMusic(raceMusic);

        if (engineLoop)
        {
            _engine.clip = engineLoop;
            _engine.Play();
        }
    }

    private void HandleRaceFinished(string winner, float time)
    {
        PlaySFX(finishLine);
        _music.Stop();
        _engine.Stop();
    }

    private void HandleSpeedChanged(float kph, float maxKph)
    {
        if (!_engine.isPlaying || maxKph <= 0f) return;
        _engine.pitch = Mathf.Lerp(minPitch, maxPitch, kph / maxKph);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void PlayCountdownBeep() => PlaySFX(countdownBeep);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void PlaySFX(AudioClip clip)
    {
        if (clip) _sfx.PlayOneShot(clip);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (!clip) return;
        _music.clip = clip;
        _music.Play();
    }

    private AudioSource AddSource(float volume, bool loop)
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.volume     = volume;
        src.loop       = loop;
        src.playOnAwake = false;
        return src;
    }
}
