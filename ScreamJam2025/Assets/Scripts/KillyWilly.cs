using UnityEngine;

public class KillyWilly : MonoBehaviour
{
    [Header("KillyWilly Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("KillyWilly Victim")]
    [SerializeField] private Transform player;
    
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

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Killy collide");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Killy collide");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Killy collide");
        }
    }

}
