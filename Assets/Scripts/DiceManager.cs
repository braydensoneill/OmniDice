using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DiceManager : MonoBehaviour
{
    private Rigidbody rb;
    public bool isRolling { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
}
