using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DiceManager : MonoBehaviour
{
    private Rigidbody rb;
    public bool isRolling { get; private set; }

    private AudioSource audioSource;
    public AudioClip collisionSound;

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

    private void Update()
    {
        if (isRolling && rb.linearVelocity.sqrMagnitude < 0.0025f && rb.angularVelocity.sqrMagnitude < 0.0025f)
        {
            isRolling = false;
            // Optional: Trigger result calculation or animation here
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Play a quick button click sound on first collision
        if (collisionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collisionSound);
        }
    }
}
