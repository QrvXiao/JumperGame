using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector2 offset = Vector2.zero;
    
    [Header("Look Ahead")]
    public float lookAheadDistance = 2f;
    public float lookAheadSpeed = 3f;
    
    [Header("Framing")]
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public bool useBounds = false;
    
    [Header("Grappling")]
    public float grappleZoomOut = 2f;
    public float zoomSpeed = 3f;
    
    private Vector3 velocity = Vector3.zero;
    private Vector2 lookAheadOffset;
    private float baseOrthographicSize;
    private Camera cam;
    private PlayerController playerController;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        baseOrthographicSize = cam.orthographicSize;
        
        if (target != null)
        {
            playerController = target.GetComponent<PlayerController>();
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Get player velocity for look-ahead
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        Vector2 targetVelocity = targetRb != null ? targetRb.linearVelocity : Vector2.zero;
        
        // Calculate look-ahead offset based on velocity
        Vector2 targetLookAhead = targetVelocity.normalized * lookAheadDistance;
        lookAheadOffset = Vector2.Lerp(lookAheadOffset, targetLookAhead, lookAheadSpeed * Time.deltaTime);
        
        // Calculate target position with offset and look-ahead
        Vector3 targetPosition = target.position + (Vector3)offset + (Vector3)lookAheadOffset;
        targetPosition.z = transform.position.z;
        
        // Apply bounds if enabled
        if (useBounds)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;
            
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);
        }
        
        // Smooth follow
        Vector3 smoothPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / smoothSpeed);
        transform.position = smoothPosition;
        
        // Zoom out slightly when grappling
        float targetSize = baseOrthographicSize;
        if (playerController != null && IsPlayerGrappling())
        {
            targetSize = baseOrthographicSize + grappleZoomOut;
        }
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }
    
    bool IsPlayerGrappling()
    {
        // Use reflection to check private isGrappling field
        var field = typeof(PlayerController).GetField("isGrappling", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(playerController);
        }
        return false;
    }
    
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
        
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position + (Vector3)offset, 0.3f);
        }
    }
}
