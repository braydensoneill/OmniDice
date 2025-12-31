using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeThreshold = 2;
    [SerializeField] private float rotationThreshold = 8; // Higher threshold for rotation
    [SerializeField] private float shakeForce = 200f;
    [SerializeField] private float torqueForce = 50f;
    [SerializeField] private float maxIntensityMultiplier = 2f; // Cap the intensity multiplier

    [Header("Debug")]
    public bool showDebugInfo = true;
    public float currentShakeIntensity;
    public float currentRotationIntensity;

    private DiceManager[] dice;
    private Vector3 lastAcceleration;
    private Vector3 lastGyroInput;

    void Start()
    {
        // Find all dice in the scene
        RefreshDiceList();
        lastAcceleration = Input.acceleration;

        // Enable gyroscope for rotation detection
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            lastGyroInput = Input.gyro.rotationRateUnbiased;
            if (showDebugInfo)
                Debug.Log("Gyroscope enabled for rotation detection");
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("Gyroscope not supported on this device");
        }
    }

    void Update()
    {
        DetectShake();
    }

    void DetectShake()
    {
        Vector3 currentAccel = Input.acceleration;
        float shakeIntensity = (currentAccel - lastAcceleration).magnitude;
        currentShakeIntensity = shakeIntensity; // Store for debugging

        // Check for linear shake
        bool isShaking = shakeIntensity > shakeThreshold;

        // Check for rotation if gyroscope is available
        bool isRotating = false;
        float rotationIntensity = 0f;

        if (SystemInfo.supportsGyroscope && Input.gyro.enabled)
        {
            Vector3 currentGyro = Input.gyro.rotationRateUnbiased;
            rotationIntensity = (currentGyro - lastGyroInput).magnitude;
            currentRotationIntensity = rotationIntensity;

            isRotating = rotationIntensity > rotationThreshold;
            lastGyroInput = currentGyro;
        }

        // Trigger dice movement if either shaking or rotating
        if (isShaking || isRotating)
        {
            // Use the higher intensity for force calculation, but clamp it
            float combinedIntensity = Mathf.Max(shakeIntensity, rotationIntensity * 0.3f); // Reduce rotation impact
            combinedIntensity = Mathf.Clamp(combinedIntensity, 0f, maxIntensityMultiplier); // Cap the multiplier
            PushDiceToTarget(combinedIntensity);

            if (showDebugInfo && isRotating)
            {
                Debug.Log($"Phone rotation detected! Intensity: {rotationIntensity:F2} (reduced to {rotationIntensity * 0.3f:F2})");
            }
        }
        lastAcceleration = currentAccel;
    }

    void PushDiceToTarget(float intensity)
    {
        GameObject rollToTarget = GameObject.FindGameObjectWithTag("RollToTarget");
        if (rollToTarget == null || dice == null) return;

        // Scale force based on shake intensity
        float dynamicForce = shakeForce * intensity;
        float dynamicTorque = torqueForce * intensity;

        foreach (DiceManager die in dice)
        {
            if (die == null) continue;

            Vector3 direction = (rollToTarget.transform.position - die.transform.position).normalized;
            Vector3 force = direction * dynamicForce;

            // Add random torque for rotation
            Vector3 randomTorque = Random.insideUnitSphere * dynamicTorque;

            die.ApplyRollForce(force, die.transform.position);

            // Apply torque directly to rigidbody for spinning
            Rigidbody rb = die.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddTorque(randomTorque);
            }
        }
    }

    public void RefreshDiceList()
    {
        dice = FindObjectsByType<DiceManager>(FindObjectsSortMode.None);
        if (showDebugInfo)
        {
            Debug.Log($"Found {dice.Length} dice in scene");
        }
    }

    // Adjust sensitivity at runtime
    public void SetSensitivity(float newThreshold)
    {
        shakeThreshold = Mathf.Clamp(newThreshold, 0.5f, 5.0f);
    }
}