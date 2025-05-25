using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("External References")]
    public Transform rollToDirection;
    public GameObject UI_Shake; // 👈 Assign this in the Inspector

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

    // Timer to show UI_Shake
    private float stillTimer = 0f;
    private float stillThreshold = 3f;

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

        if (UI_Shake != null)
        {
            UI_Shake.SetActive(false); // Start disabled
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
        HandleUIShakeDisplay();
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

    void HandleUIShakeDisplay()
    {
        if (diceArray == null || UI_Shake == null) return;

        bool anyRolling = false;
        foreach (var dice in diceArray)
        {
            if (dice != null && dice.isRolling)
            {
                anyRolling = true;
                break;
            }
        }

        if (anyRolling)
        {
            stillTimer = 0f;
            UI_Shake.SetActive(false);
        }
        else
        {
            stillTimer += Time.deltaTime;
            if (stillTimer >= stillThreshold)
            {
                UI_Shake.SetActive(true);
            }
        }
    }
}
