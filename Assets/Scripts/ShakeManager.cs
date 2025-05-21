using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("External References")]
    public Transform rollToDirection;

    [Header("Settings")]
    public float forceMultiplier = 100f;
    public float minShakeThreshold = 5f;
    public float rotationIntensity = 0.05f;
    public float moveSpeed = 2f;
    [SerializeField][Range(1, 10)] private int sensitivity = 10;

    [Header("Shake Limits")]
    [SerializeField][Range(1f, 100f)] private float maxShakeIntensity = 20f;

    [Header("Debug")]
    public float shakeIntensity;

    [Header("Phone Input")]
    [SerializeField] private bool useDeviceMotion = true;
    [SerializeField] private float gyroRotationMultiplier = 2f;
    [SerializeField] private float accelMoveMultiplier = 0.1f;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private DiceManager[] diceArray;

    // Debug tracking
    private float adjustedThreshold;
    private bool appliedForceThisFrame = false;

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;

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
            ApplyPhoneMotionToTransform();
        }

        MoveRollDirection();
        FindShakeIntensity();
        RollDice();
    }

    void ApplyPhoneMotionToTransform()
    {
        if (!SystemInfo.supportsGyroscope) return;

        Vector3 rotationRate = Input.gyro.rotationRateUnbiased * gyroRotationMultiplier;
        transform.Rotate(rotationRate, Space.Self);

        Vector3 acceleration = Input.acceleration;
        transform.position += acceleration * accelMoveMultiplier;
    }

    void MoveRollDirection()
    {
        if (rollToDirection == null) return;

        Vector3 currentPos = rollToDirection.position;
        rollToDirection.position = new Vector3(currentPos.x, currentPos.y, rollToDirection.position.z);
    }

    void FindShakeIntensity()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;

        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out _);
        float angularVelocity = angleInDegrees / Time.fixedDeltaTime;

        float rawIntensity = velocity.magnitude + angularVelocity * 0.01f;

        float clampedIntensity = Mathf.Min(rawIntensity, maxShakeIntensity);
        shakeIntensity = clampedIntensity >= minShakeThreshold ? clampedIntensity : 0f;

        lastPosition = transform.position;
        lastRotation = transform.rotation;
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
    }
}
