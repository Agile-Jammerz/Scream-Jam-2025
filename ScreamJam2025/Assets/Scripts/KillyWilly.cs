using UnityEngine;
using System.Collections;

public class KillyWilly : MonoBehaviour
{
    [Header("KillyWilly Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("KillyWilly Victim")]
    [SerializeField] private Transform player;
    
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float attackDuration = 1f;
    
    private bool isAttacking = false;
    
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
    }

    void Update()
    {
        if (player != null)
        {
            MoveTowardsPlayer();
        }
    }
    
    private void MoveTowardsPlayer()
    {
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isAttacking)
        {
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
        
        // Call the game manager to handle player loss
        GameManager.Instance.PlayerLose();
        
        // Reset the attacking state
        isAttacking = false;
    }

}
