using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("External References")]
    public Transform rollToDirection;

    [Header("Settings")]
    public float forceMultiplier = 100f;
    public float minShakeThreshold = 5f;
    public float rotationIntensity = 0.05f;
    [SerializeField][Range(1, 10)] private int sensitivity = 10;
    [SerializeField][Range(1f, 100f)] private float maxShakeIntensity = 20f;

    [Header("Debug")]
    public float shakeIntensity;

    private DiceManager[] diceArray;

    private Vector3 lastAcceleration;
    private Vector3 lastAngularVelocity;

    void Start()
    {
        Input.gyro.enabled = true;

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

        lastAcceleration = Input.acceleration;
        lastAngularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        FindShakeIntensity();
        RollDice();
    }

    void FindShakeIntensity()
    {
        // Device acceleration (linear movement)
        Vector3 currentAcceleration = Input.acceleration;
        Vector3 deltaAcceleration = currentAcceleration - lastAcceleration;
        float accelerationMagnitude = deltaAcceleration.magnitude / Time.fixedDeltaTime;

        // Device angular velocity (rotation speed)
        Vector3 angularVelocity = Input.gyro.rotationRateUnbiased;
        float angularMagnitude = angularVelocity.magnitude;

        // Raw shake intensity
        float rawIntensity = accelerationMagnitude + angularMagnitude;

        // Adjusted threshold
        float adjustedThreshold = minShakeThreshold * (11 - sensitivity) / 10f;

        // Clamp the shake intensity
        float clampedIntensity = Mathf.Min(rawIntensity, maxShakeIntensity);

        // Set shakeIntensity if above threshold
        shakeIntensity = clampedIntensity >= adjustedThreshold ? clampedIntensity : 0f;

        lastAcceleration = currentAcceleration;
        lastAngularVelocity = angularVelocity;
    }

    void RollDice()
    {
        if (shakeIntensity > 0f && diceArray != null && rollToDirection != null)
        {
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
    }
}
