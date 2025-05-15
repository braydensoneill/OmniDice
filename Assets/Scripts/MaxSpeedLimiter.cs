using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MaxSpeedLimiter : MonoBehaviour
{
    [Tooltip("The maximum speed the object is allowed to reach.")]
    public float maxSpeed = 10f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Clamp the velocity if it exceeds the max speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
