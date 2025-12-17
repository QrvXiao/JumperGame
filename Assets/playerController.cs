using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask = 1;

    [Header("Grappling")]
    public float grapplingRange = 8f;
    public float tensionBuildup = 2f;
    public float maxTension = 5f;
    public float launchMultiplier = 15f;
    // public float maxAxialForce = 3f;
    // public float axialForceBuildup = 2f;
    public float maxOrbitalSpeed = 5f;

    [Header("Arrow Indicator")]
    public Material arrowMaterial;
    public float arrowMaxLength = 4f;
    
    private Rigidbody2D rb;
    private LineRenderer arrowLineRenderer;
    private bool isGrounded;
    private bool isGrappling;
    private Camera playerCamera;
    private Vector2 grappleAnchor;
    private float currentTension = 0f;
    private Vector2 currentLaunchDirection; // Store the current launch direction
    private float originalGravityScale;
    private float grappleDistance;
    // private float currentAxialForce = 0f;

    // Input System
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool grapplePressed;
    private bool grappleReleased;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCamera = Camera.main;
        originalGravityScale = rb.gravityScale;

        // Setup arrow LineRenderer
        GameObject arrowObj = new GameObject("GrappleArrow");
        arrowObj.transform.SetParent(transform);
        arrowLineRenderer = arrowObj.AddComponent<LineRenderer>();
        arrowLineRenderer.startWidth = 0.1f;
        arrowLineRenderer.endWidth = 0.1f;
        arrowLineRenderer.positionCount = 5; // shaft (2 points) + arrow head (3 points)
        arrowLineRenderer.useWorldSpace = true;
        arrowLineRenderer.enabled = false;
        
        // Use default material if none assigned
        if (arrowMaterial != null)
        {
            arrowLineRenderer.material = arrowMaterial;
        }

        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
        inputActions.Player.Grapple.performed += ctx => grapplePressed = true;
        inputActions.Player.Grapple.canceled += ctx => grappleReleased = true;
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        CheckGrounded();
        HandleGrappling();   
        HandleInput();
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
    }

    void HandleInput()
    {
        // Horizontal movement (only when not grappling AND grounded)
        if (!isGrappling && isGrounded)
        {

            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }

        // Jump
        if (jumpPressed && (isGrounded || isGrappling))
        {
            if (isGrappling)
            {
                StopGrappling();
                // Add upward boost when jumping from grapple
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y );
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
        jumpPressed = false;

        // Grappling
        if (grapplePressed && !isGrappling)
        {
            StartGrappling();
        }
        grapplePressed = false;

        if (grappleReleased && isGrappling)
        {       
            StopGrappling();
        }
        grappleReleased = false;
    }

    void HandleGrappling()
    {
        if (isGrappling)
        {
            // Disable gravity while grappling
            rb.gravityScale = 0f;
            
            // Build tension over time
            currentTension = Mathf.Min(currentTension + tensionBuildup * Time.deltaTime, maxTension);
            
            
            // Calculate current distance and direction from grapple point
            Vector2 toPlayer = (Vector2)transform.position - grappleAnchor;
            float currentDistance = toPlayer.magnitude;
            Vector2 directionFromGrapple = toPlayer.normalized;
            
            // Calculate target position based on mouse for orbital movement
            if (Mouse.current != null)
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                mousePos = playerCamera.ScreenToWorldPoint(mousePos);
                mousePos.z = 0;
                
                Vector2 mouseDirection = ((Vector2)mousePos - grappleAnchor).normalized;
                
                // Calculate angular difference and limit orbital speed
                Vector2 currentDir = directionFromGrapple;
                float angleDiff = Vector2.SignedAngle(currentDir, mouseDirection);
                float maxAngleChange = maxOrbitalSpeed * Time.deltaTime * Mathf.Rad2Deg;
                
                // Limit the angular velocity
                if (Mathf.Abs(angleDiff) > maxAngleChange)
                {
                    angleDiff = Mathf.Sign(angleDiff) * maxAngleChange;
                }
                
                // Calculate new direction with limited angular velocity
                float currentAngle = Mathf.Atan2(currentDir.y, currentDir.x);
                float newAngle = currentAngle + angleDiff * Mathf.Deg2Rad;
                directionFromGrapple = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle));
            }
            
            // Calculate elastic distance (distance + axial force)
            float targetDistance = grappleDistance;
            
            // Position player at calculated distance and direction
            Vector2 targetPosition = grappleAnchor + directionFromGrapple * targetDistance;
            transform.position = Vector2.Lerp(transform.position, targetPosition, 15f * Time.deltaTime);
            
            // Store launch direction (away from grapple point)
            currentLaunchDirection = directionFromGrapple;
            
            // Update arrow indicator
            UpdateArrowIndicator();
            
            // Keep velocity zero to prevent physics interference
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Restore gravity when not grappling
            rb.gravityScale = originalGravityScale;
            
            // Hide arrow when not grappling
            if (arrowLineRenderer != null)
            {
                arrowLineRenderer.enabled = false;
            }
        }
    }
    
    void UpdateArrowIndicator()
    {
        if (arrowLineRenderer == null) return;
        
        // Calculate arrow length based on tension
        float arrowLength = (currentTension / maxTension) * arrowMaxLength;
        
        // Only show arrow if there's some tension
        if (arrowLength < 0.2f)
        {
            arrowLineRenderer.enabled = false;
            return;
        }
        
        arrowLineRenderer.enabled = true;
        
        // Calculate arrow positions
        Vector2 arrowStart = grappleAnchor;
        Vector2 arrowEnd = grappleAnchor - currentLaunchDirection * arrowLength;
        
        // Arrow head dimensions
        float headLength = 0.3f;
        float headWidth = 0.2f;
        Vector2 headBase = arrowEnd + currentLaunchDirection * headLength;
        Vector2 perpendicular = new Vector2(-currentLaunchDirection.y, currentLaunchDirection.x);
        Vector2 headLeft = headBase + perpendicular * headWidth;
        Vector2 headRight = headBase - perpendicular * headWidth;
        
        // Set positions: shaft start -> shaft end -> head left -> head tip -> head right
        arrowLineRenderer.positionCount = 5;
        arrowLineRenderer.SetPosition(0, new Vector3(arrowStart.x, arrowStart.y, 0));
        arrowLineRenderer.SetPosition(1, new Vector3(headBase.x, headBase.y, 0));
        arrowLineRenderer.SetPosition(2, new Vector3(headLeft.x, headLeft.y, 0));
        arrowLineRenderer.SetPosition(3, new Vector3(arrowEnd.x, arrowEnd.y, 0));
        arrowLineRenderer.SetPosition(4, new Vector3(headRight.x, headRight.y, 0));
        
        // Set color based on tension (white to red)
        Color arrowColor = Color.Lerp(Color.white, Color.red, currentTension / maxTension);
        arrowLineRenderer.startColor = arrowColor;
        arrowLineRenderer.endColor = arrowColor;
    }

    void StartGrappling()
    {
        // Find nearest grapple point within range
        GameObject[] grapplePoints = GameObject.FindGameObjectsWithTag("GrapplePoint");
        GameObject nearestPoint = null;
        float nearestDistance = float.MaxValue;
        
        foreach (GameObject point in grapplePoints)
        {
            float distance = Vector2.Distance(transform.position, point.transform.position);
            if (distance < grapplingRange && distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = point;
            }
        }
        
        if (nearestPoint != null)
        {
            isGrappling = true;
            currentTension = 0f;
            grappleAnchor = nearestPoint.transform.position;
            grappleDistance = grapplingRange; // Fix distance at grappling range
            
            // Initialize launch direction as the direction from grapple to player
            Vector2 initialDirection = ((Vector2)transform.position - grappleAnchor).normalized;
            currentLaunchDirection = initialDirection;
            
            Debug.Log($"Grappling to nearest point: {nearestPoint.name}, Fixed distance: {grappleDistance:F1}");
        }
        else
        {
            Debug.Log($"No grapple point found within range ({grapplingRange} units)");
        }
    }

    void StopGrappling()
    {
        if (isGrappling)
        {
            // Restore gravity
            rb.gravityScale = originalGravityScale;
            
            // Calculate elastic force based on distance from original grapple distance
            float currentDistance = Vector2.Distance(transform.position, grappleAnchor);
            float distanceStretch = currentDistance - grappleDistance; // How much we're stretched
            float elasticForce = Mathf.Max(distanceStretch, 0f) * launchMultiplier; // Only positive stretch contributes
            
            // Add tension force
            float totalForce = elasticForce + (currentTension * launchMultiplier * 0.5f);
            
            // Launch with the stored direction and calculated force
            Vector2 launchVelocity = currentLaunchDirection * totalForce;
            
            // Set the velocity directly
            rb.linearVelocity = -launchVelocity;
            
            Debug.Log($"Elastic force: {elasticForce:F1}, Tension: {currentTension:F1}, Total force: {totalForce:F1}, Distance stretch: {distanceStretch:F1}");
        }
        
        isGrappling = false;
        currentTension = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            GameManager.Instance.PlayerDied();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // Draw grappling range
        Gizmos.color = isGrappling ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grapplingRange);
        
        // Draw grapple connection and tension if grappling
        if (Application.isPlaying && isGrappling)
        {
            Gizmos.color = Color.Lerp(Color.yellow, Color.red, currentTension / maxTension);
            Gizmos.DrawLine(transform.position, grappleAnchor);
            
            // Draw launch direction arrow from grapple point
            float arrowLength = (currentTension / maxTension) * 4f; // Max 4 units long
            Vector2 arrowEnd2D = grappleAnchor + currentLaunchDirection * arrowLength;
            Vector3 arrowStart3D = new Vector3(grappleAnchor.x, grappleAnchor.y, 0f);
            Vector3 arrowEnd3D = new Vector3(arrowEnd2D.x, arrowEnd2D.y, 0f);
            // Draw arrow shaft
            Gizmos.color = Color.Lerp(Color.white, Color.red, currentTension / maxTension);
            Gizmos.DrawLine(arrowStart3D, arrowEnd3D);
            
            // Draw arrow head (simple V shape)
            if (arrowLength > 0.5f)
            {
                Vector3 arrowHeadSize = (Vector3)(currentLaunchDirection * 0.3f);
                Vector3 perpendicular = new Vector3(-currentLaunchDirection.y, currentLaunchDirection.x, 0) * 0.2f;
                Gizmos.DrawLine(arrowEnd3D, arrowEnd3D - arrowHeadSize + perpendicular);
                Gizmos.DrawLine(arrowEnd3D, arrowEnd3D - arrowHeadSize - perpendicular);
            }
            
            // Draw launch direction from player (original blue ray)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, currentLaunchDirection * 2f);
            
            // Draw tension level as a circle around grapple anchor
            Gizmos.color = Color.Lerp(Color.green, Color.red, currentTension / maxTension);
            Gizmos.DrawWireSphere(grappleAnchor, 0.5f + (currentTension / maxTension) * 0.5f);
            
        }
    }
}