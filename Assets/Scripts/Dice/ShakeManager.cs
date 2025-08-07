using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("External References")]
    public Transform rollToDirection;

    [Header("Settings")]
    public float forceMultiplier = 100f;
    public float minShakeThreshold = 1.2f;
    public float rotationIntensity = 0.05f;
    public float moveSpeed = 2f;
    [SerializeField][Range(1, 10)] private int sensitivity = 1;

    [Header("Shake Limits")]
    [SerializeField][Range(1f, 100f)] private float maxShakeIntensity = 20f;

    [Header("Phone Input")]
    [SerializeField] private bool useDeviceMotion = true;
    [SerializeField] private float gyroRotationMultiplier = 3.0f;
    [SerializeField] private float accelMoveMultiplier = 0.3f;
    [SerializeField] private float gyroDeadzone = 0.15f;
    [SerializeField] private float accelDeadzone = 0.15f;

    [Header("Debug")]
    public float shakeIntensity;
    private DiceManager[] diceArray;

    private Vector3 debugGyroRate;
    private Vector3 debugAcceleration;

    // Cache frequently used values
    private float cachedActivationThreshold;
    private bool thresholdCacheDirty = true;

    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }

        RefreshDiceList();
    }

    void FixedUpdate()
    {
        if (useDeviceMotion)
        {
            ReadPhoneMotion();
        }

        MoveRollDirection();
        FindShakeIntensity();
        RollDice();
    }

    void ReadPhoneMotion()
    {
        debugGyroRate = Input.gyro.rotationRate;
        debugAcceleration = Input.acceleration;
    }

    void MoveRollDirection()
    {
        if (rollToDirection == null) return;

        Vector3 currentPos = rollToDirection.position;
        rollToDirection.position = new Vector3(currentPos.x, currentPos.y, rollToDirection.position.z);
    }

    void FindShakeIntensity()
    {
        float linear = debugAcceleration.magnitude;
        float angular = debugGyroRate.magnitude;

        if (linear < accelDeadzone) linear = 0f;
        if (angular < gyroDeadzone) angular = 0f;

        float rawIntensity = (linear * accelMoveMultiplier) + (angular * gyroRotationMultiplier);
        shakeIntensity = Mathf.Min(rawIntensity, maxShakeIntensity);
    }

    float GetActivationThreshold()
    {
        // Cache the threshold calculation since sensitivity doesn't change often
        if (thresholdCacheDirty)
        {
            float minThreshold = 9.0f;
            float maxThreshold = 1.0f;
            cachedActivationThreshold = Mathf.Lerp(minThreshold, maxThreshold, (sensitivity - 1f) / 9f);
            thresholdCacheDirty = false;
        }
        return cachedActivationThreshold;
    }

    // Call this when sensitivity changes in inspector or at runtime
    public void InvalidateThresholdCache()
    {
        thresholdCacheDirty = true;
    }

    void RollDice()
    {
        if (diceArray == null || rollToDirection == null) return;

        float activationThreshold = GetActivationThreshold();

        if (shakeIntensity < activationThreshold) return;

        // Check if there are any valid dice before applying force and vibration
        bool hasValidDice = false;
        foreach (var dice in diceArray)
        {
            if (dice != null)
            {
                hasValidDice = true;
                break;
            }
        }

        if (!hasValidDice) return;

        // Trigger vibration only if there are dice to affect
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif

        foreach (var dice in diceArray)
        {
            if (dice == null) continue;

            // Cache transform position to avoid multiple property calls
            Vector3 dicePosition = dice.transform.position;
            Vector3 forceDir = (rollToDirection.position - dicePosition).normalized;

            // Pre-calculate random offset values
            Vector3 randomOffset = new Vector3(
                Random.Range(-rotationIntensity, rotationIntensity),
                Random.Range(-rotationIntensity, rotationIntensity),
                Random.Range(-rotationIntensity, rotationIntensity)
            );
            Vector3 forcePoint = dicePosition + randomOffset;

            // Cache the force calculation
            Vector3 appliedForce = forceDir * shakeIntensity * forceMultiplier;
            dice.ApplyRollForce(appliedForce, forcePoint);
        }
    }

    public void RefreshDiceList()
    {
        diceArray = FindObjectsByType<DiceManager>(FindObjectsSortMode.None);
    }
}
