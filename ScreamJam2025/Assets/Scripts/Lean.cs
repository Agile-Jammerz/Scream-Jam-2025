using UnityEngine;

public class DrunkLean : MonoBehaviour
{
    [Header("Lean Settings")]
    public float leanAmplitude = 15f;   // max degrees to lean left/right
    public float leanSpeed = 5f;        // how fast to lean in/out
    public float recoverySpeed = 3f;    // how quickly it returns upright when no input

    [Header("Input Settings")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    private Quaternion baseRotation;
    private float currentLeanAngle = 0f;
    private float targetLeanAngle = 0f;

    void Start()
    {
        baseRotation = transform.localRotation;
    }

    void Update()
    {
        HandleInput();
        UpdateLeanAnimation();
    }

    void HandleInput()
    {
        // Check for input and set target lean angle
        if (Input.GetKey(leftKey))
        {
            targetLeanAngle = leanAmplitude; // Lean left (positive Z rotation)
        }
        else if (Input.GetKey(rightKey))
        {
            targetLeanAngle = -leanAmplitude; // Lean right (negative Z rotation)
        }
        else
        {
            targetLeanAngle = 0f; // Return to upright position
        }
    }

    void UpdateLeanAnimation()
    {
        // Smoothly transition to target lean angle
        float speed = Input.GetKey(leftKey) || Input.GetKey(rightKey) ? leanSpeed : recoverySpeed;
        currentLeanAngle = Mathf.Lerp(currentLeanAngle, targetLeanAngle, Time.deltaTime * speed);
        
        // Apply the lean rotation
        Quaternion leanRotation = baseRotation * Quaternion.Euler(0f, 0f, currentLeanAngle);
        transform.localRotation = leanRotation;
    }
}