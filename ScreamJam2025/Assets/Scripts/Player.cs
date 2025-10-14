using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Player Movement Settings")]

    [Tooltip("This value controls how fast the player moves by default.")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("This value controls how fast the player moves with the drinking boost.")]
    [SerializeField] private float boostSpeed = 10f;

    [Header("Drunkenness Settings")]

    [Tooltip("This value controls how long the player spends puking.")]
    [SerializeField] private float pukingTime = 2f;
    [Tooltip("This value controls how much the wobble amplitude [-1, 1] should be multiplies by.")]
    [SerializeField] private float wobbleMultiplier = 1f;
    [Tooltip("This value is the time in seconds of holding spacebar that results in puking.")]
    [SerializeField] public float maxDrunkenness = 15f;

    [Header("Candy Settings")]

    [Tooltip("This value controls how long, in seconds, the player spends eating a piece of candy.")]
    [SerializeField] private float candyEatingTime = 1f;
    [Tooltip("This value controls how much a piece of candyy decreases drunkenness.")]
    [SerializeField] private float candyRestoreMagnitude = 3f;

    private float drunkennessIncreaseRate = 1f;
    public float drunkennessMeter = 0f;
    private float drunkennessLevel = 0f;
    private bool isPuking = false;

    public int candyCount = 0;
    private bool consumingCandy = false;
    private float candyDecreaseRate;

    void Start()
    {
        candyDecreaseRate = candyRestoreMagnitude / candyEatingTime;
    }

    void Update()
    {
        // Check if game is still active before processing any input
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null! Make sure GameManager is in the scene.");
            return;
        }
        
        if (!GameManager.Instance.isGameActive)
        {
            Debug.Log("Player: Game is not active, stopping movement");
            return;
        }
        
        // Debug: Show that Update is running and game is active
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            Debug.Log("Player: Moving - isGameActive = " + GameManager.Instance.isGameActive);
        }
        
        drunkennessLevel = drunkennessMeter / maxDrunkenness;
        if (isPuking)
        {
            return;
        }
        else if (drunkennessMeter >= maxDrunkenness)
        {
            Debug.Log("Started Puking");
            StartPuking(pukingTime);
        }
        else if (!isPuking)
        {
            if (Input.GetKey(KeyCode.C) && !consumingCandy)
            {
                Debug.Log("Eating candy");
                EatCandy(candyEatingTime);
            }
            HandleMovement();
        }
    }
    
    private void HandleMovement()
    {
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.Space))
        {
            // Activate boost movement
            currentSpeed = boostSpeed;
            if (!consumingCandy)
            {
                drunkennessMeter += drunkennessIncreaseRate * Time.deltaTime;
            }
            else
            {
                float drunkennessDifference = (drunkennessIncreaseRate - candyDecreaseRate) * Time.deltaTime;
                if (drunkennessMeter + drunkennessDifference <= 0)
                {
                    drunkennessMeter = 0;
                }
                else
                {
                    drunkennessMeter += drunkennessDifference;
                }
            }
        } else if (consumingCandy)
        {
            drunkennessMeter -= candyDecreaseRate * Time.deltaTime;
        }
        // Get input from WASD keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D keys for strafing left/right
        float vertical = Input.GetAxis("Vertical");     // W/S keys for forward/backward movement
        
        // Create movement vector relative to player's current rotation
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        float wobbleX = Mathf.Sin(Time.time * boostSpeed) * (drunkennessLevel * wobbleMultiplier);
        float wobbleY = 0f;
        float wobbleZ = 0f;
        if (drunkennessLevel > 0.1)
        {
            // Horizontal wobble
            // wobbleX = Mathf.Sin(Time.time * currentSpeed) * (drunkennessLevel * wobbleMultiplier);
        }
        if (drunkennessLevel > 0.5)
        {
            // Forward wobble
            // wobbleZ = Mathf.Cos(Time.time * currentSpeed) * wobbleAmplitude;
        }

        Vector3 wobble = new Vector3(wobbleX, wobbleY, wobbleZ);
        Vector3 finalMovement = wobble + movement;

        // Apply movement
        transform.position += finalMovement * currentSpeed * Time.deltaTime;

    }

    private void StartPuking(float duration)
    {
        isPuking = true;
        StartCoroutine(PukeCoroutine(duration));
    }

    private IEnumerator PukeCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        drunkennessMeter = 0f;
        isPuking = false;
    }

    private void EatCandy(float duration)
    {
        candyCount = Mathf.Max(0, candyCount - 1);
        consumingCandy = true;
        StartCoroutine(CandyCoroutine(duration));
    }

    private IEnumerator CandyCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        consumingCandy = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void HandleCollision(GameObject other)
    {
        if (other.CompareTag("Goal"))
        {
            Debug.Log("Player: Goal collision detected via " + (other.GetComponent<Collider>().isTrigger ? "Trigger" : "Collision"));
            GameManager.Instance.PlayerWin();
        }
        else if (other.CompareTag("Candy"))
        {
            Debug.Log("Player: Candy collision detected via " + (other.GetComponent<Collider>().isTrigger ? "Trigger" : "Collision"));
            Debug.Log("Collecting Candy");
            Destroy(other.gameObject);
            candyCount++;
        }
    }
}
