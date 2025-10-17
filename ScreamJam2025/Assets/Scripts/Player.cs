using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

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
    
    [Header("Victory Animation Settings")]
    
    [Tooltip("Duration for each victory animation before switching to the next one.")]
    [SerializeField] private float victoryAnimationDuration = 1.0f;

    [Header("Falling Animation Settings")]
    
    [Tooltip("Duration for the falling animation to complete.")]
    [SerializeField] private float fallingAnimationDuration = Mathf.PI;
    [Tooltip("Duration for the standing animation to complete after falling.")]
    [SerializeField] private float standingAnimationDuration = 1.0f;

    [Header("Particle Effects")]

    [Tooltip("The particle system that plays when the player is puking.")]
    [SerializeField] private ParticleSystem pukeParticleSystem;

    [Header("Audio Effects")]

    [Tooltip("The audio source that plays the puke sound effect.")]
    [SerializeField] private AudioSource pukeAudioSource;

    [Header("Camera Settings")]

    [Tooltip("The initial Cinemachine camera shown when game starts.")]
    [SerializeField] private CinemachineCamera initialCamera;
    [Tooltip("The main follow camera for gameplay.")]
    [SerializeField] private CinemachineCamera followCamera;
    [Tooltip("FOV increase amount when boosting.")]
    [SerializeField] private float fovIncrease = 10f;
    [Tooltip("Speed of FOV transition when boosting starts/stops.")]
    [SerializeField] private float fovTransitionSpeed = 5f;
    [Tooltip("Delay before switching from initial camera to follow camera (in seconds).")]
    [SerializeField] private float cameraSwitchDelay = 2f;

    private float drunkennessIncreaseRate = 1f;
    public float drunkennessMeter = 0f;
    private float drunkennessLevel = 0f;
    private bool isPuking = false;
    private float wobbleFrequencyIncreaseRate = 0.05f;
    private float wobbleFrequency;
    private float startAdder = 0;
    private float currentSpeed;
    private bool isSprinting = false;
    private bool isDead = false;
    private bool isVictory = false;

    public int candyCount = 0;
    private bool consumingCandy = false;
    private float candyDecreaseRate;

    public float baseX;
    private bool hasFallen = false;
    
    // FOV control variables
    private float originalFOV;
    private bool isFOVIncreased = false;
    private float targetFOV;
    private bool isTransitioningFOV = false;
    
    // Camera switching variables
    private bool hasSwitchedToFollowCamera = false;
    private CinemachineCamera currentActiveCamera;

    void Start()
    {
        candyDecreaseRate = candyRestoreMagnitude / candyEatingTime;
        wobbleFrequency = startingWobbleFrequency;
        currentSpeed = moveSpeed;
        /*pukingTime = Mathf.PI;*/
        baseX = transform.position.x;
        
        // Ensure puke particle system is stopped at start
        if (pukeParticleSystem != null)
        {
            pukeParticleSystem.Stop();
        }
        
        // Configure puke audio source to loop and ensure it's stopped at start
        if (pukeAudioSource != null)
        {
            pukeAudioSource.loop = true;
            pukeAudioSource.Stop();
        }
        
        // Initialize camera system
        InitializeCameraSystem();
        
        // Start camera switching coroutine
        StartCoroutine(CameraSwitchCoroutine());
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
        wobbleFrequency = Mathf.Min(wobbleFrequency + wobbleFrequencyIncreaseRate * Time.deltaTime, maximumWobbleFrequency);
        Debug.Log(wobbleFrequency);

        // Safety check: Ensure particle system and audio state matches isPuking state
        UpdatePukeParticleSystem();
        UpdatePukeAudio();

        /*if (isPuking)
        {
            return;
        }*/
        if (!isPuking && drunkennessMeter >= maxDrunkenness)
        {
            Debug.Log("Started Puking");
            StartPuking(pukingTime);
        }
        else if (!hasFallen && !isDead && !isVictory)
        {
            if (Input.GetKey(KeyCode.C) && !consumingCandy && candyCount > 0)
            {
                Debug.Log("Eating candy");
                EatCandy(candyEatingTime);
            }
            HandleMovement();
        }
        
        // Handle FOV transitions
        UpdateFOVTransition();
    }
    
    private void HandleMovement()
    {
        if (isPuking)
        {
            currentSpeed = pukeSpeed;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            // Activate boost movement
            currentSpeed = boostSpeed;
            isSprinting = true;
            
            // Increase FOV when boosting
            IncreaseFOV();
            
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
            
            // Reset FOV when not boosting
            ResetFOV();
            
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
        Vector3 movement = transform.right * horizontal * wobbleFrequency * strafingCoefficient + transform.forward * vertical;
        float wobbleX;
        if (wobbleFrequency < maximumWobbleFrequency)
        {
            wobbleX = Mathf.Cos((Time.time) * wobbleFrequency) * (Time.deltaTime * wobbleAmplitude * (wobbleFrequency + Mathf.Log(wobbleFrequency, 2)));
        } else
        {
            wobbleX = Mathf.Cos((Time.time) * wobbleFrequency) * (Time.deltaTime * wobbleAmplitude * wobbleFrequency);
        }
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
        Vector3 finalMovement = movement * currentSpeed * Time.deltaTime;

        // Apply movement
        transform.position += finalMovement + wobble;

        // Update animator parameters
        if (animator != null)
        {
            animator.SetBool("IsSprinting", isSprinting);
            animator.SetBool("IsDead", isDead);
        }

        if (Time.time > 3f)
        {
            if (transform.position.x < baseX - wobbleAmplitude * fallingThreshold || transform.position.x > baseX + wobbleAmplitude * fallingThreshold)
            {
                Fall(fallingAnimationDuration);
            }
        }

    }

    private void StartPuking(float duration)
    {
        isPuking = true;
        
        // Reset FOV when puking starts
        ResetFOV();
        
        // Update particle system and audio state
        UpdatePukeParticleSystem();
        UpdatePukeAudio();
        
        StartCoroutine(PukeCoroutine(duration));
    }

    private IEnumerator PukeCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        drunkennessMeter = 0f;
        isPuking = false;
        
        // Update particle system and audio state
        UpdatePukeParticleSystem();
        UpdatePukeAudio();
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
        
        // Trigger falling animation
        if (animator != null)
        {
            animator.SetBool("IsFalling", true);
            animator.SetBool("IsStanding", false);
        }
        
        StartCoroutine(FallCoroutine(duration));
    }

    private IEnumerator FallCoroutine(float duration)
    {
        // Wait for falling animation duration
        yield return new WaitForSeconds(duration);
        
        Debug.Log("Falling animation finished, transitioning to standing...");
        
        // Transition to standing animation
        if (animator != null)
        {
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsStanding", true);
            Debug.Log("Set IsFalling=false, IsStanding=true");
        }
        else
        {
            Debug.LogError("Animator is null!");
        }
        
        // Wait for standing animation to play
        yield return new WaitForSeconds(standingAnimationDuration);
        
        Debug.Log("Standing animation finished, returning to default state...");
        
        // Reset to default state
        if (animator != null)
        {
            animator.SetBool("IsStanding", false);
            Debug.Log("Set IsStanding=false");
        }

        baseX = transform.position.x;
        UIManager.Instance.ResetBalanceBar();
        hasFallen = false;
        /*drunkennessIncreaseRate = drunkennessIncreaseRate * 1.5f;*/
        startAdder += 0.05f;
        wobbleFrequency = Mathf.Min(startingWobbleFrequency + startAdder, startingWobbleFrequency + 0.3f);
    }

    private void StartVictoryAnimation()
    {
        isVictory = true;
        Debug.Log("Starting victory animation sequence!");
        
        // Reset movement-related variables
        isSprinting = false;
        currentSpeed = 0f; // Stop movement completely
        
        // Clear all other animation states to prevent conflicts
        if (animator != null)
        {
            // Reset all movement-related animations
            animator.SetBool("IsSprinting", false);
            animator.SetBool("IsFalling", false);
            animator.SetBool("IsStanding", false);
            
            // Start with first jump animation
            animator.SetBool("VictoryJump1", true);
            animator.SetBool("VictoryJump2", false);
            
            Debug.Log("Cleared all other animation states and set VictoryJump1=true");
        }
        
        StartCoroutine(VictoryAnimationCoroutine());
    }

    private IEnumerator VictoryAnimationCoroutine()
    {
        bool useFirstJumpAnimation = true;
        
        while (isVictory)
        {
            // Wait for the animation duration
            yield return new WaitForSeconds(victoryAnimationDuration);
            
            // Switch to the other animation
            useFirstJumpAnimation = !useFirstJumpAnimation;
            
            // Toggle between the two jumping animations
            if (animator != null)
            {
                if (useFirstJumpAnimation)
                {
                    animator.SetBool("VictoryJump2", false);
                    animator.SetBool("VictoryJump1", true);
                }
                else
                {
                    animator.SetBool("VictoryJump1", false);
                    animator.SetBool("VictoryJump2", true);
                }
                Debug.Log($"Switched to victory animation: {(useFirstJumpAnimation ? "Jump1" : "Jump2")}");
            }
        }
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
        
        // Stop puke particle system and audio when player dies
        UpdatePukeParticleSystem();
        UpdatePukeAudio();
        
        Debug.Log("Player has died! Game Over.");
    }

    private void UpdatePukeParticleSystem()
    {
        if (pukeParticleSystem != null)
        {
            if (isPuking)
            {
                if (!pukeParticleSystem.isPlaying)
                {
                    pukeParticleSystem.Play();
                }
            }
            else
            {
                if (pukeParticleSystem.isPlaying)
                {
                    pukeParticleSystem.Stop();
                }
            }
        }
    }

    private void UpdatePukeAudio()
    {
        if (pukeAudioSource != null)
        {
            if (isPuking)
            {
                if (!pukeAudioSource.isPlaying)
                {
                    pukeAudioSource.Play();
                }
            }
            else
            {
                if (pukeAudioSource.isPlaying)
                {
                    pukeAudioSource.Stop();
                }
            }
        }
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
            
            // Start victory animation before calling PlayerWin
            if (!isVictory && !isDead)
            {
                StartVictoryAnimation();
            }
            
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
    
    private void InitializeCameraSystem()
    {
        // Set initial camera as active
        if (initialCamera != null)
        {
            initialCamera.Priority = 10;
            currentActiveCamera = initialCamera;
        }
        
        // Set follow camera as inactive initially
        if (followCamera != null)
        {
            followCamera.Priority = 0;
        }
        
        // Initialize FOV control with the active camera
        if (currentActiveCamera != null)
        {
            originalFOV = currentActiveCamera.Lens.FieldOfView;
            targetFOV = originalFOV;
        }
    }
    
    private IEnumerator CameraSwitchCoroutine()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(cameraSwitchDelay);
        
        // Switch to follow camera
        SwitchToFollowCamera();
    }
    
    private void SwitchToFollowCamera()
    {
        if (followCamera != null && !hasSwitchedToFollowCamera)
        {
            // Switch camera priorities
            if (initialCamera != null)
            {
                initialCamera.Priority = 0;
            }
            followCamera.Priority = 10;
            
            // Update current active camera
            currentActiveCamera = followCamera;
            
            // Update FOV control with new camera
            originalFOV = followCamera.Lens.FieldOfView;
            targetFOV = originalFOV;
            
            hasSwitchedToFollowCamera = true;
            
            Debug.Log("Switched to follow camera");
        }
    }
    
    private void IncreaseFOV()
    {
        if (currentActiveCamera != null && !isFOVIncreased)
        {
            targetFOV = originalFOV + fovIncrease;
            isFOVIncreased = true;
            isTransitioningFOV = true;
        }
    }
    
    private void ResetFOV()
    {
        if (currentActiveCamera != null && isFOVIncreased)
        {
            targetFOV = originalFOV;
            isFOVIncreased = false;
            isTransitioningFOV = true;
        }
    }
    
    private void UpdateFOVTransition()
    {
        if (currentActiveCamera != null && isTransitioningFOV)
        {
            float currentFOV = currentActiveCamera.Lens.FieldOfView;
            float newFOV = Mathf.Lerp(currentFOV, targetFOV, fovTransitionSpeed * Time.deltaTime);
            currentActiveCamera.Lens.FieldOfView = newFOV;
            
            // Check if we're close enough to the target to stop transitioning
            if (Mathf.Abs(newFOV - targetFOV) < 0.1f)
            {
                currentActiveCamera.Lens.FieldOfView = targetFOV;
                isTransitioningFOV = false;
            }
        }
    }
}
