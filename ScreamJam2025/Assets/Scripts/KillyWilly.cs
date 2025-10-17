using UnityEngine;
using System.Collections;

public class KillyWilly : MonoBehaviour
{
    [Header("KillyWilly Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("KillyWilly Victim")]
    [SerializeField] private Transform player;
    
    [Header("Collision Detection")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private bool useDistanceDetection = true;
    
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private float danceDuration = 2f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip danceSound;
    
    private bool isAttacking = false;
    private bool isDancing = false;
    
    void Start()
    {
        // If player isn't assigned, try to find it by tag
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("Killy Willy: Player not found! Make sure the player has the 'Player' tag.");
            }
        }
        
        // If animator isn't assigned, try to get it from this GameObject
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("Killy Willy: Animator component not found! Make sure to assign an Animator component.");
            }
        }
        
        // If audio source isn't assigned, try to get it from this GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("Killy Willy: AudioSource component not found! Make sure to assign an AudioSource component.");
            }
        }
    }

    void Update()
    {
        // Check if game is still active before moving
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null! Make sure GameManager is in the scene.");
            return;
        }
        
        if (!GameManager.Instance.isGameActive)
        {
            Debug.Log("KillyWilly: Game is not active, stopping movement");
            return;
        }
        
        if (player != null)
        {
            // Debug.Log("KillyWilly: Moving towards player - isGameActive = " + GameManager.Instance.isGameActive);
            MoveTowardsPlayer();
            
            // Distance-based collision detection as backup
            if (useDistanceDetection && !isAttacking)
            {
                CheckDistanceToPlayer();
            }
        }
    }
    
    private void MoveTowardsPlayer()
    {
        // Don't move or rotate when dancing
        if (isDancing)
        {
            return;
        }
        
        // Calculate direction to the player
        Vector3 direction = (player.position - transform.position).normalized;
        
        // Move towards the player at a constant speed
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Make Killy Willy face the player
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }
    
    private void CheckDistanceToPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            Debug.Log("KillyWilly: Player within attack range (" + distanceToPlayer.ToString("F2") + " units)");
            HandlePlayerCollision(player.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        HandlePlayerCollision(other.gameObject);
    }
    
    void OnCollisionStay(Collision collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }
    
    void OnTriggerStay(Collider other)
    {
        HandlePlayerCollision(other.gameObject);
    }
    
    private void HandlePlayerCollision(GameObject other)
    {
        if (other.CompareTag("Player") && !isAttacking && GameManager.Instance.isGameActive)
        {
            Debug.Log("KillyWilly: Player collision detected via " + (other.GetComponent<Collider>().isTrigger ? "Trigger" : "Collision"));
            StartAttack();
        }
    }
    
    private void StartAttack()
    {
        isAttacking = true;
        
        // Trigger the attack animation
        if (animator != null)
        {
            animator.SetBool("IsAttacking", true);
        }
        
        // Play attack sound once
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
            Debug.Log("KillyWilly: Playing attack sound");
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("Killy Willy: AudioSource not found, cannot play attack sound");
        }
        else if (attackSound == null)
        {
            Debug.LogWarning("Killy Willy: Attack sound clip not assigned");
        }
        
        // Start the attack duration coroutine
        StartCoroutine(AttackSequence());
    }
    
    private IEnumerator AttackSequence()
    {
        // Wait for the attack duration
        yield return new WaitForSeconds(attackDuration);
        
        // Reset the attack animation
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
        
        // Start dancing animation
        StartDancing();
        
        // Wait for the dance duration
        yield return new WaitForSeconds(danceDuration);
        
        // Stop dancing animation
        StopDancing();
        
        // Call the game manager to handle player loss
        Debug.Log("KillyWilly: About to call PlayerLose()");
        GameManager.Instance.PlayerLose();
        Debug.Log("KillyWilly: PlayerLose() called, isGameActive = " + GameManager.Instance.isGameActive);
        
        // Reset the attacking state
        isAttacking = false;
    }
    
    private void StartDancing()
    {
        isDancing = true;
        
        // Trigger the dancing animation
        if (animator != null)
        {
            animator.SetBool("IsDancing", true);
            Debug.Log("KillyWilly: Started dancing animation");
        }
        else
        {
            Debug.LogWarning("Killy Willy: Animator not found, cannot start dancing animation");
        }
        
        // Play dance sound once
        if (audioSource != null && danceSound != null)
        {
            audioSource.PlayOneShot(danceSound);
            Debug.Log("KillyWilly: Playing dance sound");
        }
        else if (audioSource == null)
        {
            Debug.LogWarning("Killy Willy: AudioSource not found, cannot play dance sound");
        }
        else if (danceSound == null)
        {
            Debug.LogWarning("Killy Willy: Dance sound clip not assigned");
        }
    }
    
    private void StopDancing()
    {
        isDancing = false;
        
        // Stop the dancing animation
        if (animator != null)
        {
            animator.SetBool("IsDancing", false);
            Debug.Log("KillyWilly: Stopped dancing animation");
        }
        else
        {
            Debug.LogWarning("Killy Willy: Animator not found, cannot stop dancing animation");
        }
    }

}
