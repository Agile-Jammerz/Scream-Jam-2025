using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Player Movement Settings")]

    [Tooltip("This value controls how fast the player moves by default.")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("This value controls how fast the player moves with the drinking boost.")]
    [SerializeField] private float boostSpeed = 10f;
    [Tooltip("This value controls how fast the player moves when puking.")]
    [SerializeField] private float pukeSpeed = 1f;

    [Header("Drunkenness Settings")]

    [Tooltip("This value controls how long the player spends puking.")]
    [SerializeField] private float pukingTime = 2f;
    [Tooltip("This value controls the wobble amplitude.")]
    [SerializeField] public float wobbleAmplitude = 5f;
    [Tooltip("This value controls the starting wobble frequency.")]
    [SerializeField] public float startingWobbleFrequency = 1f;
    [Tooltip("This value controls the maximum wobble frequency.")]
    [SerializeField] public float maximumWobbleFrequency = 3f;
    [Tooltip("This value is the time in seconds of holding spacebar that results in puking.")]
    [SerializeField] public float maxDrunkenness = 15f;
    [Tooltip("This value is what to multiply the amplitude by to mark the bounds for which the player will fall.")]
    [SerializeField] public float fallingThreshold = 0.8f;
    [Tooltip("This value is what to multiply the strength of the strafing movement.")]
    [SerializeField] public float strafingCoefficient = 0.8f;

    [Header("Candy Settings")]

    [Tooltip("This value controls how long, in seconds, the player spends eating a piece of candy.")]
    [SerializeField] private float candyEatingTime = 1f;
    [Tooltip("This value controls how much a piece of candyy decreases drunkenness.")]
    [SerializeField] private float candyRestoreMagnitude = 3f;

    [Header("Animation")]

    [Tooltip("The animator component for player animations.")]
    [SerializeField] private Animator animator;
    [Tooltip("Avatar mask for drinking animation - should only affect torso, arms, and head.")]
    [SerializeField] private AvatarMask drinkingAvatarMask;

    [Header("Particle Effects")]

    [Tooltip("The particle system that plays when the player is puking.")]
    [SerializeField] private ParticleSystem pukeParticleSystem;

    private float drunkennessIncreaseRate = 1f;
    public float drunkennessMeter = 0f;
    private float drunkennessLevel = 0f;
    private bool isPuking = false;
    private float wobbleFrequencyIncreaseRate = 0.05f;
    private float wobbleFrequency;
    private float currentSpeed;
    private bool isSprinting = false;
    private bool isDead = false;
    private bool isDrinking = false;

    public int candyCount = 0;
    private bool consumingCandy = false;
    private float candyDecreaseRate;

    public float baseX;
    private bool hasFallen = false;
    private float fallTime = Mathf.PI;

    void Start()
    {
        candyDecreaseRate = candyRestoreMagnitude / candyEatingTime;
        wobbleFrequency = startingWobbleFrequency;
        currentSpeed = moveSpeed;
        /*pukingTime = Mathf.PI;*/
        baseX = transform.position.x;
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
            // Debug.Log("Player: Moving - isGameActive = " + GameManager.Instance.isGameActive);
        }
        
        drunkennessLevel = drunkennessMeter / maxDrunkenness;
        wobbleFrequency = Mathf.Min(wobbleFrequency + (wobbleFrequencyIncreaseRate * Time.deltaTime), maximumWobbleFrequency);

        /*if (isPuking)
        {
            return;
        }*/
        if (!isPuking && drunkennessMeter >= maxDrunkenness)
        {
            Debug.Log("Started Puking");
            StartPuking(pukingTime);
        }
        else if (!hasFallen && !isDead)
        {
            if (Input.GetKey(KeyCode.C) && !consumingCandy && candyCount > 0)
            {
                Debug.Log("Eating candy");
                EatCandy(candyEatingTime);
            }
            HandleMovement();
        }
    }
    
    private void HandleMovement()
    {
        if (isPuking)
        {
            currentSpeed = pukeSpeed;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            // Activate boost movement and drinking
            currentSpeed = boostSpeed;
            isSprinting = true;
            isDrinking = true;
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
        } else
        {
            currentSpeed = moveSpeed;
            isSprinting = false;
            isDrinking = false;
            if (consumingCandy)
            {
                drunkennessMeter = Mathf.Max(0, drunkennessMeter - candyDecreaseRate * Time.deltaTime);
            }
        }
        // Get input from WASD keys
        /*float horizontal = Input.GetAxis("Horizontal"); // A/D keys for strafing left/right
        float vertical = Input.GetAxis("Vertical");     // W/S keys for forward/backward movement*/

        // Constant Forward Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = 1;

        // Create movement vector relative to player's current rotation
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        float wobbleX = Mathf.Cos((Time.time + 0.0f * Mathf.PI) * wobbleFrequency) * (Time.deltaTime * wobbleAmplitude * wobbleFrequency);
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
        Vector3 finalMovement = movement * currentSpeed * wobbleFrequency * strafingCoefficient * Time.deltaTime;

        // Apply movement
        transform.position += finalMovement + wobble;

        // Update animator parameters
        if (animator != null)
        {
            animator.SetBool("IsSprinting", isSprinting);
            animator.SetBool("IsDead", isDead);
            animator.SetBool("IsDrinking", isDrinking);
        }

        if (Time.time > 3)
        {
            if (transform.position.x < baseX - wobbleAmplitude * fallingThreshold || transform.position.x > baseX + wobbleAmplitude * fallingThreshold)
            {
                Fall(fallTime);
            }
        }

    }

    private void StartPuking(float duration)
    {
        isPuking = true;
        
        // Start the puke particle system if it exists
        if (pukeParticleSystem != null)
        {
            pukeParticleSystem.Play();
        }
        
        StartCoroutine(PukeCoroutine(duration));
    }

    private IEnumerator PukeCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        // Stop the puke particle system if it exists
        if (pukeParticleSystem != null)
        {
            pukeParticleSystem.Stop();
        }
        
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

    private void Fall(float duration)
    {
        Debug.Log("Fallen");
        hasFallen = true;
        baseX = transform.position.x;
        StartCoroutine(FallCoroutine(duration));
    }

    private IEnumerator FallCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        UIManager.Instance.ResetBalanceBar();
        hasFallen = false;
        wobbleFrequency = startingWobbleFrequency;
    }

    private void PlayerDeath()
    {
        isDead = true;
        
        // Update animator to trigger death animation
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        
        // Stop all movement and interactions
        currentSpeed = 0f;
        isSprinting = false;
        isDrinking = false;
        
        Debug.Log("Player has died! Game Over.");
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
        else if (other.CompareTag("KillyWilly"))
        {
            Debug.Log("Player: Caught by Killy Willy!");
            if (!isDead)
            {
                PlayerDeath();
            }
        }
    }
}
