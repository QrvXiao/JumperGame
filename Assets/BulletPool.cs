using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;
    
    [Header("Pool Settings")]
    public GameObject bulletPrefab;
    public int poolSize = 20;
    
    private Queue<GameObject> bulletPool;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize pool
        bulletPool = new Queue<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bullet.transform.SetParent(transform);
            bulletPool.Enqueue(bullet);
        }
    }
    
    public GameObject GetBullet()
    {
        GameObject bullet = null;
        
        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            // Pool exhausted, create new bullet
            Debug.LogWarning($"BulletPool exhausted! Creating new bullet. Consider increasing pool size (current: {poolSize})");
            bullet = Instantiate(bulletPrefab);
            bullet.transform.SetParent(transform);
        }
        
        // Ensure bullet is properly initialized
        if (bullet != null)
        {
            bullet.SetActive(true);
            
            // Reset bullet state
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.gravityScale = 0f;
            }
        }
        
        return bullet;
    }
    
    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null) return;
        
        // Reset bullet physics
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        bulletPool.Enqueue(bullet);
    }
}
