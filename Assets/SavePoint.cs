using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [Header("Visual Feedback")]
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.cyan;
    public float pulseSpeed = 2f;
    
    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    private Vector3 originalScale;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = inactiveColor;
        }
    }
    
    void Update()
    {
        if (isActivated && spriteRenderer != null)
        {
            // Pulse effect when activated
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.15f;
            transform.localScale = originalScale * pulse;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateSavePoint();
        }
    }
    
    void ActivateSavePoint()
    {
        isActivated = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSavePoint(transform.position);
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = activeColor;
        }
        
        Debug.Log($"Save point activated at {transform.position}");
    }
    
    public void Reset()
    {
        isActivated = false;
        transform.localScale = originalScale;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = inactiveColor;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActivated ? activeColor : inactiveColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
