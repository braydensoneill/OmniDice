using UnityEngine;

public class ShakeManager : MonoBehaviour
{
    [Header("External References")]
    public Rigidbody dice;               // Rigidbody of the dice
    public Transform rollToDirection;    // Target direction to apply force toward

    [Header("Settings")]
    public float forceMultiplier = 100f;      // Scale the applied force
    public float minShakeThreshold = 5f;      // Minimum intensity to trigger force

    public float rotationIntensity = 0.05f;   // How off-center the force is applied (affects rotation)

    public float moveSpeed = 2f;              // Speed at which rollToDirection moves around (unused but kept)

    [Header("Debug")]
    public float shakeIntensity;              // Current calculated shake intensity

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    // Timer for controlling how often rollToDirection moves
    private float moveTimer = 0f;
    private float moveInterval = 0.5f;  // seconds

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void FixedUpdate()
    {

        MoveRollDirection();

        // Calculate shake intensity
        FindShakeIntensity();

        // Roll the dice towards rollToDirection if shake intensity is above threshold
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
        // Calculate linear velocity
        Vector3 velocity = (transform.position - lastPosition) / Time.fixedDeltaTime;

        // Calculate angular velocity magnitude
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out _);
        float angularVelocity = angleInDegrees / Time.fixedDeltaTime;

        // Raw shake intensity from movement and rotation
        float rawIntensity = velocity.magnitude + angularVelocity * 0.01f;

        // Apply minimum threshold
        shakeIntensity = rawIntensity >= minShakeThreshold ? rawIntensity : 0f;
    }

    void RollDice()
    {
        if (shakeIntensity > 0f && dice != null && rollToDirection != null)
        {
            Vector3 forceDir = (rollToDirection.position - dice.position).normalized;

            // Apply force at a small offset to add rotation, scaled by rotationIntensity
            Vector3 randomOffset = new Vector3(
                Random.Range(-rotationIntensity, rotationIntensity),
                Random.Range(-rotationIntensity, rotationIntensity),
                Random.Range(-rotationIntensity, rotationIntensity)
            );
            Vector3 forcePoint = dice.position + randomOffset;

            dice.AddForceAtPosition(forceDir * shakeIntensity * forceMultiplier, forcePoint, ForceMode.Force);
        }

        // Store current position and rotation for next frame's calculation
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }
}
