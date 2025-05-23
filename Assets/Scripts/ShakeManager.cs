using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("External References")]
    public Transform rollToDirection;

    [Header("Settings")]
    public float forceMultiplier = 100f;
    public float minShakeThreshold = 1.2f; // Not used now, replaced by GetActivationThreshold
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

        // Clamp intensity so it doesn't get unrealistically high
        shakeIntensity = Mathf.Min(rawIntensity, maxShakeIntensity);
    }

    float GetActivationThreshold()
    {
        // Sensitivity slider maps 1 (least sensitive) to 10 (most sensitive)
        // Adjust these values to shift the entire slider sensitivity
        float minThreshold = 9.0f;  // minimum shake required at lowest sensitivity (hardest to trigger)
        float maxThreshold = 1.0f;  // minimum shake required at highest sensitivity (easiest to trigger)
        return Mathf.Lerp(minThreshold, maxThreshold, (sensitivity - 1f) / 9f);
    }

    void RollDice()
    {
        appliedForceThisFrame = false;
        if (diceArray == null || rollToDirection == null) return;

        float activationThreshold = GetActivationThreshold();

        // Only roll dice if shakeIntensity exceeds activation threshold
        if (shakeIntensity < activationThreshold) return;

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

            // Use shakeIntensity directly to scale rolling force (controls dice roll speed)
            rb.AddForceAtPosition(forceDir * shakeIntensity * forceMultiplier, forcePoint, ForceMode.Force);
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 24;  // Set font size to 24 (2x default)

        GUI.Label(new Rect(10, 10, 400, 30), $"Shake Intensity: {shakeIntensity:F2}");
        GUI.Label(new Rect(10, 40, 400, 30), $"Activation Threshold: {GetActivationThreshold():F2}");
        GUI.Label(new Rect(10, 70, 400, 30), $"Sensitivity: {sensitivity}");
        GUI.Label(new Rect(10, 100, 400, 30), $"Force Applied: {(appliedForceThisFrame ? "Yes" : "No")}");
        GUI.Label(new Rect(10, 140, 400, 30), $"Gyro: {debugGyroRate}");
        GUI.Label(new Rect(10, 170, 400, 30), $"Accel: {debugAcceleration}");
    }
}
