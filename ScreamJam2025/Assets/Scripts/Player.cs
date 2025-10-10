using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("PlayerMovement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
   
    void Start()
    {
        
    }

    void Update()
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        // Get input from WASD keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D keys for strafing left/right
        float vertical = Input.GetAxis("Vertical");     // W/S keys for forward/backward movement
        
        // Create movement vector relative to player's current rotation
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        
        // Apply movement
        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}
