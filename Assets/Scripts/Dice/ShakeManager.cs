using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("Settings")]
    public float forceMultiplier = 100f;
    public float minShakeThreshold = 1.2f;
    public float rotationIntensity = 0.05f;
    public float moveSpeed = 2f;
    [SerializeField][Range(1, 10)] private int sensitivity = 1;

    // Property to handle sensitivity changes and cache invalidation
    public int Sensitivity
    {
        get => sensitivity;
        set
        {
            if (sensitivity != value)
            {
                sensitivity = Mathf.Clamp(value, 1, 10);
                thresholdCacheDirty = true;
            }
        }
    }

    [Header("Shake Limits")]
    [SerializeField][Range(1f, 100f)] private float maxShakeIntensity = 20f;

    [Header("Phone Input")]
    [SerializeField] private bool useDeviceMotion = true;
    [SerializeField] private float gyroRotationMultiplier = 3.0f;
    [SerializeField] private float accelMoveMultiplier = 0.3f;
    [SerializeField] private float gyroDeadzone = 0.2f;       // Reduced for better responsiveness
    [SerializeField] private float accelDeadzone = 0.25f;     // Reduced for better Z-axis detection

    [Header("Physics Direction")]
    [SerializeField] private float upwardForceComponent = 0.3f;
    [SerializeField] private float gyroInfluenceStrength = 0.2f;

    [Header("Direct Movement")]
    [SerializeField] private float directMovementMultiplier = 1.0f;    // How strongly phone movement affects dice
    [SerializeField] private float minimumUpwardForce = 0.2f;          // Minimum upward force to overcome gravity
    [SerializeField] private float zNegativeMultiplier = 1.2f;         // Makes Z- movement easier than Z+ (1.0 = equal, >1.0 = easier Z-)

    [Header("Natural Feel")]
    [SerializeField] private float intensitySmoothing = 0.8f;          // How much previous intensity affects current (0-1)
    [SerializeField] private bool useIntensitySmoothing = true;        // Enable/disable smoothing
    [SerializeField] private bool singlePushPerDirection = true;       // Only apply force once per shake direction
    [SerializeField] private float directionSensitivity = 0.3f;        // How much direction must change for new push

    [Header("Conflict Resolution")]
    [SerializeField] private float zAxisPriorityThreshold = 0.15f;     // Z-axis vs Y-axis priority threshold
    [SerializeField] private float yToZStrengthMultiplier = 0.8f;      // Y-axis influence on Z when active
    [SerializeField] private float gyroConflictReduction = 0.3f;       // How much to reduce gyro when linear movement is strong

    [Header("Smooth Shaking")]
    [SerializeField] private bool enableInputSmoothing = true;         // Enable input smoothing for rapid movements
    [SerializeField][Range(0.1f, 0.9f)] private float inputSmoothingStrength = 0.7f; // How much to smooth (higher = smoother)

    [Header("Debug")]
    public float shakeIntensity;
    public Vector3 currentShakeDirection;
    public Vector3 debugAcceleration;     // Made public for debugging
    public Vector3 debugGyroRate;         // Made public for debugging
    private DiceManager[] diceArray;

    // Optimized caching
    private float lastShakeIntensity;
    private Vector3 lastShakeDirection;
    private bool hasAppliedForceThisDirection = false;

    // Input smoothing for better behavior during rapid shaking
    private Vector3 smoothedAcceleration;
    private Vector3 smoothedGyroRate;

    // Cache frequently used values
    private float cachedActivationThreshold;
    private bool thresholdCacheDirty = true;

    // Performance optimization - reduce allocations
    private static readonly Vector3 UpVector = Vector3.up;
    private const float DeadzoneSqr = 0.01f; // Squared deadzone for faster comparisons

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
        if (!useDeviceMotion) return;

        ReadPhoneMotion();
        FindShakeIntensity();
        RollDice();
    }

    void ReadPhoneMotion()
    {
        // Get raw input data
        Vector3 rawAcceleration = Input.acceleration;
        Vector3 rawGyroRate = Input.gyro.rotationRate;

        // Apply input smoothing if enabled
        if (enableInputSmoothing)
        {
            smoothedAcceleration = Vector3.Lerp(smoothedAcceleration, rawAcceleration, inputSmoothingStrength);
            smoothedGyroRate = Vector3.Lerp(smoothedGyroRate, rawGyroRate, inputSmoothingStrength);

            // Use smoothed values for calculations
            debugAcceleration = smoothedAcceleration;
            debugGyroRate = smoothedGyroRate;
        }
        else
        {
            // Use raw input directly
            debugAcceleration = rawAcceleration;
            debugGyroRate = rawGyroRate;
        }
    }

    void FindShakeIntensity()
    {
        // Use squared magnitude for faster calculations where possible
        float linearMagnitude = debugAcceleration.magnitude;
        float angularMagnitude = debugGyroRate.magnitude;

        // Apply deadzones more efficiently
        if (linearMagnitude < accelDeadzone) linearMagnitude = 0f;
        if (angularMagnitude < gyroDeadzone) angularMagnitude = 0f;

        // Calculate raw intensity
        float currentRawIntensity = (linearMagnitude * accelMoveMultiplier) + (angularMagnitude * gyroRotationMultiplier);

        // Apply smoothing for more natural feel
        if (useIntensitySmoothing && lastShakeIntensity > 0.01f)
        {
            // Smooth intensity transitions for more natural feel
            float smoothedIntensity = Mathf.Lerp(lastShakeIntensity, currentRawIntensity, 1f - intensitySmoothing);
            shakeIntensity = Mathf.Min(Mathf.Max(currentRawIntensity, smoothedIntensity), maxShakeIntensity);
        }
        else
        {
            shakeIntensity = Mathf.Min(currentRawIntensity, maxShakeIntensity);
        }

        lastShakeIntensity = shakeIntensity;
    }

    Vector3 GetShakeDirection()
    {
        Vector3 shakeForce = Vector3.zero;

        if (!useDeviceMotion)
        {
            // Fallback random movement
            return new Vector3(
                Random.Range(-0.3f, 0.3f),
                minimumUpwardForce,
                Random.Range(-0.3f, 0.3f)
            ).normalized;
        }

        // Cache magnitude calculations to avoid redundant computation
        float accelMagnitude = debugAcceleration.magnitude;
        float gyroMagnitude = debugGyroRate.magnitude;

        bool hasAccelInput = accelMagnitude > accelDeadzone;
        bool hasGyroInput = gyroMagnitude > gyroDeadzone;

        // Process acceleration input
        if (hasAccelInput)
        {
            // Apply directional forces
            float multiplier = directMovementMultiplier;
            shakeForce.x = debugAcceleration.x * multiplier;

            // Z-axis control with priority system to prevent conflicts
            // Primary: Forward/back movement (Z-axis)
            float zFromAccel = -debugAcceleration.z * multiplier;

            // Apply bias to make Z- movement easier than Z+
            if (zFromAccel > 0) // Z- movement (negative acceleration becomes positive force)
            {
                zFromAccel *= zNegativeMultiplier;
            }

            shakeForce.z = zFromAccel;

            // Secondary: Up/down movement only if forward/back is minimal
            float zFromY = 0f;
            if (Mathf.Abs(debugAcceleration.z) < zAxisPriorityThreshold)
            {
                // Use Y-axis for Z control when no significant Z movement
                zFromY = -debugAcceleration.y * multiplier * yToZStrengthMultiplier;

                // Apply bias to make Z- movement easier than Z+
                if (zFromY > 0) // Z- movement
                {
                    zFromY *= zNegativeMultiplier;
                }

                shakeForce.z += zFromY;
            }

            // Debug: Print movement priorities and Z-force breakdown
            if (Mathf.Abs(debugAcceleration.z) > 0.1f || Mathf.Abs(debugAcceleration.y) > 0.1f)
            {
                string priority = Mathf.Abs(debugAcceleration.z) >= zAxisPriorityThreshold ? "Z-priority" : "Y-priority";
                Debug.Log($"Z-Force Breakdown → Z-accel: {zFromAccel:F3}, Y-to-Z: {zFromY:F3}, Total Z: {shakeForce.z:F3} ({priority})");
            }
        }

        // Process gyroscope input
        if (hasGyroInput)
        {
            // Reduce gyro influence when linear movement is dominant to prevent conflicts
            float gyroStrength = gyroInfluenceStrength;

            // Scale down gyro influence if there's significant linear X movement
            if (Mathf.Abs(debugAcceleration.x) > 0.2f)
            {
                gyroStrength *= 0.3f; // Reduce gyro influence when linear movement is strong
            }

            // Scale down gyro Z influence if there's significant linear Z movement
            float gyroZStrength = gyroStrength;
            if (Mathf.Abs(debugAcceleration.z) > 0.15f)
            {
                gyroZStrength *= 0.2f; // Further reduce when Z movement is dominant
            }

            // Calculate gyro contributions
            float gyroX = debugGyroRate.y * gyroStrength;
            float gyroZ = -debugGyroRate.x * gyroZStrength;

            // Add rotational influence with conflict prevention
            shakeForce.x += gyroX;
            shakeForce.z += gyroZ;

            // Debug gyro Z contribution when significant
            if (Mathf.Abs(gyroZ) > 0.05f)
            {
                Debug.Log($"Gyro Z-Force: {gyroZ:F3} (from gyroRate.x: {debugGyroRate.x:F3}, strength: {gyroZStrength:F3})");
            }
        }

        // Ensure upward force if any input detected
        if (hasAccelInput || hasGyroInput)
        {
            shakeForce.y = Mathf.Max(upwardForceComponent, minimumUpwardForce);
        }
        else
        {
            // No significant input detected - return minimal random movement
            return new Vector3(
                Random.Range(-0.1f, 0.1f),
                minimumUpwardForce * 0.5f,
                Random.Range(-0.1f, 0.1f)
            ).normalized;
        }

        // Final safety check: ensure Y is never negative
        if (shakeForce.y < minimumUpwardForce)
        {
            shakeForce.y = minimumUpwardForce;
        }

        // Debug total Z-force when significant (helps identify overlap issues)
        if (Mathf.Abs(shakeForce.z) > 0.3f)
        {
            Debug.Log($"*** HIGH Z-FORCE DETECTED: {shakeForce.z:F3} (Total from all sources) ***");
        }

        return shakeForce.normalized;
    }
    float GetActivationThreshold()
    {
        // Cache the threshold calculation since sensitivity doesn't change often
        if (thresholdCacheDirty)
        {
            // Increased threshold values to require more vigorous shaking
            float minThreshold = 15.0f;  // Was 9.0f - much higher threshold for low sensitivity
            float maxThreshold = 3.0f;   // Was 1.0f - higher threshold for high sensitivity
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
        if (diceArray == null || diceArray.Length == 0) return;

        float activationThreshold = GetActivationThreshold();

        if (shakeIntensity < activationThreshold)
        {
            // Reset direction tracking when shaking stops
            if (singlePushPerDirection)
            {
                hasAppliedForceThisDirection = false;
            }
            return;
        }

        // Early exit if no valid dice (optimized check)
        bool hasValidDice = false;
        for (int i = 0; i < diceArray.Length; i++)
        {
            if (diceArray[i] != null)
            {
                hasValidDice = true;
                break;
            }
        }

        if (!hasValidDice) return;

        // Trigger vibration only if there are dice to affect
        // #if UNITY_ANDROID || UNITY_IOS
        //         Handheld.Vibrate();
        // #endif

        // Calculate shake direction based on device acceleration
        Vector3 shakeDirection = GetShakeDirection();
        currentShakeDirection = shakeDirection; // Store for debug visualization

        // Check if we should apply force based on single-push-per-direction setting
        bool shouldApplyForce = true;
        if (singlePushPerDirection)
        {
            // Calculate how much the direction has changed
            float directionChange = Vector3.Angle(lastShakeDirection, shakeDirection);
            float threshold = directionSensitivity * 180f;

            // Only apply force if direction changed significantly or we haven't applied force yet
            if (hasAppliedForceThisDirection && directionChange < threshold)
            {
                shouldApplyForce = false;
                // Debug direction blocking
                Debug.Log($"BLOCKING: Direction change {directionChange:F1}° < {threshold:F1}° threshold. Z-direction: {shakeDirection.z:F3}");
            }
            else if (directionChange >= threshold)
            {
                // Direction changed enough, reset the flag
                hasAppliedForceThisDirection = false;
                Debug.Log($"ALLOWING: Direction change {directionChange:F1}° >= {threshold:F1}° threshold. New Z: {shakeDirection.z:F3}");
            }
        }

        // If we shouldn't apply force, return early
        if (!shouldApplyForce) return;

        // Mark that we've applied force in this direction
        if (singlePushPerDirection)
        {
            hasAppliedForceThisDirection = true;
            lastShakeDirection = shakeDirection;
        }

        // Optimized force application with reduced allocations
        float baseIntensity = shakeIntensity * forceMultiplier;
        float halfRotationIntensity = rotationIntensity * 0.5f;

        for (int i = 0; i < diceArray.Length; i++)
        {
            var dice = diceArray[i];
            if (dice == null) continue;

            // Cache dice position once
            Vector3 dicePosition = dice.transform.position;

            // Optimized individual direction calculation
            Vector3 individualDirection = shakeDirection;

            // Minimal natural variation for better feel
            individualDirection.x += Random.Range(-0.1f, 0.1f);
            individualDirection.y += Random.Range(0.0f, 0.1f); // Only positive Y variation
            individualDirection.z += Random.Range(-0.1f, 0.1f);

            // Fast normalization (since we know Y is positive)
            individualDirection.Normalize();

            // Optimized force point calculation
            Vector3 forcePoint = new Vector3(
                dicePosition.x + Random.Range(-halfRotationIntensity, halfRotationIntensity),
                dicePosition.y + Random.Range(-halfRotationIntensity, halfRotationIntensity),
                dicePosition.z + Random.Range(-halfRotationIntensity, halfRotationIntensity)
            );

            // Apply force with slight intensity variation for natural feel
            float individualIntensity = baseIntensity * Random.Range(0.9f, 1.1f);
            Vector3 appliedForce = individualDirection * individualIntensity;

            dice.ApplyRollForce(appliedForce, forcePoint);
        }
    }

    public void RefreshDiceList()
    {
        diceArray = FindObjectsByType<DiceManager>(FindObjectsSortMode.None);
    }
}
