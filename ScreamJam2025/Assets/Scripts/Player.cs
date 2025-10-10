using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("PlayerMovement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float boostSpeed = 10f;
    [SerializeField] private float pukingTime = 2f;
    private float drunkennessMeter = 0f;
    private float maxDrunkenness = 500f;
    private bool isPuking = false;

    void Start()
    {
        
    }

    void Update()
    {
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
            HandleMovement();
        }
    }
    
    private void HandleMovement()
    {
        // Get input from WASD keys
        float horizontal = Input.GetAxis("Horizontal"); // A/D keys for strafing left/right
        float vertical = Input.GetAxis("Vertical");     // W/S keys for forward/backward movement
        
        // Create movement vector relative to player's current rotation
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;
        
        if (Input.GetKey(KeyCode.Space))
        {
            // Apply boost movement
            transform.position += movement * boostSpeed * Time.deltaTime;
            drunkennessMeter += 1;
        }
        else
        {
            // Apply movement
            transform.position += movement * moveSpeed * Time.deltaTime;
        }
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
}
