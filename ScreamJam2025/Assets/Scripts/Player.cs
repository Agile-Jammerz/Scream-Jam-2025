using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("PlayerMovement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float boostSpeed = 10f;
    [SerializeField] private float pukingTime = 2f;
    [SerializeField] private float wobbleMultiplier = 1f;
    [SerializeField] private float maxDrunkenness = 15f;
    private float drunkennessIncreaseRate = 1f;
    private float drunkennessMeter = 0f;
    private float drunkennessLevel = 0f;
    private bool isPuking = false;

    void Start()
    {
        
    }

    void Update()
    {
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
            drunkennessMeter += drunkennessIncreaseRate * Time.deltaTime;
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


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            GameManager.Instance.PlayerWin();
        }
    }
}
