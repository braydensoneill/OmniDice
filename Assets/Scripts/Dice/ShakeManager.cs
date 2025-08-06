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
    private bool appliedForceThisFrame = false;
    private DiceManager[] diceArray;

    private Vector3 debugGyroRate;
    private Vector3 debugAcceleration;

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
        float minThreshold = 9.0f;
        float maxThreshold = 1.0f;
        return Mathf.Lerp(minThreshold, maxThreshold, (sensitivity - 1f) / 9f);
    }

    void RollDice()
    {
        appliedForceThisFrame = false;
        if (diceArray == null || rollToDirection == null) return;

        float activationThreshold = GetActivationThreshold();

        if (shakeIntensity < activationThreshold) return;

        appliedForceThisFrame = true;

        // Trigger vibration
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif

        foreach (var dice in diceArray)
        {
            if (dice == null) continue;

            Vector3 forceDir = (rollToDirection.position - dice.transform.position).normalized;

            Vector3 randomOffset = new Vector3(
                Random.Range(-rotationIntensity, rotationIntensity),
                Random.Range(-rotationIntensity, rotationIntensity),
                Random.Range(-rotationIntensity, rotationIntensity)
            );
            Vector3 forcePoint = dice.transform.position + randomOffset;

            dice.ApplyRollForce(forceDir * shakeIntensity * forceMultiplier, forcePoint);
        }
    }

    public void RefreshDiceList()
    {
        diceArray = FindObjectsOfType<DiceManager>();
    }
}
