using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeThreshold = 2;
    [SerializeField] private float shakeForce = 500f;
    [SerializeField] private float torqueForce = 100;

    [Header("Debug")]
    public bool showDebugInfo = true;
    public float currentShakeIntensity;

    private DiceManager[] dice;
    private Vector3 lastAcceleration;

    void Start()
    {
        // Find all dice in the scene
        RefreshDiceList();
        lastAcceleration = Input.acceleration;
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

        if (shakeIntensity > shakeThreshold)
        {
            PushDiceToTarget(shakeIntensity);
        }
        // No need to stop anything - dice will naturally settle when no forces applied

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