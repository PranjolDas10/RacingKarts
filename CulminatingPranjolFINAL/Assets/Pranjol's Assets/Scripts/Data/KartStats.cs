using UnityEngine;

/// <summary>
/// Data-driven kart configuration via ScriptableObject.
/// Create kart presets in the Project window: Racing → Kart Stats.
/// Assign a KartStats asset to PlayerController.stats to override its
/// individual serialized fields, enabling quick tuning without code changes.
/// </summary>
[CreateAssetMenu(fileName = "KartStats", menuName = "Racing/Kart Stats", order = 0)]
public class KartStats : ScriptableObject
{
    [Header("Engine")]
    [Tooltip("Peak torque applied per driven wheel (Nm).")]
    public float maxMotorTorque = 400f;
    [Tooltip("Top speed used to normalize the speedometer (KPH).")]
    public float maxSpeed = 180f;

    [Header("Braking")]
    [Tooltip("Brake torque applied when reversing while moving forward.")]
    public float brakeTorque = 500f;
    [Tooltip("Light brake torque applied while coasting to prevent free-rolling.")]
    public float idleBrakeTorque = 10f;
    [Tooltip("Minimum speed (KPH) above which full brakes activate.")]
    public float brakeSpeedThreshold = 10f;

    [Header("Steering — Ackermann Geometry")]
    [Tooltip("Longitudinal distance between front and rear axles (m).")]
    public float wheelBase = 2.55f;
    [Tooltip("Lateral distance between the two rear wheels (m).")]
    public float rearTrack = 1.5f;
    [Tooltip("Turning circle radius at full lock (m).")]
    public float turnRadius = 6f;

    [Header("Nitro")]
    [Tooltip("Maximum nitro charge — arbitrary energy units.")]
    public float nitroCapacity = 10f;
    [Tooltip("Charge recovered per second while not boosting.")]
    public float nitroRechargeRate = 0.5f;
    [Tooltip("Charge consumed per second while boosting.")]
    public float nitroDrainRate = 1f;
    [Tooltip("Force in Newtons applied along the forward axis during boost.")]
    public float nitroBoostForce = 5000f;

    [Header("Camera")]
    [Tooltip("Extra FOV added during a nitro boost.")]
    public float boostFovIncrease = 15f;
}
