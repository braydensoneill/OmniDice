using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DiceManager : MonoBehaviour
{
    private Rigidbody rb;
    public bool isRolling;

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
        if (isRolling && rb.linearVelocity.magnitude < 0.05f && rb.angularVelocity.magnitude < 0.05f)
        {
            isRolling = false;
            // Optional: trigger landing animation or calculate result here
        }
    }
}
