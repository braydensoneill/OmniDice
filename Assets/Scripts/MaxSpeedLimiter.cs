using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MaxSpeedLimiter : MonoBehaviour
{
    [Tooltip("The maximum speed the object is allowed to reach.")]
    public float maxSpeed = 10f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}
