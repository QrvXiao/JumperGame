using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("UI")]
    public GameObject deathUI;
    public Text deathText;
    public GameObject victoryUI;
    public Text victoryText;
    
    [Header("Game Objects")]
    public GameObject player;
    public Vector3 playerSpawnPoint;
    
    private bool gameRunning = true;
    private bool gameWon = false;
    private Vector3 originalSpawnPoint;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            originalSpawnPoint = player.transform.position;
            playerSpawnPoint = originalSpawnPoint;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        deathUI.SetActive(false);
        if (victoryUI != null)
            victoryUI.SetActive(false);
        Time.timeScale = 1f;
    }
    
    void Update()
    {
        if (!gameRunning)
        {
            bool restart = false;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) restart = true;
            if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)) restart = true;
            if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) restart = true;

            if (restart)
            {
                if (gameWon)
                    FullReset();
                else
                    RestartGame();
            }
        }
    }
    
    public void PlayerDied()
    {
        gameRunning = false;
        gameWon = false;
        Time.timeScale = 0f;
        deathUI.SetActive(true);
        deathText.text = "You Died!\nPress any key to restart";
    }
    
    public void PlayerWon()
    {
        gameRunning = false;
        gameWon = true;
        Time.timeScale = 0f;
        
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            if (victoryText != null)
                victoryText.text = "Victory!\nPress any key to restart";
        }
    }
    
    public void SetSavePoint(Vector3 position)
    {
        playerSpawnPoint = position;
        Debug.Log($"Save point updated to {position}");
    }
    
    void RestartGame()
    {
        gameRunning = true;
        gameWon = false;
        Time.timeScale = 1f;
        deathUI.SetActive(false);
        
        // Reset player position to last save point
        player.transform.position = playerSpawnPoint;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        
        // Reset all gunners
        AutoGunner[] gunners = FindObjectsOfType<AutoGunner>();
        foreach (AutoGunner gunner in gunners)
        {
            gunner.ResetTimer();
        }
        
        // Clear all bullets
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }
    }
    
    void FullReset()
    {
        gameRunning = true;
        gameWon = false;
        Time.timeScale = 1f;
        
        // Hide all UI
        deathUI.SetActive(false);
        if (victoryUI != null)
            victoryUI.SetActive(false);
        
        // Reset spawn point to original
        playerSpawnPoint = originalSpawnPoint;
        player.transform.position = originalSpawnPoint;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        
        // Reset all save points
        SavePoint[] savePoints = FindObjectsOfType<SavePoint>();
        foreach (SavePoint sp in savePoints)
        {
            sp.Reset();
        }
        
        // Reactivate victory star
        VictoryStar[] stars = FindObjectsOfType<VictoryStar>(true);
        foreach (VictoryStar star in stars)
        {
            star.gameObject.SetActive(true);
        }
        
        // Reset all gunners
        AutoGunner[] gunners = FindObjectsOfType<AutoGunner>();
        foreach (AutoGunner gunner in gunners)
        {
            gunner.ResetTimer();
        }
        
        // Clear all bullets
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }
        
        Debug.Log("Full game reset");
    }
}