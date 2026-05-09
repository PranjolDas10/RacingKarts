using UnityEngine;

/// <summary>
/// Manages vehicle particle effects. Toggled by <see cref="PlayerController"/>
/// during nitro activation/deactivation. AI karts share the same component but
/// are driven via direct method calls rather than input polling.
/// </summary>
public class CarEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] nitrusSmoke;

    private PlayerController _controller;

    private void Start()
    {
        _controller = GetComponent<PlayerController>();
    }

    /// <summary>Begins emitting nitro particles and sets the boost flag.</summary>
    public void startNitrusEmitter()
    {
        if (_controller.nitrusFlag) return;

        foreach (var ps in nitrusSmoke) ps.Play();
        _controller.nitrusFlag = true;
    }

    /// <summary>Stops emitting nitro particles and clears the boost flag.</summary>
    public void stopNitrusEmitter()
    {
        if (!_controller.nitrusFlag) return;

        foreach (var ps in nitrusSmoke) ps.Stop();
        _controller.nitrusFlag = false;
    }
}
