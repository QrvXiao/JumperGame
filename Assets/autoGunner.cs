using UnityEngine;

public class AutoGunner : MonoBehaviour
{
    [Header("Shooting")]
    public Transform shootPoint;
    public float shootInterval = 2f;
    public float bulletSpeed = 8f;
    public Vector2 shootDirection = Vector2.right;
    
    private float shootTimer;
    
    void Update()
    {
        // Don't update timer when game is paused
        if (Time.timeScale == 0f) return;
        
        shootTimer += Time.deltaTime;
        
        if (shootTimer >= shootInterval)
        {
            Shoot();
            shootTimer = 0f;
        }
    }
    
    public void ResetTimer()
    {
        shootTimer = 0f;
    }
    
    void Shoot()
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool not found! Make sure BulletPool exists in the scene.");
            return;
        }
        
        // Get bullet from pool
        GameObject bullet = BulletPool.Instance.GetBullet();
        
        if (bullet == null)
        {
            Debug.LogWarning("BulletPool returned null bullet!");
            return;
        }
        
        bullet.transform.position = shootPoint.position;
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
        
        // Initialize bullet start position AFTER moving it
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(shootPoint.position);
        }
        
        // Set bullet velocity
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.gravityScale = 0f;
            bulletRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            bulletRb.linearVelocity = shootDirection.normalized * bulletSpeed;
        }
        else
        {
            Debug.LogWarning("Bullet has no Rigidbody2D!");
        }
        
        Debug.Log($"Gunner shot bullet at {shootPoint.position}, velocity: {shootDirection.normalized * bulletSpeed}");
    }
    
    void OnDrawGizmosSelected()
    {
        if (shootPoint != null)
        {
            // Draw shoot direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(shootPoint.position, shootDirection.normalized * 2f);
            Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
        }
    }
}