using UnityEngine;

public class FloatingTarget : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float swaySpeed = 0.8f;
    [SerializeField] private float swayAmplitude = 0.3f;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        // Gentle up/down floating motion
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // Gentle side-to-side sway
        float xOffset = Mathf.Sin(Time.time * swaySpeed) * swayAmplitude;

        // Apply the floating motion
        transform.position = originalPosition + new Vector3(xOffset, yOffset, 0f);
    }
}