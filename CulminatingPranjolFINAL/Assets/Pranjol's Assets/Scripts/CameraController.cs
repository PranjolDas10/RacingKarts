using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Third-person follow camera with speed-reactive smoothing and dynamic FOV
/// during nitro boost. All camera data flows from the player's
/// <see cref="PlayerController"/> so this component has no direct UI coupling.
/// </summary>
public class CameraController : MonoBehaviour
{
    // FormerlySerializedAs preserves the saved Inspector value after a rename.
    [FormerlySerializedAs("smothTime")]
    [Range(0f, 50f)] public float smoothTime = 8f;

    [SerializeField] private float fovLerpSpeed = 5f;

    private PlayerController _player;
    private Transform        _lookAtTarget;
    private Transform        _mountPoint;
    private float            _defaultFov;
    private float            _boostedFov;

    private void Start()
    {
        GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo == null) { Debug.LogError("CameraController: no Player tag found."); return; }

        _player      = playerGo.GetComponent<PlayerController>();
        _lookAtTarget = playerGo.transform.Find("Camera LookAt");
        _mountPoint   = playerGo.transform.Find("Camera Constraint");

        _defaultFov = Camera.main.fieldOfView;
        _boostedFov = _defaultFov + 15f;
    }

    private void FixedUpdate()
    {
        Follow();
        UpdateFov();
    }

    private void Follow()
    {
        if (_mountPoint == null || _lookAtTarget == null) return;

        float speed = _player.KPH / smoothTime;
        transform.position = Vector3.Lerp(transform.position, _mountPoint.position, Time.deltaTime * speed);
        transform.LookAt(_lookAtTarget);
    }

    private void UpdateFov()
    {
        float targetFov = _player.nitrusFlag ? _boostedFov : _defaultFov;
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFov, Time.deltaTime * fovLerpSpeed);
    }
}
