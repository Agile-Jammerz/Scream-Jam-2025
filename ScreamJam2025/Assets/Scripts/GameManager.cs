using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject loseScreen;
    public GameObject winScreen;
    
    [Header("Game State")]
    public bool isGameActive = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayerLose()
    {
        Debug.Log("GameManager: PlayerLose() called - setting isGameActive to false");
        isGameActive = false;
        loseScreen.SetActive(true);
    }

    public void PlayerWin()
    {
        isGameActive = false;
        winScreen.SetActive(true);
    }
}
