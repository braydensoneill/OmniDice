using UnityEngine;

public class IdleTextAnimator : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float pulseSpeed = 1.5f;
    public float pulseAmount = 0.05f;

    [Header("Wiggle Settings")]
    public float wiggleInterval = 4f;
    public float wiggleDuration = 0.3f;
    public float wiggleAmount = 5f;

    private Vector3 originalScale;
    private Vector3 originalRotation;
    private float wiggleTimer = 0f;
    private float wiggleTimeElapsed = 0f;
    private bool isWiggling = false;

    // Cache for performance
    private float nextWiggleTime;

    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localEulerAngles;
        nextWiggleTime = Time.time + wiggleInterval;
    }

    void Update()
    {
        AnimateBreathing();
        AnimateWiggle();
    }

    void AnimateBreathing()
    {
        float scaleOffset = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * scaleOffset;
    }

    void AnimateWiggle()
    {
        // Use cached time instead of accumulating deltaTime
        if (!isWiggling && Time.time >= nextWiggleTime)
        {
            isWiggling = true;
            wiggleTimeElapsed = 0f;
            nextWiggleTime = Time.time + wiggleInterval;
        }

        if (isWiggling)
        {
            wiggleTimeElapsed += Time.deltaTime;
            float wiggleProgress = Mathf.PingPong(wiggleTimeElapsed * 10f, 1f);
            float wiggleAngle = Mathf.Sin(wiggleProgress * Mathf.PI * 2f) * wiggleAmount;

            transform.localEulerAngles = originalRotation + new Vector3(0f, 0f, wiggleAngle);

            if (wiggleTimeElapsed >= wiggleDuration)
            {
                isWiggling = false;
                transform.localEulerAngles = originalRotation;
            }
        }
    }
}
