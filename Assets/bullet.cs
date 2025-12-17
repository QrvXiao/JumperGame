using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float maxDistance = 20f;
    
    private Vector2 startPosition;
    private Rigidbody2D rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void OnEnable()
    {
        // Don't set startPosition here - it will be set after bullet is repositioned
        Debug.Log($"Bullet enabled at {transform.position}, active: {gameObject.activeSelf}");
    }
    
    public void Initialize(Vector2 position)
    {
        startPosition = position;
    }
    
    void Update()
    {
        // Check if bullet traveled max distance
        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxDistance)
        {
            Debug.Log($"Bullet exceeded max distance: {distanceTraveled}");
            ReturnToPool();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bullet hit: {other.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Player"))
        {
            // Destroy player
            GameManager.Instance.PlayerDied();
            ReturnToPool();
        }
        else if (other.CompareTag("Ground"))
        {
            // Hit wall, return to pool
            ReturnToPool();
        }
    }
    
    void ReturnToPool()
    {
        // Stop bullet movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Return to pool
        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.ReturnBullet(gameObject);
        }
        else
        {
            // Fallback if pool doesn't exist
            Destroy(gameObject);
        }
    }
}