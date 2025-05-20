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

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private float moveTimer = 0f;
    private float moveInterval = 0.5f;

    private DiceManager[] diceArray;

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;

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
        MoveRollDirection();
        FindShakeIntensity();
        RollDice();
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
        float adjustedThreshold = minShakeThreshold * (11 - sensitivity) / 10f;
        float clampedIntensity = Mathf.Min(rawIntensity, maxShakeIntensity);
        shakeIntensity = clampedIntensity >= adjustedThreshold ? clampedIntensity : 0f;
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

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }
}
