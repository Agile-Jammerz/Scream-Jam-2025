using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject loseScreen;
    public GameObject winScreen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayerLose()
    {
        loseScreen.SetActive(true);
    }

    public void PlayerWin()
    {
        winScreen.SetActive(true);
    }
}
