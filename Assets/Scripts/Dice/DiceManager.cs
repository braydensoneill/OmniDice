using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DiceManager : MonoBehaviour
{
    private Rigidbody rb;
    public bool isRolling { get; private set; }

    private AudioSource audioSource;
    public AudioClip collisionSound;

    private const float VELOCITY_THRESHOLD_SQR = 0.0025f;
    private const float ANGULAR_VELOCITY_THRESHOLD_SQR = 0.0025f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    public void ApplyRollForce(Vector3 force, Vector3 forcePoint)
    {
        rb.AddForceAtPosition(force, forcePoint, ForceMode.Force);
        isRolling = true;
    }

    private void FixedUpdate()
    {
        CheckRollingState();
    }

    private void CheckRollingState()
    {
        if (isRolling && rb.linearVelocity.sqrMagnitude < VELOCITY_THRESHOLD_SQR &&
            rb.angularVelocity.sqrMagnitude < ANGULAR_VELOCITY_THRESHOLD_SQR)
        {
            isRolling = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(collisionSound);
        }
    }
}
