using UnityEngine;

public class WiggleUI : MonoBehaviour
{
    public float wiggleSpeed = 5f;
    public float wiggleAmount = 10f;

    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
        transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, angle);
    }
}