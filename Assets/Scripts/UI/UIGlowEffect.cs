using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class GlowSettings
{
    [Header("Glow Effect Settings")]
    public Color glowColor = new Color(1f, 1f, 0f, 0.8f); // Yellow glow
    public float glowIntensity = 1.5f; // How bright the glow is
    public float glowSize = 10f; // How far the glow extends
    public float pulseSpeed = 2f; // Speed of the pulsing animation
    public bool enablePulse = true; // Whether the glow should pulse
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    public AnimationCurve glowCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}

/// <summary>
/// Adds a glow effect to UI buttons for selection indication
/// Can be used for both dice selection and skin selection
/// </summary>
public class UIGlowEffect : MonoBehaviour
{
    [Header("Glow Configuration")]
    public GlowSettings glowSettings = new GlowSettings();
    
    [Header("References")]
    public Image targetImage; // The button image to glow
    public GameObject glowObject; // Optional: existing glow GameObject
    
    private Image glowImage;
    private CanvasGroup glowCanvasGroup;
    private bool isGlowing = false;
    private Coroutine glowCoroutine;
    private Coroutine pulseCoroutine;
    
    void Awake()
    {
        // Auto-find target image if not assigned
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
            if (targetImage == null)
            {
                targetImage = GetComponentInChildren<Image>();
            }
        }
        
        CreateGlowEffect();
    }
    
    void CreateGlowEffect()
    {
        if (targetImage == null)
        {
            Debug.LogWarning($"[UIGlowEffect] No target image found on {gameObject.name}");
            return;
        }
        
        // Create glow object if not provided
        if (glowObject == null)
        {
            glowObject = new GameObject("GlowEffect");
            glowObject.transform.SetParent(transform, false);
        }
        
        // Setup glow image
        glowImage = glowObject.GetComponent<Image>();
        if (glowImage == null)
        {
            glowImage = glowObject.AddComponent<Image>();
        }
        
        // Setup canvas group for fading
        glowCanvasGroup = glowObject.GetComponent<CanvasGroup>();
        if (glowCanvasGroup == null)
        {
            glowCanvasGroup = glowObject.AddComponent<CanvasGroup>();
        }
        
        // Configure glow image
        glowImage.sprite = targetImage.sprite;
        glowImage.color = glowSettings.glowColor;
        glowImage.raycastTarget = false; // Don't interfere with button clicks
        
        // Position glow behind the target image
        RectTransform glowRect = glowObject.GetComponent<RectTransform>();
        RectTransform targetRect = targetImage.GetComponent<RectTransform>();
        
        // Match the target's rect transform
        glowRect.anchorMin = targetRect.anchorMin;
        glowRect.anchorMax = targetRect.anchorMax;
        glowRect.anchoredPosition = targetRect.anchoredPosition;
        glowRect.sizeDelta = targetRect.sizeDelta + Vector2.one * glowSettings.glowSize;
        
        // Move glow behind target
        glowObject.transform.SetSiblingIndex(0);
        
        // Start with glow disabled
        glowCanvasGroup.alpha = 0f;
        glowObject.SetActive(false);
    }
    
    /// <summary>
    /// Enable the glow effect
    /// </summary>
    public void ShowGlow()
    {
        if (isGlowing) return;
        
        isGlowing = true;
        glowObject.SetActive(true);
        
        // Stop any existing coroutines
        if (glowCoroutine != null) StopCoroutine(glowCoroutine);
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        
        // Start fade in
        glowCoroutine = StartCoroutine(FadeGlow(0f, 1f, glowSettings.fadeInDuration));
        
        // Start pulse if enabled
        if (glowSettings.enablePulse)
        {
            pulseCoroutine = StartCoroutine(PulseGlow());
        }
    }
    
    /// <summary>
    /// Disable the glow effect
    /// </summary>
    public void HideGlow()
    {
        if (!isGlowing) return;
        
        isGlowing = false;
        
        // Stop pulse
        if (pulseCoroutine != null) 
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // Stop any existing fade
        if (glowCoroutine != null) StopCoroutine(glowCoroutine);
        
        // Start fade out
        glowCoroutine = StartCoroutine(FadeGlow(glowCanvasGroup.alpha, 0f, glowSettings.fadeOutDuration, () => {
            glowObject.SetActive(false);
        }));
    }
    
    /// <summary>
    /// Toggle the glow effect
    /// </summary>
    public void ToggleGlow()
    {
        if (isGlowing)
            HideGlow();
        else
            ShowGlow();
    }
    
    private IEnumerator FadeGlow(float fromAlpha, float toAlpha, float duration, System.Action onComplete = null)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = glowSettings.glowCurve.Evaluate(t);
            
            glowCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, curveValue);
            yield return null;
        }
        
        glowCanvasGroup.alpha = toAlpha;
        onComplete?.Invoke();
    }
    
    private IEnumerator PulseGlow()
    {
        while (isGlowing)
        {
            // Pulse the intensity
            float time = Time.time * glowSettings.pulseSpeed;
            float pulse = (Mathf.Sin(time) + 1f) * 0.5f; // 0 to 1
            
            // Apply pulse to color intensity
            Color currentColor = glowSettings.glowColor;
            currentColor.a = glowSettings.glowColor.a * (0.6f + 0.4f * pulse); // Pulse between 60% and 100%
            
            if (glowImage != null)
            {
                glowImage.color = currentColor;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Update glow settings at runtime
    /// </summary>
    public void UpdateGlowSettings(GlowSettings newSettings)
    {
        glowSettings = newSettings;
        
        if (glowImage != null)
        {
            glowImage.color = glowSettings.glowColor;
            
            // Update size
            RectTransform glowRect = glowObject.GetComponent<RectTransform>();
            RectTransform targetRect = targetImage.GetComponent<RectTransform>();
            glowRect.sizeDelta = targetRect.sizeDelta + Vector2.one * glowSettings.glowSize;
        }
    }
    
    /// <summary>
    /// Set custom glow color
    /// </summary>
    public void SetGlowColor(Color color)
    {
        glowSettings.glowColor = color;
        if (glowImage != null)
        {
            glowImage.color = color;
        }
    }
    
    void OnDestroy()
    {
        if (glowCoroutine != null) StopCoroutine(glowCoroutine);
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
    }
    
    // Editor helper methods
    #if UNITY_EDITOR
    [ContextMenu("Test Show Glow")]
    void TestShowGlow()
    {
        if (Application.isPlaying)
            ShowGlow();
    }
    
    [ContextMenu("Test Hide Glow")]
    void TestHideGlow()
    {
        if (Application.isPlaying)
            HideGlow();
    }
    #endif
}
