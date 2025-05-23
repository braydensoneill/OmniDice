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
    [SerializeField][Range(1, 10)] private int sensitivity = 7;

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
    private float adjustedThreshold;
    private bool appliedForceThisFrame = false;

    private DiceManager[] diceArray;

    // Debug values
    private Vector3 debugGyroRate;
    private Vector3 debugAcceleration;

    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }

        GameObject diceFolder = GameObject.Find("Dice");
        if (diceFolder != null)
        {
            diceArray = diceFolder.GetComponentsInChildren<DiceManager>();
        }
        else
        {
            Debug.LogWarning("No 'Dice' folder found at the root of the hierarchy.");
            diceArray = new DiceManager[0];
        }
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

        adjustedThreshold = minShakeThreshold * (11 - sensitivity) / 10f;

        float clampedIntensity = Mathf.Min(rawIntensity, maxShakeIntensity);
        shakeIntensity = clampedIntensity >= adjustedThreshold ? clampedIntensity : 0f;
    }

    void RollDice()
    {
        appliedForceThisFrame = false;

        if (shakeIntensity <= 0f || diceArray == null || rollToDirection == null) return;

        appliedForceThisFrame = true;

        foreach (var dice in diceArray)
        {
            if (dice == null) continue;

            Rigidbody rb = dice.GetComponent<Rigidbody>();
            if (rb == null) continue;

            Vector3 forceDir = (rollToDirection.position - rb.position).normalized;

            Vector3 randomOffset = new Vector3(
                Random.Range(-rotationIntensity, rotationIntensity),
                Random.Range(-rotationIntensity, rotationIntensity),
                Random.Range(-rotationIntensity, rotationIntensity)
            );
            Vector3 forcePoint = rb.position + randomOffset;

            rb.AddForceAtPosition(forceDir * shakeIntensity * forceMultiplier, forcePoint, ForceMode.Force);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Shake Intensity: {shakeIntensity:F2}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Adjusted Threshold: {adjustedThreshold:F2}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Sensitivity: {sensitivity}");
        GUI.Label(new Rect(10, 70, 300, 20), $"Force Applied: {(appliedForceThisFrame ? "Yes" : "No")}");
        GUI.Label(new Rect(10, 100, 300, 20), $"Gyro: {debugGyroRate}");
        GUI.Label(new Rect(10, 120, 300, 20), $"Accel: {debugAcceleration}");
    }
}
