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
