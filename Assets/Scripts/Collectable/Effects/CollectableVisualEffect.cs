using UnityEngine;

public class CollectableVisualEffect : MonoBehaviour
{
    [Header("Rotation")]
    public bool enableRotation = true;
    public Vector3 rotationAxis = Vector3.forward;
    public float rotationSpeed = 100f;
    
    [Header("Bobbing")]
    public bool enableBobbing = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    public Vector3 bobAxis = Vector3.up;
    
    [Header("Pulse")]
    public bool enablePulse = false;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;
    
    [Header("Glow")]
    public bool enableGlow = false;
    public SpriteRenderer glowRenderer;
    public float glowSpeed = 3f;
    public float glowMinAlpha = 0.3f;
    public float glowMaxAlpha = 1f;
    
    private Vector3 startPosition;
    private Vector3 originalScale;
    private Color originalGlowColor;

    private void Start()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;
        
        if (glowRenderer != null)
        {
            originalGlowColor = glowRenderer.color;
        }
    }

    private void Update()
    {
        if (enableRotation)
        {
            ApplyRotation();
        }

        if (enableBobbing)
        {
            ApplyBobbing();
        }

        if (enablePulse)
        {
            ApplyPulse();
        }

        if (enableGlow && glowRenderer != null)
        {
            ApplyGlow();
        }
    }

    private void ApplyRotation()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }

    private void ApplyBobbing()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        Vector3 newPosition = startPosition + (bobAxis.normalized * offset);
        transform.position = newPosition;
    }

    private void ApplyPulse()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }

    private void ApplyGlow()
    {
        float alpha = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, 
            (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f);
        
        Color newColor = originalGlowColor;
        newColor.a = alpha;
        glowRenderer.color = newColor;
    }

    public void StopAllEffects()
    {
        enableRotation = false;
        enableBobbing = false;
        enablePulse = false;
        enableGlow = false;
        
        transform.position = startPosition;
        transform.localScale = originalScale;
    }
}
