using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseMenuUI;
    public bool isGamePaused = false;
    
    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;
    
    void Start()
    {
        // Ensure the game starts unpaused
        ResumeGame();
    }
    
    void Update()
    {
        // Check for pause input
        if (Input.GetKeyDown(pauseKey))
        {
            if (isGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene("Cinematic");
    }
    
    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f; // Pause the game
        pauseMenuUI.SetActive(true); // Show pause menu
        Cursor.lockState = CursorLockMode.None; // Unlock cursor
        Cursor.visible = true; // Show cursor
    }
    
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f; // Resume normal time
        pauseMenuUI.SetActive(false); // Hide pause menu
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor for FPS controls
        Cursor.visible = false; // Hide cursor
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene("MainMenu"); // Load main menu scene (adjust scene name as needed)
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
