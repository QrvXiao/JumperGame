using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [Header("Visual Effects")]
    public float pulseSpeed = 2f;
    public float pulseScale = 1.3f;
    public Color inRangeColor = Color.cyan;
    public Color outOfRangeColor = Color.white;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool playerInRange = false;
    private Transform playerTransform;
    private float grapplingRange;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // Find player and get grappling range
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                grapplingRange = controller.grapplingRange;
            }
        }
    }
    
    void Update()
    {
        if (playerTransform == null) return;
        
        // Check if player is in range
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = distance <= grapplingRange;
        
        if (playerInRange)
        {
            // Pulse effect
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1f) * 0.5f;
            transform.localScale = originalScale * pulse;
            
            // Change color
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, inRangeColor, Time.deltaTime * 5f);
            }
        }
        else
        {
            // Return to normal
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 5f);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(spriteRenderer.color, outOfRangeColor, Time.deltaTime * 5f);
            }
        }
    }
}
