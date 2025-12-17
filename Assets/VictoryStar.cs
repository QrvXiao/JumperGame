using UnityEngine;

public class VictoryStar : MonoBehaviour
{
    [Header("Visual Effects")]
    public float rotationSpeed = 90f;
    public float pulseSpeed = 3f;
    public float pulseScale = 1.2f;
    
    private Vector3 originalScale;
    
    void Start()
    {
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        // Rotate star
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        
        // Pulse scale
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1f) * 0.5f;
        transform.localScale = originalScale * pulse;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerWon();
                gameObject.SetActive(false);
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
